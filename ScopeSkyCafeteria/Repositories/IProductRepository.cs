using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IProductRepository
    {
        Task<Product>CreateProductAsync(Product product);
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product?> UpdateProductAsync(Guid id, Product product);
        Task<Product?> DeleteProductAsync(Guid id);
    }
}