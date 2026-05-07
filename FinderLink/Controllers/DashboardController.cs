using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IItemService _itemService;

        public DashboardController(IItemService itemService)
        {
            _itemService = itemService;
        }

        public async Task<IActionResult> Index()
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var allItems = await _itemService.GetAllItemsAsync();
            ViewBag.TotalItems = allItems.Count;
            ViewBag.UnclaimedItems = allItems.Count(i => i.Status == "unclaimed");
            ViewBag.ClaimedItems = allItems.Count(i => i.Status == "claimed");
            ViewBag.PendingItems = allItems.Count(i => i.Status == "pending");

            var now = DateTime.UtcNow;
            var monthly = Enumerable.Range(0, 6)
                .Select(offset =>
                {
                    var month = now.AddMonths(-offset);
                    var lost = allItems.Count(i => i.DateFound.Year == month.Year && i.DateFound.Month == month.Month);
                    var claimed = allItems.Count(i => i.Status == "claimed" &&
                        i.Claims.Any(c => c.Status == "verified" && c.DateVerified.HasValue &&
                                          c.DateVerified.Value.Year == month.Year &&
                                          c.DateVerified.Value.Month == month.Month));
                    return new
                    {
                        Label = month.ToString("MMM yyyy"),
                        Lost = lost,
                        Claimed = claimed
                    };
                })
                .Reverse()
                .ToList();

            var byCategory = allItems
                .GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.MonthlyStats = monthly;
            ViewBag.CategoryStats = byCategory;
            ViewBag.RecentItems = allItems.OrderByDescending(i => i.CreatedAt).Take(10).ToList();
            return View();
        }
    }
}
