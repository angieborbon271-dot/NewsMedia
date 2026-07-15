using Microsoft.AspNetCore.Mvc;
using NewsMedia.Models;
using System.Text.Json;

namespace NewsMedia.Mvc.Controllers
{
    public class SavedController : Controller
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public SavedController(IHttpClientFactory factory) => _http = factory.CreateClient("Api");

		public async Task<IActionResult> Index()
		{
			var userId = HttpContext.Session.GetString("UserId") ?? "anonymous";
			var items = await _http.GetFromJsonAsync<List<SourceItem>>(
				$"api/source-items/user/{userId}", _json) ?? new();

			// Si no tiene guardados, mostramos items de las fuentes como fallback
			List<Dictionary<string, string>> fallbackItems = new();
			if (!items.Any())
			{
				var sources = await _http.GetFromJsonAsync<List<Source>>("api/sources", _json) ?? new();
				foreach (var source in sources)
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
						fallbackItems.AddRange(fetched ?? new());
					}
					catch { }
				}
			}

			ViewBag.FallbackItems = fallbackItems;
			return View(items);
		}

		public async Task<IActionResult> Download()
        {
            var userId = HttpContext.Session.GetString("UserId") ?? "anonymous";
            var response = await _http.GetAsync($"api/source-items/export/{userId}");
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/json", "mis-noticias.json");
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var userId = HttpContext.Session.GetString("UserId") ?? "anonymous";
            using var form = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            form.Add(new StreamContent(stream), "file", file.FileName);

            var response = await _http.PostAsync($"api/source-items/import/{userId}", form);
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Artículos importados correctamente.";
            else
                TempData["Error"] = "Error al importar el archivo.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _http.DeleteAsync($"api/source-items/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}