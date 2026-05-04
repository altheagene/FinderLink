using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class PublicController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
