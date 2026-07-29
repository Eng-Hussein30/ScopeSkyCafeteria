using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ScopeSkyCafeteria.Models.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ScopeSkyCafeteria.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly UserManager<User> userManager;

        public TokenRepository(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<string> CreatJWTToken(User user)
        {
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtKey =
                Environment.GetEnvironmentVariable("JWT_KEY")
                ?? throw new Exception("JWT_KEY is missing.");

            var jwtIssuer =
                Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? throw new Exception("JWT_ISSUER is missing.");

            var jwtAudience =
                Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? throw new Exception("JWT_AUDIENCE is missing.");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}