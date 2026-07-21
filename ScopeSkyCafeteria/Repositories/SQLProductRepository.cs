using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public class SQLProductRepository : IProductRepository
    {
        private readonly SSCafeteriaDbContext dbContext;

        public SQLProductRepository(SSCafeteriaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> DeleteProductAsync(Guid id)
        {
            var existingProduct = await dbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (existingProduct == null) { return null; }
            dbContext.Products.Remove(existingProduct);
            await dbContext.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<List<Product>> GetAllProductsAsync(
            Guid? categoryId = null,
            string? search = null)
        {
            var query = dbContext.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Filter by Category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Search by Product Name
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{search}%"));
            }

            return await query.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await dbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Product?> UpdateProductAsync(Guid id, Product product)
        {
            var existingProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null) { return null; }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.IsAvailable = product.IsAvailable;
            existingProduct.CategoryId = product.CategoryId;

            await dbContext.SaveChangesAsync();

            return existingProduct;
        }

        public async Task<Product?> UpdateProductImageAsync(Guid productId, string imageUrl)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return null;
            }

            product.ImageUrl = imageUrl;

            await dbContext.SaveChangesAsync();

            return product;
        }
    }
}