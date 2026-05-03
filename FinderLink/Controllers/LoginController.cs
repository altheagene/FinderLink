using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
