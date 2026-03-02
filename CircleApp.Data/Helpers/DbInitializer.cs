using System;
using System.Linq;
using System.Threading.Tasks;
using CircleApp.Data.Helpers.Constants;
using CircleApp.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace CircleApp.Data.Helpers
{
    public static class DbInitializer
    {
        public static async Task SeedUsersAndRolesAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // --------------------
            // ROLES
            // --------------------
            foreach (var roleName in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            const string defaultPassword = "Coding@1234?";

            // --------------------
            // USER
            // --------------------
            const string userEmail = "user@mail.com";
            var existingUser = await userManager.FindByEmailAsync(userEmail);

            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = "user",
                    Email = userEmail,
                    FullName = "USER",
                    ProfilePictureUrl = "https://cdn-icons-png.flaticon.com/512/9203/9203764.png",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, AppRoles.User);
                }
            }

            // --------------------
            // ADMIN
            // --------------------
            const string adminEmail = "admin@email.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new User
                {
                    UserName = "admin.admin",
                    Email = adminEmail,
                    FullName = "Admin",
                    ProfilePictureUrl = "https://cdn-icons-png.flaticon.com/512/9203/9203764.png",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                }
            }
        }

        // --------------------------------------------------
        // OPTIONAL: Legacy data seed (currently unused)
        // --------------------------------------------------
        public static async Task SeedAsync(AppDbContext appDbContext)
        {
            await Task.CompletedTask;
        }
    }
}