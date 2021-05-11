using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Data
{
    public class ContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (await roleManager.Roles.AnyAsync())
                return;

            await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Teacher"));
            await roleManager.CreateAsync(new IdentityRole("Student"));
        }

        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByNameAsync("superadmin") == null)
            {
                var defaultSuperAdmin = new ApplicationUser
                {
                    UserName = "superadmin",
                    Email = "superadmin@martin.com",
                    FirstName = "Martin",
                    LastName = "Pejchinovski",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                await userManager.CreateAsync(defaultSuperAdmin, "SuperAdmin925;");
                await userManager.AddToRoleAsync(defaultSuperAdmin, "Student");
                await userManager.AddToRoleAsync(defaultSuperAdmin, "Teacher");
                await userManager.AddToRoleAsync(defaultSuperAdmin, "Admin");
                await userManager.AddToRoleAsync(defaultSuperAdmin, "SuperAdmin");
            }
        }

        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var defaultAdmin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@martin.com",
                    FirstName = "Martin",
                    LastName = "Pejchinovski",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                await userManager.CreateAsync(defaultAdmin, "Admin925;");
                await userManager.AddToRoleAsync(defaultAdmin, "Admin");
            }
        }
    }
}