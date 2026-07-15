using NewsMedia.Models;
using NewsMedia.Repositories;

namespace NewsMedia.Business
{
    public class SettingBusiness : ISettingBusiness
    {
        private readonly ISettingRepository _repo;

        public SettingBusiness(ISettingRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Setting>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Setting?> GetByKeyAsync(string key) => _repo.GetByKeyAsync(key);
        public Task<Setting?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<Setting> CreateAsync(Setting setting) => _repo.CreateAsync(setting);
        public Task<Setting?> UpdateAsync(int id, Setting setting) => _repo.UpdateAsync(id, setting);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<string?> GetValueAsync(string key)
        {
            var setting = await _repo.GetByKeyAsync(key);
            return setting?.Value;
        }
    }
}