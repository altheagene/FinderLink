using FinderLink.Data;
using FinderLink.Models;

namespace FinderLink.Services
{
    public interface IDbInitializerService
    {
        Task InitializeAsync();
        Task SeedDefaultDataAsync();
    }

    public class DbInitializerService : IDbInitializerService
    {
        private readonly FinderLinkDbContext _context;
        private readonly ILogger<DbInitializerService> _logger;

        public DbInitializerService(FinderLinkDbContext context, ILogger<DbInitializerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database migration");
                throw;
            }
        }

        public async Task SeedDefaultDataAsync()
        {
            try
            {
                if (_context.Admins.Any())
                {
                    _logger.LogInformation("Database already contains data. Skipping seeding.");
                    return;
                }

                var admin = new Admin
                {
                    Name = "System Administrator",
                    Username = "admin",
                    Email = "admin@finderlink.com",
                    Password = "admin123",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                var item1 = new Item
                {
                    ItemName = "Lost iPhone 15",
                    Description = "Black iPhone 15 with blue case, found in parking lot",
                    Category = "Electronics",
                    LocationFound = "Parking Area",
                    Status = "unclaimed",
                    DateFound = DateTime.UtcNow.AddDays(-5).Date,
                    FoundByName = "Security Officer",
                    FoundByContact = "guard@finderlink.local",
                    CreatedBy = admin.AdminId,
                    CreatedAt = DateTime.UtcNow
                };

                var item2 = new Item
                {
                    ItemName = "Keys",
                    Description = "Set of car keys with red keychain",
                    Category = "Keys",
                    LocationFound = "Main Gate",
                    Status = "unclaimed",
                    DateFound = DateTime.UtcNow.AddDays(-2).Date,
                    FoundByName = "Campus Staff",
                    FoundByContact = "staff@finderlink.local",
                    CreatedBy = admin.AdminId,
                    CreatedAt = DateTime.UtcNow
                };

                var item3 = new Item
                {
                    ItemName = "Wallet",
                    Description = "Brown leather wallet with driver's license",
                    Category = "Wallet",
                    LocationFound = "Library",
                    Status = "unclaimed",
                    DateFound = DateTime.UtcNow.AddDays(-1).Date,
                    FoundByName = "Anonymous",
                    FoundByContact = "unknown@example.com",
                    CreatedBy = admin.AdminId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Items.AddRange(item1, item2, item3);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database seeding");
                throw;
            }
        }
    }
}