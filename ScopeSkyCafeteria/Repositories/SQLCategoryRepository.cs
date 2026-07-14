using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public class SQLCategoryRepository : ICategoryRepository
    {
        private readonly SSCafeteriaDbContext dbContext;
        public SQLCategoryRepository(SSCafeteriaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {

            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> DeleteCategoryAsync(Guid Id)
        {
            var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Id);
            if (existingCategory == null) { return null; }
            dbContext.Categories.Remove(existingCategory);
            await dbContext.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<List<Category>> GetAllCategoryAsync()
        {
            return await dbContext.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdCategoryAsync(Guid Id)
        {
           return await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<Category?> UpdateCategoryAsync(Guid Id, Category category)
        {
            var existingCategory =await dbContext.Categories.FirstOrDefaultAsync(x=>x.Id == Id);
            if (existingCategory == null) { return null; }
            existingCategory.Name = category.Name;

            await dbContext.SaveChangesAsync();
            return existingCategory;
        }
    }
}
