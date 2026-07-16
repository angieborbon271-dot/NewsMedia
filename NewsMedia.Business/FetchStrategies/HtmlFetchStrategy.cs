using System.Text.RegularExpressions;
using Microsoft.Extensions.Http;

namespace NewsMedia.Business.FetchStrategies
{
    public class HtmlFetchStrategy : IFetchStrategy
    {
        private readonly HttpClient _http;
        public HtmlFetchStrategy(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("SourceFetch");
        }

        public string ComponentType => "html";

        public async Task<List<Dictionary<string, string>>> FetchAsync(string url, string? secret = null)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<Dictionary<string, string>>();

                var html = await response.Content.ReadAsStringAsync();
                var matches = Regex.Matches(html,
                    @"<a[^>]+href=""(https?://[^""]+)""[^>]*>(.*?)</a>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                return matches.Take(20).Select(m => new Dictionary<string, string>
                {
                    ["title"] = Regex.Replace(m.Groups[2].Value, "<.*?>", "").Trim(),
                    ["url"] = m.Groups[1].Value,
                    ["description"] = "",
                    ["publishedAt"] = "",
                    ["imageUrl"] = ""
                }).Where(i => !string.IsNullOrEmpty(i["title"])).ToList();
            }
            catch
            {
                return new List<Dictionary<string, string>>();
            }
        }
    }
}