using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class ManageItemsController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IWebHostEnvironment _environment;

        public ManageItemsController(IItemService itemService, IWebHostEnvironment environment)
        {
            _itemService = itemService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? category, string? location)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var items = await _itemService.GetAllItemsAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items.Where(i =>
                    i.ItemName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(i.Description) && i.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    i.Category.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    i.LocationFound.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                items = items.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                items = items.Where(i => i.LocationFound.Equals(location, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.Categories = Models.LookupData.Categories;
            ViewBag.Locations = Models.LookupData.Locations;
            ViewBag.Search = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedLocation = location;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrWhiteSpace(item.ImagePath))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, item.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            await _itemService.DeleteItemAsync(id);
            TempData["Success"] = "Item deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
