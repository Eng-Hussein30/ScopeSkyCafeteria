using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category?> UpdateCategoryAsync(Guid Id, Category category);
        Task<Category?> DeleteCategoryAsync(Guid Id);
        Task<List<Category>> GetAllCategoryAsync();
        Task<Category?> GetByIdCategoryAsync(Guid Id);

        Task<bool> CategoryNameExistsAsync(
            string name,
            Guid? excludeId = null);
    }
}