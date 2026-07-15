using NewsMedia.Models;

namespace NewsMedia.Business
{
    public interface ISettingBusiness
    {
        Task<List<Setting>> GetAllAsync();
        Task<Setting?> GetByKeyAsync(string key);
        Task<Setting?> GetByIdAsync(int id);
        Task<Setting> CreateAsync(Setting setting);
        Task<Setting?> UpdateAsync(int id, Setting setting);
        Task<bool> DeleteAsync(int id);
        Task<string?> GetValueAsync(string key);
    }
}
