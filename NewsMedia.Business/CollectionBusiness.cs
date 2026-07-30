using NewsMedia.Models;
using NewsMedia.Repositories;

namespace NewsMedia.Business
{
	public class CollectionBusiness : ICollectionBusiness
	{
		private readonly ICollectionRepository _repo;

		public CollectionBusiness(ICollectionRepository repo)
		{
			_repo = repo;
		}

		public Task<List<Collection>> GetByOwnerAsync(string ownerId) => _repo.GetByOwnerAsync(ownerId);
		public Task<Collection?> GetByIdAsync(int id, string ownerId) => _repo.GetByIdAsync(id, ownerId);
		public Task<Collection> CreateAsync(Collection collection) => _repo.CreateAsync(collection);
		public Task<bool> DeleteAsync(int id, string ownerId) => _repo.DeleteAsync(id, ownerId);

		public Task<bool> AssignItemAsync(int collectionId, int sourceItemId, string ownerId)
			=> _repo.AssignItemAsync(collectionId, sourceItemId, ownerId);

		public Task<bool> RemoveItemAsync(int sourceItemId, string ownerId)
			=> _repo.RemoveItemAsync(sourceItemId, ownerId);
	}
}