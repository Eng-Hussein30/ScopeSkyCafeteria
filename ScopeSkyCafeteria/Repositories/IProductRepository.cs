using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IProductRepository
    {
        Task<Product>CreateProductAsync(Product product);
        Task<List<Product>> GetAllProductsAsync(
            Guid? categoryId = null,
            string? search = null);
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product?> UpdateProductAsync(Guid id, Product product);
        Task<Product?> DeleteProductAsync(Guid id);
        Task<Product?> UpdateProductImageAsync(Guid productId, string imageUrl);
    }
}