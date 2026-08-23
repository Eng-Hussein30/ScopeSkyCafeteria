using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public class SQLUserRepository : IUserRepository
    {
            private readonly SSCafeteriaDbContext dbContext;
    
            public SQLUserRepository(SSCafeteriaDbContext dbContext)
            {
                 this.dbContext = dbContext;
            }
    
            public async Task<User> CreateUserAsync(User user)
            {
                user.Id = Guid.NewGuid();
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
                return user;
            }
    
            public async Task<User?> DeleteUserAsync(Guid id)
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return null;

                dbContext.Users.Remove(user);
                await dbContext.SaveChangesAsync();
                return user;
            }
    
            public async Task<List<User>> GetAllUserAsync()
            {
                return await dbContext.Users.ToListAsync();
            }
    
            public async Task<User?> GetUserByIdAsync(Guid id)
            {
                return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            }
    }
}
