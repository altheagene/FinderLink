using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class ReportLossController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ReportLossController> _logger;

        public ReportLossController(IItemService itemService, IWebHostEnvironment environment, ILogger<ReportLossController> logger)
        {
            _itemService = itemService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Categories = LookupData.Categories;
            ViewBag.Locations = LookupData.Locations;
            ViewBag.RecentItems = (await _itemService.GetAllItemsAsync()).Take(8).ToList();
            return View(new ReportLossViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ReportLossViewModel model)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Categories = LookupData.Categories;
            ViewBag.Locations = LookupData.Locations;
            ViewBag.RecentItems = (await _itemService.GetAllItemsAsync()).Take(8).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (!adminId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            string imagePath = null;

            // ✅ OPTIONAL IMAGE HANDLING
            if (model.ItemPhoto != null && model.ItemPhoto.Length > 0)
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ItemPhoto.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ItemPhoto.CopyToAsync(stream);
                }

                imagePath = $"/uploads/{fileName}";
            }

            try
            {
                await _itemService.CreateItemAsync(new Item
                {
                    ItemName = model.ItemName,
                    Category = model.Category,
                    Description = model.Description,
                    LocationFound = model.LocationFound,
                    DateFound = model.DateFound,
                    FoundByName = model.FoundByName,
                    FoundByContact = model.FoundByContact,
                    ImagePath = imagePath, // can be null now
                    CreatedBy = adminId.Value,
                    Status = "unclaimed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit report loss item.");
                ModelState.AddModelError(string.Empty, "Unable to save the report. Please try again.");
                return View(model);
            }

            TempData["Success"] = "Item report submitted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
