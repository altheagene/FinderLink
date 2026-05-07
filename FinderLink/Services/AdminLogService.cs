using FinderLink.Data;
using FinderLink.Models;
using Microsoft.EntityFrameworkCore;

namespace FinderLink.Services
{
    public interface IAdminLogService
    {
        Task<AdminLog> LogActionAsync(int adminId, string action, int? itemId = null, int? claimId = null, string? remarks = null);
        Task<List<AdminLog>> GetLogsByAdminAsync(int adminId);
        Task<List<AdminLog>> GetLogsByActionAsync(string action);
        Task<List<AdminLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AdminLog>> GetAllLogsAsync();
    }

    public class AdminLogService : IAdminLogService
    {
        private readonly FinderLinkDbContext _context;

        public AdminLogService(FinderLinkDbContext context)
        {
            _context = context;
        }

        public async Task<AdminLog> LogActionAsync(int adminId, string action, int? itemId = null, int? claimId = null, string? remarks = null)
        {
            var log = new AdminLog
            {
                AdminId = adminId,
                Action = action,
                ItemId = itemId,
                ClaimId = claimId,
                LogDate = DateTime.UtcNow,
                Remarks = remarks
            };

            _context.AdminLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<List<AdminLog>> GetLogsByAdminAsync(int adminId)
        {
            return await _context.AdminLogs
                .Where(al => al.AdminId == adminId)
                .Include(al => al.Admin)
                .Include(al => al.Item)
                .Include(al => al.Claim)
                .OrderByDescending(al => al.LogDate)
                .ToListAsync();
        }

        public async Task<List<AdminLog>> GetLogsByActionAsync(string action)
        {
            return await _context.AdminLogs
                .Where(al => al.Action == action)
                .Include(al => al.Admin)
                .Include(al => al.Item)
                .OrderByDescending(al => al.LogDate)
                .ToListAsync();
        }

        public async Task<List<AdminLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AdminLogs
                .Where(al => al.LogDate >= startDate && al.LogDate <= endDate)
                .Include(al => al.Admin)
                .Include(al => al.Item)
                .OrderByDescending(al => al.LogDate)
                .ToListAsync();
        }

        public async Task<List<AdminLog>> GetAllLogsAsync()
        {
            return await _context.AdminLogs
                .Include(al => al.Admin)
                .Include(al => al.Item)
                .Include(al => al.Claim)
                .OrderByDescending(al => al.LogDate)
                .ToListAsync();
        }
    }
}
