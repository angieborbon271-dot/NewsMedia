using NewsMedia.Models;

namespace NewsMedia.Repositories
{
    public interface ISettingRepository
    {
        Task<List<Setting>> GetAllAsync();
        Task<Setting?> GetByIdAsync(int id);
        Task<Setting?> GetByKeyAsync(string key);
        Task<Setting> CreateAsync(Setting setting);
        Task<Setting?> UpdateAsync(int id, Setting updated);
        Task<bool> DeleteAsync(int id);
    }
}
