using Microsoft.EntityFrameworkCore;
using NewsMedia.Data;
using NewsMedia.Models;

namespace NewsMedia.Repositories
{
	public class CollectionRepository : ICollectionRepository
	{
		private readonly AppDbContext _context;

		public CollectionRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<List<Collection>> GetByOwnerAsync(string ownerId)
			=> await _context.Collections
				.Where(c => c.OwnerId == ownerId)
				.Include(c => c.Items)
				.OrderByDescending(c => c.CreatedAt)
				.ToListAsync();

		public async Task<Collection?> GetByIdAsync(int id, string ownerId)
			=> await _context.Collections
				.Include(c => c.Items)
				.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);

		public async Task<Collection> CreateAsync(Collection collection)
		{
			_context.Collections.Add(collection);
			await _context.SaveChangesAsync();
			return collection;
		}

		public async Task<bool> DeleteAsync(int id, string ownerId)
		{
			var collection = await _context.Collections.FindAsync(id);
			if (collection == null || collection.OwnerId != ownerId) return false;

			_context.Collections.Remove(collection);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> AssignItemAsync(int collectionId, int sourceItemId, string ownerId)
		{
			var collection = await _context.Collections.FindAsync(collectionId);
			if (collection == null || collection.OwnerId != ownerId) return false;

			var item = await _context.SourceItems.FindAsync(sourceItemId);
			if (item == null || item.SavedBy != ownerId) return false;

			item.CollectionId = collectionId;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> RemoveItemAsync(int sourceItemId, string ownerId)
		{
			var item = await _context.SourceItems.FindAsync(sourceItemId);
			if (item == null || item.SavedBy != ownerId) return false;

			item.CollectionId = null;
			await _context.SaveChangesAsync();
			return true;
		}
	}
}