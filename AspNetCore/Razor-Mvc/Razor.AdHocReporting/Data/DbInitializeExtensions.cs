using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

using Korzh.DbUtils;

using Korzh.EasyQuery.Services;
using System.Threading.Tasks;

namespace EqDemo.Services
{
    public static class DbInitializeExtensions
    {
        private static IConfiguration _config;
        private static IWebHostEnvironment _env;

        public static async Task EnsureDbInitializedAsync(this IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = scope.ServiceProvider.GetService<AppDbContext>()) {
                if (context.Database.EnsureCreated()) {
                    Korzh.DbUtils.DbInitializer.Create(options => {
                        options.UseSqlite(config.GetConnectionString("EqDemoDb"));
                        //options.UseSqlServer(config.GetConnectionString("EqDemoDb"));
                        options.UseZipPacker(System.IO.Path.Combine(env.ContentRootPath, "App_Data", "EqDemoData.zip"));
                    })
                    .Seed();
                }

                if (context.Database.CanConnect()) {
                    //create eq-manager role
                    await CheckAddManagerRoleAsync(scope.ServiceProvider);

                    //create default user
                    await CheckAddDefaultUserAsync(scope.ServiceProvider);
                }
            }
        }

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices)
        {
            var demoUser = DemoUserOptions.FromConfiguration(_config);
            if (!demoUser.IsConfigured) {
                Console.WriteLine($"Default demo user is not created: set '{DemoUserOptions.PasswordKey}' via user-secrets or environment variables.");
                return;
            }

            var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
            try {
                var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                var user = await userManager.FindByEmailAsync(demoUser.Email);
                var resetDemoUser = _config.GetValue<bool>("resetDefaultUser");
                if (resetDemoUser && user != null) {
                    dbContext.Reports.RemoveRange(dbContext.Reports.Where(r => r.OwnerId == user.Id));
                    dbContext.SaveChanges();

                    await userManager.DeleteAsync(user);
                    user = null;
                }

                if (user == null) {
                    user = new IdentityUser() {
                        UserName = demoUser.Email,
                        Email = demoUser.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, demoUser.Password);
                    if (result.Succeeded) {
                        await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                        var defaultReportsGenerator = new DefaultReportGenerator(_env, dbContext);
                        await defaultReportsGenerator.GenerateAsync(user);
                    }
                }
                else {
                    await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private static async Task CheckAddManagerRoleAsync(IServiceProvider scopedServices)
        {
            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

            try {
                IdentityRole role = await roleManager.FindByNameAsync(DefaultEqAuthProvider.EqManagerRole);
                if (role == null) {
                    role = new IdentityRole(DefaultEqAuthProvider.EqManagerRole);
                    var result = await roleManager.CreateAsync(role);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }
    }
}
