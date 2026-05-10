using FinderLink.Data;
using FinderLink.Models;
using Microsoft.EntityFrameworkCore;

namespace FinderLink.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> ToggleAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly FinderLinkDbContext _context;

        public CategoryService(FinderLinkDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            category.IsActive = true;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<bool> ToggleAsync(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return false;

            cat.IsActive = !cat.IsActive;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}