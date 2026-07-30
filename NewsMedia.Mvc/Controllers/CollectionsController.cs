using Microsoft.AspNetCore.Mvc;
using NewsMedia.Models;
using System.Text.Json;

namespace NewsMedia.Mvc.Controllers
{
	public class CollectionsController : Controller
	{
		private readonly HttpClient _http;
		private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

		public CollectionsController(IHttpClientFactory factory) => _http = factory.CreateClient("Api");

		public async Task<IActionResult> Index()
		{
			var userId = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userId))
				return RedirectToAction("Login", "Auth");

			var collections = await _http.GetFromJsonAsync<List<Collection>>("api/collections", _json) ?? new();
			return View(collections);
		}

		[HttpPost]
		public async Task<IActionResult> Create(string name, string? description)
		{
			var userId = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userId))
				return RedirectToAction("Login", "Auth");

			if (string.IsNullOrWhiteSpace(name))
			{
				TempData["Error"] = "La carpeta necesita un nombre.";
				return RedirectToAction(nameof(Index));
			}

			await _http.PostAsJsonAsync("api/collections", new { name, description });
			TempData["Success"] = $"Carpeta «{name}» creada.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var userId = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userId))
				return RedirectToAction("Login", "Auth");

			await _http.DeleteAsync($"api/collections/{id}");
			TempData["Success"] = "Carpeta eliminada.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> RemoveItem(int sourceItemId)
		{
			var userId = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userId))
				return RedirectToAction("Login", "Auth");

			await _http.DeleteAsync($"api/collections/items/{sourceItemId}");
			TempData["Success"] = "Noticia quitada de la carpeta.";
			return RedirectToAction(nameof(Index));
		}
	}
}