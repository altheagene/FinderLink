using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class ManageItemsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
