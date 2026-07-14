using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
  
        public interface ITokenRepository
        {
            Task<string> CreatJWTToken(User user);
        }
    
}
