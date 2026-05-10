using FinderLink.Data;
using FinderLink.Models;
using Microsoft.EntityFrameworkCore;

namespace FinderLink.Services
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllAsync();
        Task<Location?> GetByIdAsync(int id);
        Task<Location> CreateAsync(Location location);
        Task<Location> UpdateAsync(Location location);
        Task<bool> ToggleAsync(int id);
    }

    public class LocationService : ILocationService
    {
        private readonly FinderLinkDbContext _context;

        public LocationService(FinderLinkDbContext context)
        {
            _context = context;
        }

        public async Task<List<Location>> GetAllAsync()
        {
            return await _context.Locations
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(x => x.LocationId == id);
        }

        public async Task<Location> CreateAsync(Location location)
        {
            location.IsActive = true;

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return location;
        }

        public async Task<Location> UpdateAsync(Location location)
        {
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();

            return location;
        }

        public async Task<bool> ToggleAsync(int id)
        {
            var loc = await _context.Locations.FindAsync(id);
            if (loc == null) return false;

            loc.IsActive = !loc.IsActive;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}