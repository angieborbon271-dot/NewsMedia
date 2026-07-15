using NewsMedia.Models;

namespace NewsMedia.Repositories
{
    public interface ISourceItemRepository
    {
        Task<List<SourceItem>> GetAllAsync();
        Task<List<SourceItem>> GetByUserAsync(string userId);
        Task<List<SourceItem>> GetBySourceAsync(int sourceId);
        Task<SourceItem> CreateAsync(SourceItem item);
        Task<bool> DeleteAsync(int id);
    }
}
