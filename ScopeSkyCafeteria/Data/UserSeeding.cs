using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Data
{
    public class UserSeeding
    {
        public static async Task SeedUsersAsync(
            UserManager<User> userManager,
            SSCafeteriaDbContext dbContext)
        {
            Env.Load();

            // ================= SUPER ADMIN =================

            var superAdminUserName = Env.GetString("SUPERADMIN_USERNAME");
            var superAdminEmail = Env.GetString("SUPERADMIN_EMAIL");
            var superAdminPassword = Env.GetString("SUPERADMIN_PASSWORD");
            var superAdminPhone = Env.GetString("SUPERADMIN_PHONE");

            var superAdmin = await userManager.FindByNameAsync(superAdminUserName);

            if (superAdmin == null)
            {
                superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = superAdminUserName,
                    Email = superAdminEmail,
                    PhoneNumber = superAdminPhone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    superAdmin,
                    superAdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        superAdmin,
                        Roles.SuperAdmin);

                    await CreateWalletAsync(
                        dbContext,
                        superAdmin);
                }
                else
                {
                    var errors = string.Join(
                        " | ",
                        result.Errors.Select(e =>
                            $"{e.Code}: {e.Description}"));

                    throw new Exception(
                        $"Failed to create SuperAdmin: {errors}");
                }
            }
            else
            {
                await CreateWalletIfNotExistsAsync(
                    dbContext,
                    superAdmin);
            }


            // ================= ADMIN =================

            var adminUserName = Env.GetString("ADMIN_USERNAME");
            var adminEmail = Env.GetString("ADMIN_EMAIL");
            var adminPassword = Env.GetString("ADMIN_PASSWORD");
            var adminPhone = Env.GetString("ADMIN_PHONE");

            var admin = await userManager.FindByNameAsync(adminUserName);

            if (admin == null)
            {
                admin = new User
                {
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = adminUserName,
                    Email = adminEmail,
                    PhoneNumber = adminPhone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin,
                    adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        Roles.Admin);

                    await CreateWalletAsync(
                        dbContext,
                        admin);
                }
                else
                {
                    var errors = string.Join(
                        " | ",
                        result.Errors.Select(e =>
                            $"{e.Code}: {e.Description}"));

                    throw new Exception(
                        $"Failed to create Admin: {errors}");
                }
            }
            else
            {
                await CreateWalletIfNotExistsAsync(
                    dbContext,
                    admin);
            }


            // ================= USER =================

            var normalUserName = Env.GetString("USER_USERNAME");
            var normalUserEmail = Env.GetString("USER_EMAIL");
            var normalUserPassword = Env.GetString("USER_PASSWORD");
            var normalUserPhone = Env.GetString("USER_PHONE");

            var user = await userManager.FindByNameAsync(normalUserName);

            if (user == null)
            {
                user = new User
                {
                    FirstName = "Default",
                    LastName = "User",
                    UserName = normalUserName,
                    Email = normalUserEmail,
                    PhoneNumber = normalUserPhone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    DepartmentName = "PR"
                };

                var result = await userManager.CreateAsync(
                    user,
                    normalUserPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        user,
                        Roles.User);

                    await CreateWalletAsync(
                        dbContext,
                        user);
                }
                else
                {
                    var errors = string.Join(
                        " | ",
                        result.Errors.Select(e =>
                            $"{e.Code}: {e.Description}"));

                    throw new Exception(
                        $"Failed to create User: {errors}");
                }
            }
            else
            {
                await CreateWalletIfNotExistsAsync(
                    dbContext,
                    user);
            }
        }


        // =====================================================
        // إنشاء محفظة جديدة
        // =====================================================

        private static async Task CreateWalletAsync(
            SSCafeteriaDbContext dbContext,
            User user)
        {
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Balance = 0,
                Debt = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await dbContext.Wallets.AddAsync(wallet);
            await dbContext.SaveChangesAsync();
        }


        // =====================================================
        // التأكد من وجود المحفظة
        // =====================================================

        private static async Task CreateWalletIfNotExistsAsync(
            SSCafeteriaDbContext dbContext,
            User user)
        {
            var walletExists = await dbContext.Wallets
                .AnyAsync(x => x.UserId == user.Id);

            if (walletExists)
                return;

            await CreateWalletAsync(
                dbContext,
                user);
        }
    }
}
