using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Korzh.DbUtils;

using Korzh.EasyQuery.Services;

using EqDemo.Data;
using EqDemo.Models;

namespace EqDemo.Services
{
    public static class DbInitializeExtensions
    {
        public static async Task EnsureDbInitializedAsync(this IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = scope.ServiceProvider.GetService<AppDbContext>()) {
                if (context.Database.EnsureCreated()) {
                    Korzh.DbUtils.DbInitializer.Create(options => {
                        options.UseSqlite(config.GetConnectionString("EqDemoSqLite"));
                        //options.UseSqlServer(config.GetConnectionString("EqDemoDb"));
                        options.UseZipPacker(System.IO.Path.Combine(env.ContentRootPath, "App_Data", "EqDemoData.zip"));
                    })
                    .Seed();
                }

                if (context.Database.CanConnect()) {
                    //create eq-manager role
                    await CheckAddManagerRoleAsync(scope.ServiceProvider);

                    //create default user
                    await CheckAddDefaultUserAsync(scope.ServiceProvider, config, env);
                }
            }
        }

        // Password seeded by older versions of this sample; used only to find and retire accounts that still accept it.
        private const string LegacyPublishedPassword = "demo";

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices, IConfiguration config, IWebHostEnvironment env)
        {
            var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scopedServices.GetRequiredService<ILoggerFactory>().CreateLogger("EqDemo.DbInitialize");
            var demoUser = DemoUserOptions.FromConfiguration(config);

            try {
                var user = await userManager.FindByEmailAsync(demoUser.Email);
                var acceptsLegacyPassword = user != null && await userManager.CheckPasswordAsync(user, LegacyPublishedPassword);

                if (!env.IsDevelopment()) {
                    if (acceptsLegacyPassword) {
                        await userManager.RemovePasswordAsync(user);
                        logger.LogWarning("Password sign-in disabled for the default demo user {Email}: it still accepted the published sample password.", demoUser.Email);
                    }
                    logger.LogInformation("Default demo user is seeded only in the Development environment.");
                    return;
                }

                var password = demoUser.Password;
                var passwordGenerated = string.IsNullOrEmpty(password);
                if (passwordGenerated) {
                    password = GeneratePassword();
                }

                var resetDemoUser = config.GetValue<bool>("resetDefaultUser");
                if (resetDemoUser && user != null) {
                    var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                    dbContext.Reports.RemoveRange(dbContext.Reports.Where(r => r.OwnerId == user.Id));
                    dbContext.SaveChanges();

                    await userManager.DeleteAsync(user);
                    user = null;
                }

                if (user == null) {
                    user = new ApplicationUser() {
                        UserName = demoUser.Email,
                        Email = demoUser.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded) {
                        await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                        var defaultReportsGenerator = scopedServices.GetRequiredService<DefaultReportGenerator>();
                        await defaultReportsGenerator.GenerateAsync(user);
                        LogSeededPassword(logger, demoUser.Email, password, passwordGenerated);
                    }
                    else {
                        logger.LogWarning("Default demo user was not created: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
                else {
                    var rotate = acceptsLegacyPassword
                        || (!passwordGenerated && !await userManager.CheckPasswordAsync(user, password));
                    if (rotate) {
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await userManager.ResetPasswordAsync(user, token, password);
                        if (result.Succeeded) {
                            LogSeededPassword(logger, demoUser.Email, password, passwordGenerated);
                        }
                        else {
                            logger.LogWarning("Default demo user password was not updated: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
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

        private static string GeneratePassword()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(18))
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static void LogSeededPassword(ILogger logger, string email, string password, bool generated)
        {
            if (generated) {
                logger.LogWarning("Default demo user {Email} seeded with generated password {Password}. Set '{Key}' (user-secrets or environment) to choose your own.",
                    email, password, DemoUserOptions.PasswordKey);
            }
            else {
                logger.LogInformation("Default demo user {Email} seeded with the password from '{Key}'.", email, DemoUserOptions.PasswordKey);
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
