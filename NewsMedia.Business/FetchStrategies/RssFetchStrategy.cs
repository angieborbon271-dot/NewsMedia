using Microsoft.Extensions.Http;

namespace NewsMedia.Business.FetchStrategies
{
    public class RssFetchStrategy : FeedFetchStrategyBase
    {
        public RssFetchStrategy(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }
        public override string ComponentType => "rss";
    }
}