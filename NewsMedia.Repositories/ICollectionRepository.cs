using NewsMedia.Models;

namespace NewsMedia.Repositories
{
	public interface ICollectionRepository
	{
		Task<List<Collection>> GetByOwnerAsync(string ownerId);
		Task<Collection?> GetByIdAsync(int id, string ownerId);
		Task<Collection> CreateAsync(Collection collection);
		Task<bool> DeleteAsync(int id, string ownerId);
		Task<bool> AssignItemAsync(int collectionId, int sourceItemId, string ownerId);
		Task<bool> RemoveItemAsync(int sourceItemId, string ownerId);
	}
}