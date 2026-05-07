using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class PublicController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IClaimService _claimService;
        private readonly IAdminService _adminService;

        public PublicController(IItemService itemService, IClaimService claimService, IAdminService adminService)
        {
            _itemService = itemService;
            _claimService = claimService;
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? category, string? location)
        {
            var items = await _itemService.GetItemsByStatusAsync("unclaimed");
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

            ViewBag.Categories = LookupData.Categories;
            ViewBag.Locations = LookupData.Locations;
            ViewBag.Search = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedLocation = location;
            ViewBag.TotalUnclaimed = (await _itemService.GetItemsByStatusAsync("unclaimed")).Count;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(CreateClaimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ClaimError"] = "Please complete all required fields.";
                return RedirectToAction(nameof(Index));
            }

            var defaultAdmin = await _adminService.GetAdminByUsernameAsync("admin");
            if (defaultAdmin == null)
            {
                TempData["ClaimError"] = "Admin account is missing.";
                return RedirectToAction(nameof(Index));
            }

            await _claimService.CreateClaimAsync(new Claim
            {
                ItemId = model.ItemId,
                AdminId = defaultAdmin.AdminId,
                ClaimerName = model.FullName,
                ClaimerContact = model.ContactInfo,
                ClaimDescription = model.ProofOfOwnership
            });

            TempData["ClaimSuccess"] = "Claim submitted for verification.";
            return RedirectToAction(nameof(Index));
        }
    }
}
