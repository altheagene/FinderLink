using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAdminService _adminService;

        public LoginController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["HideSideBar"] = true;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            ViewData["HideSideBar"] = true;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isValid = await _adminService.ValidateLoginAsync(model.Username, model.Password);
            if (!isValid)
            {
                ModelState.AddModelError("LoginError", "Invalid username or password.");
                return View(model);
            }

            var admin = await _adminService.GetAdminByUsernameAsync(model.Username);
            HttpContext.Session.SetInt32("AdminId", admin!.AdminId);
            HttpContext.Session.SetString("AdminUsername", admin.Username);
            HttpContext.Session.SetString("AdminName", admin.Name);

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
