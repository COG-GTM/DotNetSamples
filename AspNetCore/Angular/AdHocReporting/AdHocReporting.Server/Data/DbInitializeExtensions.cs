using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

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
            var (defaultUserPassword, isGeneratedPassword) = ResolveDefaultUserPassword();

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
                    await CheckAddDefaultUserAsync(scope.ServiceProvider, defaultUserPassword, isGeneratedPassword);
                }
            }
        }

        public static string defaultUserEmail {
            get {
                return _config.GetValue("defaultUserEmail", "demo@korzh.com").ToString();
            }
        }
        // Seeded by earlier versions of this sample and published in its source code.
        private const string PublishedDemoPassword = "demo";

        private static (string Password, bool IsGenerated) ResolveDefaultUserPassword()
        {
            var configured = _config.GetValue<string>("defaultUserPassword");
            if (!string.IsNullOrEmpty(configured)) {
                if (configured == PublishedDemoPassword && !_env.IsDevelopment()) {
                    throw new InvalidOperationException(
                        "'defaultUserPassword' is set to the publicly known sample password. Configure a different value before running outside Development.");
                }
                return (configured, false);
            }

            if (!_env.IsDevelopment()) {
                throw new InvalidOperationException(
                    "'defaultUserPassword' is not configured. Provide it via an environment variable or a secret store before running outside Development.");
            }

            return (Convert.ToBase64String(RandomNumberGenerator.GetBytes(18)), true);
        }

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices, string defaultUserPassword, bool isGeneratedPassword)
        {
            var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
            try {
                var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                var user = await userManager.FindByEmailAsync(defaultUserEmail);
                var resetDemoUser = _config.GetValue<bool>("resetDefaultUser");
                if (resetDemoUser && user != null) {
                    dbContext.Reports.RemoveRange(dbContext.Reports.Where(r => r.OwnerId == user.Id));
                    dbContext.SaveChanges();

                    await userManager.DeleteAsync(user);
                    user = null;
                }

                if (user == null) {
                    user = new IdentityUser() {
                        UserName = defaultUserEmail,
                        Email = defaultUserEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, defaultUserPassword);
                    if (result.Succeeded) {
                        await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                        var defaultReportsGenerator = new DefaultReportGenerator(_env, dbContext);
                        await defaultReportsGenerator.GenerateAsync(user);
                        ReportGeneratedPassword(isGeneratedPassword, defaultUserPassword);
                    }
                    else {
                        Console.WriteLine("Default user was not created: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
                else {
                    if (defaultUserPassword != PublishedDemoPassword
                        && await userManager.CheckPasswordAsync(user, PublishedDemoPassword)) {
                        // Rotate an account seeded by an earlier version that still accepts the published password.
                        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await userManager.ResetPasswordAsync(user, resetToken, defaultUserPassword);
                        if (result.Succeeded) {
                            Console.WriteLine($"Rotated the published password of the default user '{defaultUserEmail}'.");
                            ReportGeneratedPassword(isGeneratedPassword, defaultUserPassword);
                        }
                        else {
                            Console.WriteLine("Default user password was not rotated: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                        }
                    }

                    if (!await userManager.IsInRoleAsync(user, DefaultEqAuthProvider.EqManagerRole)) {
                        await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private static void ReportGeneratedPassword(bool isGeneratedPassword, string password)
        {
            if (isGeneratedPassword) {
                Console.WriteLine($"Development: default user '{defaultUserEmail}' has the generated password '{password}'. Set 'defaultUserPassword' to choose your own.");
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
