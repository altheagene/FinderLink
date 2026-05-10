using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class ManageItemsController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILocationService _locationService;
        private readonly ICategoryService _categoryService;

        public ManageItemsController(IItemService itemService, IWebHostEnvironment environment, ILocationService locationService, ICategoryService categoryService)
        {
            _itemService = itemService;
            _environment = environment;
            _locationService = locationService;
            _categoryService = categoryService;
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

            //ViewBag.Categories = Models.LookupData.Categories;
            //ViewBag.Locations = Models.LookupData.Locations;
            ViewBag.Locations = await _locationService.GetAllAsync();
            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Search = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedLocation = location;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int itemId,
            string itemName,
            string? description,
            string category,
            string locationFound,
            IFormFile? imageFile,
            bool removeImage)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var item = await _itemService.GetItemByIdAsync(itemId);
            if (item == null)
            {
                TempData["Error"] = "Item not found.";
                return RedirectToAction(nameof(Index));
            }

            item.ItemName = itemName;
            item.Description = description;
            item.Category = category;
            item.LocationFound = locationFound;

            if (removeImage && !string.IsNullOrWhiteSpace(item.ImagePath))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, item.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                item.ImagePath = null;
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(item.ImagePath))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, item.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var savePath = Path.Combine(uploadsPath, fileName);
                await using var stream = System.IO.File.Create(savePath);
                await imageFile.CopyToAsync(stream);
                item.ImagePath = $"/uploads/{fileName}";
            }

            await _itemService.UpdateItemAsync(item);
            TempData["Success"] = "Item updated successfully.";
            return RedirectToAction(nameof(Index));
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
