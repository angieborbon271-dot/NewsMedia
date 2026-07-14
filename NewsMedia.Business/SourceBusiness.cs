using NewsMedia.Models;
using NewsMedia.Repositories;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NewsMedia.Business
{
    public class SourceBusiness : ISourceBusiness
    {
        private readonly ISourceRepository _repo;
        private readonly HttpClient _http = new HttpClient();

        public SourceBusiness(ISourceRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Source>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Source?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<Source> CreateAsync(Source source) => _repo.CreateAsync(source);
        public Task<Source?> UpdateAsync(int id, Source source) => _repo.UpdateAsync(id, source);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<List<Dictionary<string, string>>> FetchItemsAsync(Source source, string? secret = null)
        {
            return source.ComponentType.ToLower() switch
            {
                "rss" => await FetchRssAsync(source.Url),
                "json" => await FetchJsonAsync(source.Url, secret),
                "html" => await FetchHtmlAsync(source.Url),
                "api" => await FetchJsonAsync(source.Url, secret),
                _ => new List<Dictionary<string, string>>()
            };
        }

        private async Task<List<Dictionary<string, string>>> FetchRssAsync(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
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

        private string? ExtractImgFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return null;
            var match = Regex.Match(html, @"<img[^>]+src=""(https?://[^""]+)""", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private async Task<List<Dictionary<string, string>>> FetchJsonAsync(string url, string? secret)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(secret))
                    request.Headers.Add("Authorization", $"Bearer {secret}");

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new List<Dictionary<string, string>>();

                var json = await response.Content.ReadAsStringAsync();
                var root = System.Text.Json.JsonDocument.Parse(json).RootElement;

                // Si la respuesta es un array directo
                System.Text.Json.JsonElement array;
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    array = root;
                }
                else
                {
                    // Busca la primera propiedad 
                    var arrayProp = root.EnumerateObject()
                        .FirstOrDefault(p => p.Value.ValueKind == System.Text.Json.JsonValueKind.Array);
                    if (arrayProp.Value.ValueKind != System.Text.Json.JsonValueKind.Array)
                        return new List<Dictionary<string, string>>();
                    array = arrayProp.Value;
                }

                var result = new List<Dictionary<string, string>>();
                foreach (var item in array.EnumerateArray().Take(20))
                {
                    var dict = new Dictionary<string, string>();
                    foreach (var prop in item.EnumerateObject())
                    {
                        var key = prop.Name;
                        var val = prop.Value.ValueKind == System.Text.Json.JsonValueKind.String
                            ? prop.Value.GetString() ?? ""
                            : prop.Value.ToString();
                        dict[key] = val;
                    }

                    // Normaliza CAMPOS distintas APIS
                    if (!dict.ContainsKey("imageUrl"))
                        dict["imageUrl"] = dict.GetValueOrDefault("image", "")
                                        ?? dict.GetValueOrDefault("urlToImage", "")
                                        ?? dict.GetValueOrDefault("thumbnail", "")
                                        ?? "";
                    if (!dict.ContainsKey("description"))
                        dict["description"] = dict.GetValueOrDefault("content", "") ?? "";
                    if (!dict.ContainsKey("publishedAt"))
                        dict["publishedAt"] = dict.GetValueOrDefault("published_at", "")
                                           ?? dict.GetValueOrDefault("date", "") ?? "";

                    result.Add(dict);
                }
                return result;
            }
            catch
            {
                return new List<Dictionary<string, string>>();
            }
        }

        private async Task<List<Dictionary<string, string>>> FetchHtmlAsync(string url)
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