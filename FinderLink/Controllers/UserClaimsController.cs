using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public class UserClaimsController : Controller
    {
        private readonly IClaimService _claimService;

        public UserClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status = null)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var claims = await _claimService.GetAllClaimsAsync();
            if (!string.IsNullOrWhiteSpace(status))
            {
                claims = claims.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                claims = claims.Where(c =>
                    c.ClaimerName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.ClaimerContact.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.Item != null && c.Item.ItemName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(c.ClaimDescription) && c.ClaimDescription.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            var allClaims = await _claimService.GetAllClaimsAsync();
            ViewBag.PendingCount = allClaims.Count(c => c.Status == "pending");
            ViewBag.VerifiedCount = allClaims.Count(c => c.Status == "verified");
            ViewBag.RejectedCount = allClaims.Count(c => c.Status == "rejected");
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;
            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            var adminId = HttpContext.Session.GetInt32("AdminId")!.Value;
            await _claimService.VerifyClaimAsync(id, adminId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            if (!this.IsAdminLoggedIn())
            {
                return RedirectToAction("Index", "Login");
            }

            await _claimService.RejectClaimAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
