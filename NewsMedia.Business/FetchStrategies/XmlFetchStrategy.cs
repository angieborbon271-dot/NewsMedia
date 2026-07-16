using Microsoft.Extensions.Http;

namespace NewsMedia.Business.FetchStrategies
{
    public class XmlFetchStrategy : FeedFetchStrategyBase
    {
        public XmlFetchStrategy(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }
        public override string ComponentType => "xml";
    }
}