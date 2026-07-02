using Microsoft.EntityFrameworkCore;
using NewsMedia.Data;
using NewsMedia.Models;

namespace NewsMedia.Repositories
{
    public class SourceRepository
    {
        private readonly AppDbContext _context;

        public SourceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Source>> GetAllAsync()
            => await _context.Sources.ToListAsync();

        public async Task<Source?> GetByIdAsync(int id)
            => await _context.Sources.FindAsync(id);

        public async Task<Source> CreateAsync(Source source)
        {
            _context.Sources.Add(source);
            await _context.SaveChangesAsync();
            return source;
        }

        public async Task<Source?> UpdateAsync(int id, Source updated)
        {
            var source = await _context.Sources.FindAsync(id);
            if (source == null) return null;
            source.Name = updated.Name;
            source.Url = updated.Url;
            source.ComponentType = updated.ComponentType;
            source.Description = updated.Description;
            await _context.SaveChangesAsync();
            return source;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var source = await _context.Sources.FindAsync(id);
            if (source == null) return false;
            _context.Sources.Remove(source);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}