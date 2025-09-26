using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Korzh.DbUtils;
using Korzh.EasyQuery.Services;
using EqDemo.Data;
using EqDemo.Models;
using EqDemo.Services;

namespace EqDemo
{
    public static class DbInitializeExtensions
    {
        public static async Task EnsureDbInitializedAsync(this WebApplication app, IConfiguration config, IWebHostEnvironment env)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (await context.Database.EnsureCreatedAsync())
            {
                Korzh.DbUtils.DbInitializer.Create(options => {
                    options.UseSqlite(config.GetConnectionString("EqDemoDb"));
                    options.UseZipPacker(Path.Combine(env.ContentRootPath, "App_Data", "EqDemoData.zip"));
                })
                .Seed();
            }

            if (context.Database.CanConnect())
            {
                await CheckAddManagerRoleAsync(scope.ServiceProvider);
                await CheckAddDefaultUserAsync(scope.ServiceProvider, config);
            }
        }

        const string _defaultUserEmail = "demo@korzh.com";
        const string _defaultUserPassword = "demo";

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices, IConfiguration config)
        {
            var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();

            try
            {
                var user = await userManager.FindByEmailAsync(_defaultUserEmail);
                if (user == null)
                {
                    user = new IdentityUser()
                    {
                        UserName = _defaultUserEmail,
                        Email = _defaultUserEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, _defaultUserPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "eq-manager");
                        var defaultReportsGenerator = scopedServices.GetRequiredService<DefaultReportGenerator>();
                        defaultReportsGenerator.Generate(user);
                    }
                }
                else
                {
                    await userManager.AddToRoleAsync(user, "eq-manager");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task CheckAddManagerRoleAsync(IServiceProvider scopedServices)
        {
            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                IdentityRole role = await roleManager.FindByNameAsync("eq-manager");
                if (role == null)
                {
                    role = new IdentityRole("eq-manager");
                    var result = await roleManager.CreateAsync(role);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
