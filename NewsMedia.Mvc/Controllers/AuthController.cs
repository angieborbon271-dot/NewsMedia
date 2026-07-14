using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NewsMedia.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _http;

        public AuthController(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("Api");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var body = JsonSerializer.Serialize(new { email, password });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            try
            {
                var response = await _http.PostAsync("api/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    var id = data.TryGetProperty("id", out var uid) ? uid.GetString() ?? "" : "";
                    var role = data.TryGetProperty("role", out var r) ? r.GetString() ?? "viewer" : "viewer";
                    var firstName = data.TryGetProperty("firstName", out var fn) ? fn.GetString() ?? "" : "";
                    var token = data.TryGetProperty("token", out var t) ? t.GetString() ?? "" : "";
                    HttpContext.Session.SetString("UserId", id);
                    HttpContext.Session.SetString("UserEmail", email);
                    HttpContext.Session.SetString("UserRole", role);
                    HttpContext.Session.SetString("UserName", firstName);
                    HttpContext.Session.SetString("Token", token);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch { }
            ViewBag.Error = "Correo o contraseña incorrectos";
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string firstName, string lastName)
        {
            var body = JsonSerializer.Serialize(new { email, password, firstName, lastName });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/auth/register", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "¡Cuenta creada con éxito! Inicia sesión.";
                return RedirectToAction("Login");
            }
            ViewBag.Error = "Error al registrarse. El correo puede estar en uso.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _http.PostAsync("api/auth/logout", null);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Las contraseñas nuevas no coinciden.";
                return View();
            }
            var body = JsonSerializer.Serialize(new
            {
                email = HttpContext.Session.GetString("UserEmail"),
                currentPassword,
                newPassword
            });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/auth/change-password", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Contraseña actualizada correctamente.";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Contraseña actual incorrecta.";
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/reset-password", new { email });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    ViewBag.Success = true;
                    ViewBag.TempPassword = result.GetProperty("tempPassword").GetString();
                }
                else
                {
                    ViewBag.Error = "No se encontró ninguna cuenta con ese correo.";
                }
            }
            catch
            {
                ViewBag.Error = "Error al conectar con el servidor. Intenta de nuevo.";
            }
            return View();
        }

    }  
}    