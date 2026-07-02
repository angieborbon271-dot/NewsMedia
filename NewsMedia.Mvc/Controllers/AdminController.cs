using Microsoft.AspNetCore.Mvc;
using NewsMedia.Models;
using System.Text.Json;

namespace NewsMedia.Mvc.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public AdminController(IHttpClientFactory factory) => _http = factory.CreateClient("Api");

        private IActionResult RedirectIfNotAdmin()
        {
            if (HttpContext.Session.GetString("UserRole") != "admin")
                return RedirectToAction("Index", "Home");
            return null!;
        }

        // ── FUENTES ──────────────────────────────────────────────
        public async Task<IActionResult> Sources()
        {
            var check = RedirectIfNotAdmin(); if (check != null) return check;
            var sources = await _http.GetFromJsonAsync<List<Source>>("api/sources", _json) ?? new();
            return View(sources);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSource(Source source)
        {
            await _http.PostAsJsonAsync("api/sources", source);
            TempData["Success"] = "Fuente creada correctamente.";
            return RedirectToAction(nameof(Sources));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSource(int id)
        {
            await _http.DeleteAsync($"api/sources/{id}");
            TempData["Success"] = "Fuente eliminada.";
            return RedirectToAction(nameof(Sources));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSource(int id, Source source)
        {
            await _http.PutAsJsonAsync($"api/sources/{id}", source);
            TempData["Success"] = "Fuente actualizada.";
            return RedirectToAction(nameof(Sources));
        }

        // ── CONFIGURACIÓN ─────────────────────────────────────────
        public async Task<IActionResult> Config()
        {
            var check = RedirectIfNotAdmin(); if (check != null) return check;
            var settings = await _http.GetFromJsonAsync<List<Setting>>("api/settings", _json) ?? new();
            var users = await _http.GetFromJsonAsync<List<JsonElement>>("api/users", _json) ?? new();
            ViewBag.Users = users;
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSetting(Setting setting)
        {
            await _http.PostAsJsonAsync("api/settings", setting);
            TempData["Success"] = "Secret guardado.";
            return RedirectToAction(nameof(Config));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSetting(int id)
        {
            await _http.DeleteAsync($"api/settings/{id}");
            TempData["Success"] = "Secret eliminado.";
            return RedirectToAction(nameof(Config));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string id, string role)
        {
            var content = new StringContent($"\"{role}\"", System.Text.Encoding.UTF8, "application/json");
            await _http.PutAsync($"api/users/{id}/role", content);
            TempData["Success"] = "Rol actualizado.";
            return RedirectToAction(nameof(Config));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _http.DeleteAsync($"api/users/{id}");
            TempData["Success"] = "Usuario eliminado.";
            return RedirectToAction(nameof(Config));
        }
    }
}
