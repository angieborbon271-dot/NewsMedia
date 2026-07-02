using Microsoft.AspNetCore.Mvc;

namespace NewsMedia.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}