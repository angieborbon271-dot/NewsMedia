using System.Text.Json;
using Microsoft.Extensions.Http;

namespace NewsMedia.Business.FetchStrategies
{
    public class JsonFetchStrategy : IFetchStrategy
    {
        private readonly HttpClient _http;
        public JsonFetchStrategy(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
        }

        public virtual string ComponentType => "json";

        public async Task<List<Dictionary<string, string>>> FetchAsync(string url, string? secret = null)
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
                var root = JsonDocument.Parse(json).RootElement;

                JsonElement array;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    array = root;
                }
                else
                {
                    var arrayProp = root.EnumerateObject()
                        .FirstOrDefault(p => p.Value.ValueKind == JsonValueKind.Array);
                    if (arrayProp.Value.ValueKind != JsonValueKind.Array)
                        return new List<Dictionary<string, string>>();
                    array = arrayProp.Value;
                }

                var result = new List<Dictionary<string, string>>();
                foreach (var item in array.EnumerateArray().Take(20))
                {
                    var dict = new Dictionary<string, string>();
                    foreach (var prop in item.EnumerateObject())
                    {
                        var val = prop.Value.ValueKind == JsonValueKind.String
                            ? prop.Value.GetString() ?? ""
                            : prop.Value.ToString();
                        dict[prop.Name] = val;
                    }

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
    }

    public class ApiFetchStrategy : JsonFetchStrategy
    {
        public ApiFetchStrategy(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }
        public override string ComponentType => "api";
    }
}