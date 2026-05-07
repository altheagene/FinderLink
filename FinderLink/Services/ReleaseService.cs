using FinderLink.Data;
using FinderLink.Models;
using Microsoft.EntityFrameworkCore;

namespace FinderLink.Services
{
    public interface IReleaseService
    {
        Task<Release> ReleaseItemAsync(int itemId, int claimId, int releasedBy, int releasedTo, string? proof);
        Task<Release?> GetReleaseByIdAsync(int releaseId);
        Task<List<Release>> GetReleasesByAdminAsync(int adminId);
        Task<List<Release>> GetReleasesByItemAsync(int itemId);
        Task<List<Release>> GetAllReleasesAsync();
    }

    public class ReleaseService : IReleaseService
    {
        private readonly FinderLinkDbContext _context;
        private readonly IAdminLogService _adminLogService;

        public ReleaseService(FinderLinkDbContext context, IAdminLogService adminLogService)
        {
            _context = context;
            _adminLogService = adminLogService;
        }

        public async Task<Release> ReleaseItemAsync(int itemId, int claimId, int releasedBy, int releasedTo, string? proof)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var claim = await _context.Claims
                        .Include(c => c.Item)
                        .FirstOrDefaultAsync(c => c.ClaimId == claimId);

                    if (claim == null)
                        throw new InvalidOperationException("Claim not found");

                    if (claim.Status != "verified")
                        throw new InvalidOperationException("Only verified claims can be released");

                    var release = new Release
                    {
                        ItemId = itemId,
                        ClaimId = claimId,
                        ReleasedTo = releasedTo,
                        ReleasedBy = releasedBy,
                        ReleaseDate = DateTime.UtcNow,
                        Proof = proof
                    };

                    // Update claim status
                    claim.Status = "released";

                    // Update item status to released
                    if (claim.Item != null)
                    {
                        claim.Item.Status = "released";
                    }

                    _context.Releases.Add(release);
                    _context.Claims.Update(claim);
                    await _context.SaveChangesAsync();

                    // Log the action
                    await _adminLogService.LogActionAsync(releasedBy, "release_item", itemId, claimId, "Item released");

                    await transaction.CommitAsync();
                    return release;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Release?> GetReleaseByIdAsync(int releaseId)
        {
            return await _context.Releases
                .Include(r => r.Item)
                .Include(r => r.Claim)
                .Include(r => r.ReleasedToAdmin)
                .Include(r => r.ReleasedByAdmin)
                .FirstOrDefaultAsync(r => r.ReleaseId == releaseId);
        }

        public async Task<List<Release>> GetReleasesByAdminAsync(int adminId)
        {
            return await _context.Releases
                .Where(r => r.ReleasedTo == adminId || r.ReleasedBy == adminId)
                .Include(r => r.Item)
                .Include(r => r.Claim)
                .Include(r => r.ReleasedToAdmin)
                .Include(r => r.ReleasedByAdmin)
                .OrderByDescending(r => r.ReleaseDate)
                .ToListAsync();
        }

        public async Task<List<Release>> GetReleasesByItemAsync(int itemId)
        {
            return await _context.Releases
                .Where(r => r.ItemId == itemId)
                .Include(r => r.Claim)
                .Include(r => r.ReleasedToAdmin)
                .Include(r => r.ReleasedByAdmin)
                .OrderByDescending(r => r.ReleaseDate)
                .ToListAsync();
        }

        public async Task<List<Release>> GetAllReleasesAsync()
        {
            return await _context.Releases
                .Include(r => r.Item)
                .Include(r => r.Claim)
                .Include(r => r.ReleasedToAdmin)
                .Include(r => r.ReleasedByAdmin)
                .OrderByDescending(r => r.ReleaseDate)
                .ToListAsync();
        }
    }
}
