using Microsoft.EntityFrameworkCore;
using NewsMedia.Data;
using NewsMedia.Models;

namespace NewsMedia.Repositories
{
    public class SourceItemRepository : ISourceItemRepository
    {
        private readonly AppDbContext _db;
        public SourceItemRepository(AppDbContext db) => _db = db;

        public async Task<List<SourceItem>> GetAllAsync() =>
            await _db.SourceItems.ToListAsync();

        public async Task<List<SourceItem>> GetByUserAsync(string userId) =>
            await _db.SourceItems
                .Where(i => i.SavedBy == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

        public async Task<List<SourceItem>> GetBySourceAsync(int sourceId) =>
            await _db.SourceItems
                .Where(i => i.SourceId == sourceId)
                .ToListAsync();

        public async Task<SourceItem> CreateAsync(SourceItem item)
        {
            _db.SourceItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.SourceItems.FindAsync(id);
            if (item == null) return false;
            _db.SourceItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}