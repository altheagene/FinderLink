using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
