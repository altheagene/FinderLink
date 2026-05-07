using FinderLink.Data;
using FinderLink.Models;
using Microsoft.EntityFrameworkCore;

namespace FinderLink.Services
{
    public interface IItemService
    {
        Task<Item> CreateItemAsync(Item item);
        Task<Item?> GetItemByIdAsync(int itemId);
        Task<List<Item>> GetItemsByStatusAsync(string status);
        Task<List<Item>> GetAllItemsAsync();
        Task<Item> UpdateItemAsync(Item item);
        Task<bool> DeleteItemAsync(int itemId);
        Task<List<Item>> SearchItemsAsync(string searchTerm);
    }

    public class ItemService : IItemService
    {
        private readonly FinderLinkDbContext _context;

        public ItemService(FinderLinkDbContext context)
        {
            _context = context;
        }

        public async Task<Item> CreateItemAsync(Item item)
        {
            item.CreatedAt = DateTime.UtcNow;
            item.Status = "unclaimed";

            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Item?> GetItemByIdAsync(int itemId)
        {
            return await _context.Items
                .Include(i => i.CreatedByAdmin)
                .Include(i => i.FoundByAdmin)
                .Include(i => i.Claims)
                .Include(i => i.Releases)
                .FirstOrDefaultAsync(i => i.ItemId == itemId);
        }

        public async Task<List<Item>> GetItemsByStatusAsync(string status)
        {
            return await _context.Items
                .Where(i => i.Status == status)
                .Include(i => i.CreatedByAdmin)
                .Include(i => i.Claims)
                .OrderByDescending(i => i.DateFound)
                .ToListAsync();
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            return await _context.Items
                .Include(i => i.CreatedByAdmin)
                .Include(i => i.FoundByAdmin)
                .Include(i => i.Claims)
                .OrderByDescending(i => i.DateFound)
                .ToListAsync();
        }

        public async Task<Item> UpdateItemAsync(Item item)
        {
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null) return false;

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Item>> SearchItemsAsync(string searchTerm)
        {
            return await _context.Items
                .Where(i => i.ItemName.Contains(searchTerm) ||
                           (i.Description != null && i.Description.Contains(searchTerm)) ||
                           i.Category.Contains(searchTerm) ||
                           i.LocationFound.Contains(searchTerm))
                .Include(i => i.CreatedByAdmin)
                .OrderByDescending(i => i.DateFound)
                .ToListAsync();
        }
    }
}
