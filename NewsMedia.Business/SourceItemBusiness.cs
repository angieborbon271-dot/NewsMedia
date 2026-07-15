using NewsMedia.Models;
using NewsMedia.Repositories;
using System.Text.Json;

namespace NewsMedia.Business
{
    public class SourceItemBusiness : ISourceItemBusiness
    {
        private readonly ISourceItemRepository _repo;

        public SourceItemBusiness(ISourceItemRepository repo)
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
			return JsonSerializer.Serialize(export, new JsonSerializerOptions
			{
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
		}

		// Importa items desde un JSON subido por el usuario
		// Importa items desde un JSON subido por el usuario
		public async Task<int> ImportFromJsonAsync(string jsonContent, string userId)
		{
			var items = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);
			if (items == null) return 0;

			int count = 0;
			foreach (var item in items)
			{
				var sourceId = TryGetPropertyCI(item, "sourceId", out var sid) ? sid.GetInt32() : 0;
				var json = TryGetPropertyCI(item, "json", out var j) ? (j.GetString() ?? "{}") : "{}";

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

		// Busca una propiedad sin importar mayúsculas/minúsculas (por si otro grupo
		// manda el JSON en un casing distinto al acordado)
		private static bool TryGetPropertyCI(JsonElement element, string propertyName, out JsonElement value)
		{
			foreach (var prop in element.EnumerateObject())
			{
				if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
				{
					value = prop.Value;
					return true;
				}
			}
			value = default;
			return false;
		}
	}
}