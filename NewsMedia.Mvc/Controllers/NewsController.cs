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

			// Si el usuario está logueado, traemos sus carpetas para poder elegir dónde guardar
			List<Collection> collections = new();
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
			{
				try
				{
					collections = await _http.GetFromJsonAsync<List<Collection>>("api/collections", _json) ?? new();
				}
				catch { }
			}

			ViewBag.Sources = sources;
			ViewBag.SelectedSource = sourceId;
			ViewBag.Collections = collections;
			return View(items);
		}

		[HttpPost]
		public async Task<IActionResult> Save([FromForm] int sourceId, [FromForm] string title,
	[FromForm] string url, [FromForm] string description,
	[FromForm] string publishedAt, [FromForm] string imageUrl, [FromForm] int? collectionId)
		{
			var userId = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userId))
			{
				TempData["Error"] = "Debes iniciar sesión para guardar artículos.";
				return RedirectToAction("Login", "Auth");
			}

			var item = new SourceItem
			{
				SourceId = sourceId,
				Json = JsonSerializer.Serialize(new { title, url, description, publishedAt, imageUrl }),
				SavedBy = userId,
				CreatedAt = DateTime.UtcNow
			};

			var response = await _http.PostAsJsonAsync("api/source-items", item);

			// Si eligió una carpeta, asignamos la noticia recién guardada usando el endpoint de Collections
			if (response.IsSuccessStatusCode && collectionId.HasValue && collectionId.Value > 0)
			{
				var created = await response.Content.ReadFromJsonAsync<SourceItem>(_json);
				if (created != null)
					await _http.PostAsync($"api/collections/{collectionId.Value}/items/{created.Id}", null);
			}

			TempData["Success"] = collectionId.HasValue && collectionId.Value > 0
				? "Artículo guardado en la carpeta."
				: "Artículo guardado.";
			return RedirectToAction(nameof(Index));
		}
	}
}