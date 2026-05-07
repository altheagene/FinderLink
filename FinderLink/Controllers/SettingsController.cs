using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IAdminService _adminService;

        public SettingsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var admin = await _adminService.GetAdminByIdAsync(HttpContext.Session.GetInt32("AdminId")!.Value);
            if (admin == null)
            {
                return RedirectToAction("Logout", "Login");
            }

            var model = new SettingsViewModel
            {
                Name = admin.Name,
                Email = admin.Email,
                Username = admin.Username,
                Role = "Administrator"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(SettingsViewModel model)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var admin = await _adminService.GetAdminByIdAsync(HttpContext.Session.GetInt32("AdminId")!.Value);
            if (admin == null)
            {
                return RedirectToAction("Logout", "Login");
            }

            if (!ModelState.IsValid)
            {
                model.Role = "Administrator";
                return View("Index", model);
            }

            admin.Name = model.Name;
            admin.Email = model.Email;
            admin.Username = model.Username;
            await _adminService.UpdateAdminAsync(admin);
            TempData["SettingsMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(SettingsViewModel model)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var admin = await _adminService.GetAdminByIdAsync(HttpContext.Session.GetInt32("AdminId")!.Value);
            if (admin == null)
            {
                return RedirectToAction("Logout", "Login");
            }

            if (string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                TempData["SettingsError"] = "Please enter current and new password.";
                return RedirectToAction(nameof(Index));
            }

            if (admin.Password != model.CurrentPassword)
            {
                TempData["SettingsError"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Index));
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["SettingsError"] = "Password confirmation does not match.";
                return RedirectToAction(nameof(Index));
            }

            admin.Password = model.NewPassword;
            await _adminService.UpdateAdminAsync(admin);
            TempData["SettingsMessage"] = "Password updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
