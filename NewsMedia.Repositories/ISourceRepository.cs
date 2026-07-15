using NewsMedia.Models;

namespace NewsMedia.Repositories
{
    public interface ISourceRepository
    {
        Task<List<Source>> GetAllAsync();
        Task<Source?> GetByIdAsync(int id);
        Task<Source> CreateAsync(Source source);
        Task<Source?> UpdateAsync(int id, Source updated);
        Task<bool> DeleteAsync(int id);
    }
}
