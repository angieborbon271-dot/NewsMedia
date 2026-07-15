using NewsMedia.Models;

namespace NewsMedia.Business
{
    public interface ISourceItemBusiness
    {
        Task<List<SourceItem>> GetAllAsync();
        Task<SourceItem> CreateAsync(SourceItem item);
        Task<List<SourceItem>> GetByUserAsync(string userId);
        Task<List<SourceItem>> GetBySourceAsync(int sourceId);
        Task<bool> DeleteAsync(int id);
        Task<SourceItem> SaveAsync(int sourceId, Dictionary<string, string> item, string savedBy);
        Task<string> ExportToJsonAsync(string userId);
        Task<int> ImportFromJsonAsync(string jsonContent, string userId);
    }
}
