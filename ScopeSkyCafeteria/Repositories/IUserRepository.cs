using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User?> DeleteUserAsync(Guid id);
        Task<List<User>> GetAllUserAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        
    }
}
