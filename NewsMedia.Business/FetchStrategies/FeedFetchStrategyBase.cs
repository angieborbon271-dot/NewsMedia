using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Http;

namespace NewsMedia.Business.FetchStrategies
{
    public abstract class FeedFetchStrategyBase : IFetchStrategy
    {
        protected readonly HttpClient Http;

        protected FeedFetchStrategyBase(IHttpClientFactory httpClientFactory)
        {
            Http = httpClientFactory.CreateClient();
        }

        public abstract string ComponentType { get; }

        public async Task<List<Dictionary<string, string>>> FetchAsync(string url, string? secret = null)
        {
            try
            {
                var response = await Http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<Dictionary<string, string>>();

                var xml = await response.Content.ReadAsStringAsync();
                var doc = XDocument.Parse(xml);

                XNamespace media = "http://search.yahoo.com/mrss/";
                XNamespace content = "http://purl.org/rss/1.0/modules/content/";

                return doc.Descendants("item")
                    .Take(20)
                    .Select(item =>
                    {
                        var imageUrl =
                            item.Element(media + "content")?.Attribute("url")?.Value
                            ?? item.Element(media + "thumbnail")?.Attribute("url")?.Value
                            ?? item.Element("enclosure")?.Attribute("url")?.Value
                            ?? ExtractImgFromHtml(item.Element(content + "encoded")?.Value ?? "")
                            ?? ExtractImgFromHtml(item.Element("description")?.Value ?? "");

                        return new Dictionary<string, string>
                        {
                            ["title"] = item.Element("title")?.Value ?? "",
                            ["url"] = item.Element("link")?.Value ?? "",
                            ["description"] = item.Element("description")?.Value ?? "",
                            ["publishedAt"] = item.Element("pubDate")?.Value ?? "",
                            ["imageUrl"] = imageUrl ?? ""
                        };
                    }).ToList();
            }
            catch
            {
                return new List<Dictionary<string, string>>();
            }
        }

        private static string? ExtractImgFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return null;
            var match = Regex.Match(html, @"<img[^>]+src=""(https?://[^""]+)""", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}