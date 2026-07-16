using NewsMedia.Business.FetchStrategies;
using NewsMedia.Models;
using NewsMedia.Repositories;
using Microsoft.Extensions.Http;

namespace NewsMedia.Business
{
    public class SourceBusiness : ISourceBusiness
    {
        private readonly ISourceRepository _repo;
        private readonly Dictionary<string, IFetchStrategy> _strategies;

        public SourceBusiness(ISourceRepository repo, IEnumerable<IFetchStrategy> strategies)
        {
            _repo = repo;
            _strategies = strategies.ToDictionary(s => s.ComponentType.ToLower());
        }

        public Task<List<Source>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Source?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<Source> CreateAsync(Source source) => _repo.CreateAsync(source);
        public Task<Source?> UpdateAsync(int id, Source source) => _repo.UpdateAsync(id, source);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<List<Dictionary<string, string>>> FetchItemsAsync(Source source, string? secret = null)
        {
            var type = source.ComponentType.ToLower();
            if (!_strategies.TryGetValue(type, out var strategy))
                return new List<Dictionary<string, string>>();

            return await strategy.FetchAsync(source.Url, secret);
        }
    }
}