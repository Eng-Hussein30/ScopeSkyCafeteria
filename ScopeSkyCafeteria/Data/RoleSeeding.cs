using Microsoft.AspNetCore.Identity;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Data
{
    public static class RoleSeeding
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.SuperAdmin))
                await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.SuperAdmin));

            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Admin));

            if (!await roleManager.RoleExistsAsync(Roles.User))
                await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.User));
        }
    }
}
