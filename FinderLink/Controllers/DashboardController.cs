using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
