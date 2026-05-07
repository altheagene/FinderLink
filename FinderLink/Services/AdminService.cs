using FinderLink.Data;
using FinderLink.Models;
using Microsoft.EntityFrameworkCore;

namespace FinderLink.Services
{
    public interface IAdminService
    {
        Task<Admin> CreateAdminAsync(Admin admin);
        Task<Admin?> GetAdminByIdAsync(int adminId);
        Task<Admin?> GetAdminByEmailAsync(string email);
        Task<Admin?> GetAdminByUsernameAsync(string username);
        Task<List<Admin>> GetAllAdminsAsync();
        Task<Admin> UpdateAdminAsync(Admin admin);
        Task<bool> DeleteAdminAsync(int adminId);
        Task<bool> ValidateLoginAsync(string email, string password);
    }

    public class AdminService : IAdminService
    {
        private readonly FinderLinkDbContext _context;

        public AdminService(FinderLinkDbContext context)
        {
            _context = context;
        }

        public async Task<Admin> CreateAdminAsync(Admin admin)
        {
            admin.CreatedAt = DateTime.UtcNow;
            // TODO: Hash password before storing
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<Admin?> GetAdminByIdAsync(int adminId)
        {
            return await _context.Admins
                .Include(a => a.ItemsCreated)
                .Include(a => a.AdminLogs)
                .FirstOrDefaultAsync(a => a.AdminId == adminId);
        }

        public async Task<Admin?> GetAdminByEmailAsync(string email)
        {
            return await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Admin?> GetAdminByUsernameAsync(string username)
        {
            return await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<List<Admin>> GetAllAdminsAsync()
        {
            return await _context.Admins
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Admin> UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<bool> DeleteAdminAsync(int adminId)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null) return false;

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateLoginAsync(string username, string password)
        {
            var admin = await GetAdminByUsernameAsync(username);
            if (admin == null) return false;

            return admin.Password == password;
        }
    }
}
