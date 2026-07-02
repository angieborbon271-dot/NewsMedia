using NewsMedia.Models;
using NewsMedia.Repositories;
using System.Text.Json;

namespace NewsMedia.Business
{
    public class SourceItemBusiness
    {
        private readonly SourceItemRepository _repo;

        public SourceItemBusiness(SourceItemRepository repo)
        {
            _repo = repo;
        }

        public Task<List<SourceItem>> GetAllAsync() => _repo.GetAllAsync();
        public Task<SourceItem> CreateAsync(SourceItem item) => _repo.CreateAsync(item);
        public Task<List<SourceItem>> GetByUserAsync(string userId) => _repo.GetByUserAsync(userId);
        public Task<List<SourceItem>> GetBySourceAsync(int sourceId) => _repo.GetBySourceAsync(sourceId);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<SourceItem> SaveAsync(int sourceId, Dictionary<string, string> item, string savedBy)
        {
            var sourceItem = new SourceItem
            {
                SourceId = sourceId,
                Json = JsonSerializer.Serialize(item),
                SavedBy = savedBy,
                CreatedAt = DateTime.UtcNow
            };
            return await _repo.CreateAsync(sourceItem);
        }

        // Exporta los items guardados como JSON descargable
        public async Task<string> ExportToJsonAsync(string userId)
        {
            var items = await _repo.GetByUserAsync(userId);
            var export = items.Select(i => new
            {
                i.Id,
                i.SourceId,
                i.Json,
                i.SavedBy,
                i.CreatedAt
            });
            return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        }

        // Importa items desde un JSON subido por el usuario
        public async Task<int> ImportFromJsonAsync(string jsonContent, string userId)
        {
            var items = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);
            if (items == null) return 0;

            int count = 0;
            foreach (var item in items)
            {
                var sourceId = item.TryGetProperty("sourceId", out var sid) ? sid.GetInt32() : 0;
                var json = item.TryGetProperty("json", out var j) ? j.GetString() ?? "{}" : "{}";

                await _repo.CreateAsync(new SourceItem
                {
                    SourceId = sourceId,
                    Json = json,
                    SavedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
                count++;
            }
            return count;
        }
    }
}