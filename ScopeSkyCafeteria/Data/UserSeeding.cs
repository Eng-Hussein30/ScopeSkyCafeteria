using Microsoft.AspNetCore.Identity;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Data
{
    public class UserSeeding
    {
        public static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            // ================= SUPER ADMIN =================
            var UserNameSuperAdmin = "SuperAdmin";
            var EmailSuperAdmin = "superadmin@test.com";

            if (await userManager.FindByNameAsync(UserNameSuperAdmin) == null)
            {
                var superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = UserNameSuperAdmin,
                    Email = EmailSuperAdmin,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superAdmin, "SuperAdmin@1996");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
                }
            }

            // ================= ADMIN =================
            var UserNameAdmin = "Admin";
            var adminEmail = "admin@test.com";

            if (await userManager.FindByNameAsync(UserNameAdmin) == null)
            {
                var admin = new User
                {
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = UserNameAdmin,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@1996");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
                }
            }

            // ================= USER =================
            var UserNameUser = "User";
            var EmailUser = "user@test.com";

            if (await userManager.FindByNameAsync(UserNameUser) == null)
            {
                var user = new User
                {
                    FirstName = "Default",
                    LastName = "User",
                    UserName = UserNameUser,
                    Email = EmailUser,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "User@1996");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.User);
                }
            }
        }
    }
}
