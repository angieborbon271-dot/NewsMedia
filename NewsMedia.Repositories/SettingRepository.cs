using Microsoft.EntityFrameworkCore;
using NewsMedia.Data;
using NewsMedia.Models;

namespace NewsMedia.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly AppDbContext _context;

        public SettingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Setting>> GetAllAsync()
            => await _context.Settings.ToListAsync();

        public async Task<Setting?> GetByIdAsync(int id)
            => await _context.Settings.FindAsync(id);

        public async Task<Setting?> GetByKeyAsync(string key)
            => await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);

        public async Task<Setting> CreateAsync(Setting setting)
        {
            _context.Settings.Add(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        public async Task<Setting?> UpdateAsync(int id, Setting updated)
        {
            var setting = await _context.Settings.FindAsync(id);
            if (setting == null) return null;
            setting.Key = updated.Key;
            setting.Value = updated.Value;
            setting.Description = updated.Description;
            setting.IsSecret = updated.IsSecret;
            await _context.SaveChangesAsync();
            return setting;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var setting = await _context.Settings.FindAsync(id);
            if (setting == null) return false;
            _context.Settings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}