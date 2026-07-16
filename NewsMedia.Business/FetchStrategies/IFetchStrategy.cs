using Microsoft.Extensions.Http;

namespace NewsMedia.Business.FetchStrategies
{
    public interface IFetchStrategy
    {
        string ComponentType { get; }
        Task<List<Dictionary<string, string>>> FetchAsync(string url, string? secret = null);
    }
}