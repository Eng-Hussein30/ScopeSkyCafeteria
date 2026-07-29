using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Data
{
    public class UserSeeding
    {
        public static async Task SeedUsersAsync(UserManager<User> userManager)
        {

            Env.Load();

            // ================= SUPER ADMIN =================
            var superAdminUserName = Env.GetString("SUPERADMIN_USERNAME");
            var superAdminEmail = Env.GetString("SUPERADMIN_EMAIL");
            var superAdminPassword = Env.GetString("SUPERADMIN_PASSWORD");

            if (await userManager.FindByNameAsync(superAdminUserName) == null)
            {
                var superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = superAdminUserName,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superAdmin, superAdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
                }
            }

            // ================= ADMIN =================
            var adminUserName = Env.GetString("ADMIN_USERNAME");
            var adminEmail = Env.GetString("ADMIN_EMAIL");
            var adminPassword = Env.GetString("ADMIN_PASSWORD");

            if (await userManager.FindByNameAsync(adminUserName) == null)
            {
                var admin = new User
                {
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
                }
            }

            // ================= USER =================
            var normalUserName = Env.GetString("USER_USERNAME");
            var normalUserEmail = Env.GetString("USER_EMAIL");
            var normalUserPassword = Env.GetString("USER_PASSWORD");

            if (await userManager.FindByNameAsync(normalUserName) == null)
            {
                var user = new User
                {
                    FirstName = "Default",
                    LastName = "User",
                    UserName = normalUserName,
                    Email = normalUserEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, normalUserPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.User);
                }
            }
        }
    }
}