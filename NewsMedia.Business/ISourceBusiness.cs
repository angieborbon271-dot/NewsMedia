using NewsMedia.Models;

namespace NewsMedia.Business
{
    public interface ISourceBusiness
    {
        Task<List<Source>> GetAllAsync();
        Task<Source?> GetByIdAsync(int id);
        Task<Source> CreateAsync(Source source);
        Task<Source?> UpdateAsync(int id, Source source);
        Task<bool> DeleteAsync(int id);
        Task<List<Dictionary<string, string>>> FetchItemsAsync(Source source, string? secret = null);
    }
}
