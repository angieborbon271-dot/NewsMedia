using Microsoft.AspNetCore.Mvc;
using NewsMedia.Models;
using System.Text.Json;

namespace NewsMedia.Mvc.Controllers
{
    public class NewsController : Controller
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public NewsController(IHttpClientFactory factory) => _http = factory.CreateClient("Api");

        public async Task<IActionResult> Index(int? sourceId = null)
        {
            var sources = await _http.GetFromJsonAsync<List<Source>>("api/sources", _json) ?? new();
            List<Dictionary<string, string>> items = new();
            var sourcesToFetch = sourceId.HasValue
                ? sources.Where(s => s.Id == sourceId.Value).ToList()
                : sources;

            foreach (var source in sourcesToFetch)
            {
                try
                {
                    var url = $"api/sources/{source.Id}/items";
                    var fetched = await _http.GetFromJsonAsync<List<Dictionary<string, string>>>(url, _json);
                    if (fetched != null)
                        foreach (var item in fetched)
                        {
                            item["sourceName"] = source.Name;
                            item["sourceId"] = source.Id.ToString();
                        }
                    items.AddRange(fetched ?? new());
                }
                catch { }
            }

            ViewBag.Sources = sources;
            ViewBag.SelectedSource = sourceId;
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromForm] int sourceId, [FromForm] string title,
    [FromForm] string url, [FromForm] string description,
    [FromForm] string publishedAt, [FromForm] string imageUrl)
        {
            var userId = HttpContext.Session.GetString("UserId") ?? "anonymous";
            var item = new SourceItem
            {
                SourceId = sourceId,
                Json = JsonSerializer.Serialize(new { title, url, description, publishedAt, imageUrl }),
                SavedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _http.PostAsJsonAsync("api/source-items", item);
            TempData["Success"] = "Artículo guardado.";
            return RedirectToAction(nameof(Index));
        }
    }
}