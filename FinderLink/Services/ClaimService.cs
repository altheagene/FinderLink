using FinderLink.Data;
using FinderLink.Models;
using Microsoft.EntityFrameworkCore;

namespace FinderLink.Services
{
    public interface IClaimService
    {
        Task<List<Claim>> GetAllClaimsAsync();
        Task<Claim> CreateClaimAsync(Claim claim);
        Task<Claim?> GetClaimByIdAsync(int claimId);
        Task<List<Claim>> GetClaimsByItemAsync(int itemId);
        Task<List<Claim>> GetClaimsByAdminAsync(int adminId);
        Task<List<Claim>> GetClaimsByStatusAsync(string status);
        Task<Claim> VerifyClaimAsync(int claimId, int verifiedBy);
        Task<Claim> RejectClaimAsync(int claimId);
        Task<bool> DeleteClaimAsync(int claimId);
    }

    public class ClaimService : IClaimService
    {
        private readonly FinderLinkDbContext _context;

        public ClaimService(FinderLinkDbContext context)
        {
            _context = context;
        }

        public async Task<List<Claim>> GetAllClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.Item)
                .Include(c => c.Admin)
                .Include(c => c.VerifiedByAdmin)
                .OrderByDescending(c => c.DateClaimed)
                .ToListAsync();
        }

        public async Task<Claim> CreateClaimAsync(Claim claim)
        {
            claim.DateClaimed = DateTime.UtcNow;
            claim.Status = "pending";

            // Update item status to "pending" when first claim is made
            var item = await _context.Items.FindAsync(claim.ItemId);
            if (item != null && item.Status == "unclaimed")
            {
                item.Status = "pending";
            }

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<Claim?> GetClaimByIdAsync(int claimId)
        {
            return await _context.Claims
                .Include(c => c.Item)
                .Include(c => c.Admin)
                .Include(c => c.VerifiedByAdmin)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);
        }

        public async Task<List<Claim>> GetClaimsByItemAsync(int itemId)
        {
            return await _context.Claims
                .Where(c => c.ItemId == itemId)
                .Include(c => c.Admin)
                .Include(c => c.VerifiedByAdmin)
                .OrderByDescending(c => c.DateClaimed)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetClaimsByAdminAsync(int adminId)
        {
            return await _context.Claims
                .Where(c => c.AdminId == adminId)
                .Include(c => c.Item)
                .Include(c => c.VerifiedByAdmin)
                .OrderByDescending(c => c.DateClaimed)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetClaimsByStatusAsync(string status)
        {
            return await _context.Claims
                .Where(c => c.Status == status)
                .Include(c => c.Item)
                .Include(c => c.Admin)
                .OrderByDescending(c => c.DateClaimed)
                .ToListAsync();
        }

        public async Task<Claim> VerifyClaimAsync(int claimId, int verifiedBy)
        {
            var claim = await _context.Claims
                .Include(c => c.Item)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
                throw new InvalidOperationException("Claim not found");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    claim.Status = "verified";
                    claim.VerifiedBy = verifiedBy;
                    claim.DateVerified = DateTime.UtcNow;

                    // Update item status to claimed
                    if (claim.Item != null)
                    {
                        claim.Item.Status = "claimed";
                    }

                    _context.Claims.Update(claim);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return claim;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Claim> RejectClaimAsync(int claimId)
        {
            var claim = await _context.Claims.FindAsync(claimId);

            if (claim == null)
                throw new InvalidOperationException("Claim not found");

            claim.Status = "rejected";
            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            return claim;
        }

        public async Task<bool> DeleteClaimAsync(int claimId)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return false;

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
