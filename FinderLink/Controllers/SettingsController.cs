using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILocationService _locationService;
        private readonly ICategoryService _categoryService;

        public SettingsController(IAdminService adminService, ILocationService locationService, ICategoryService categoriesService)
        {
            _adminService = adminService;
            _locationService = locationService;
            _categoryService = categoriesService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "profile")
        {
            ViewBag.ActiveTab = tab;

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
                Locations = await _locationService.GetAllAsync(),
                Categories = await _categoryService.GetAllAsync(),
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
            HttpContext.Session.SetString("AdminUsername", admin.Username);
            HttpContext.Session.SetString("AdminName", admin.Name);

            await _adminService.UpdateAdminAsync(admin);
            TempData["SettingsMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index), new { tab = "profile" });
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
            return RedirectToAction(nameof(Index), new { tab = "security" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(string name)
        {
            if (!this.IsAdminLoggedIn())
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["SettingsError"] = "Location name is required.";
                return RedirectToAction(nameof(Index));
            }

            await _locationService.CreateAsync(new Location
            {
                Name = name,
                IsActive = true
            });

            TempData["SettingsMessage"] = "Location added successfully.";
            return RedirectToAction(nameof(Index), new { tab = "locations" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(int locationId, string name)
        {
            if (!this.IsAdminLoggedIn())
                return RedirectToAction("Index", "Login");

            var location = await _locationService.GetByIdAsync(locationId);

            if (location == null)
            {
                TempData["SettingsError"] = "Location not found.";
                return RedirectToAction(nameof(Index));
            }

            location.Name = name;

            await _locationService.UpdateAsync(location);

            TempData["SettingsMessage"] = "Location updated successfully.";
            return RedirectToAction(nameof(Index), new { tab = "locations" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLocation(int id)
        {
            if (!this.IsAdminLoggedIn())
                return RedirectToAction("Index", "Login");

            var location = await _locationService.GetByIdAsync(id);

            if (location == null)
            {
                TempData["SettingsError"] = "Location not found.";
                return RedirectToAction(nameof(Index));
            }

            location.IsActive = !location.IsActive;

            await _locationService.UpdateAsync(location);

            TempData["SettingsMessage"] = "Location status updated.";
            return RedirectToAction(nameof(Index), new { tab = "locations" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name)
        {
            if (!this.IsAdminLoggedIn())
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["SettingsError"] = "Category name is required.";
                return RedirectToAction(nameof(Index), new { tab = "categories" });
            }

            await _categoryService.CreateAsync(new Category
            {
                Name = name,
                IsActive = true
            });

            TempData["SettingsMessage"] = "Category added successfully.";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int categoryId, string name)
        {
            if (!this.IsAdminLoggedIn())
                return RedirectToAction("Index", "Login");

            var category = await _categoryService.GetByIdAsync(categoryId);

            if (category == null)
            {
                TempData["SettingsError"] = "Category not found.";
                return RedirectToAction(nameof(Index), new { tab = "categories" });
            }

            category.Name = name;

            await _categoryService.UpdateAsync(category);

            TempData["SettingsMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategory(int id)
        {
            if (!this.IsAdminLoggedIn())
                return RedirectToAction("Index", "Login");

            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                TempData["SettingsError"] = "Category not found.";
                return RedirectToAction(nameof(Index), new { tab = "categories" });
            }

            category.IsActive = !category.IsActive;

            await _categoryService.UpdateAsync(category);

            TempData["SettingsMessage"] = "Category status updated.";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }
    }
}
