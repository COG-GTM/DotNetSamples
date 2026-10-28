using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

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

        public const string DefaultUserEmailFallback = "demo@korzh.com";

        // Published with earlier versions of this sample, so never left on a seeded account.
        private const string LegacyDefaultUserPassword = "demo";

        public static string GetDefaultUserEmail(IConfiguration config)
        {
            var email = config["defaultUserEmail"];
            return string.IsNullOrWhiteSpace(email) ? DefaultUserEmailFallback : email;
        }

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices, IConfiguration config, IWebHostEnvironment env)
        {
            var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scopedServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializeExtensions));

            try {
                var email = GetDefaultUserEmail(config);
                var configuredPassword = GetConfiguredPassword(config, logger);
                var user = await userManager.FindByEmailAsync(email);
                // The default user is a well-known account, so only create it where it's wanted.
                var seedDefaultUser = config.GetValue("seedDefaultUser", env.IsDevelopment());
                var resetDemoUser = config.GetValue<bool>("resetDefaultUser");
                if (resetDemoUser && seedDefaultUser && user != null) {
                    var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                    dbContext.Reports.RemoveRange(dbContext.Reports.Where(r => r.OwnerId == user.Id));
                    dbContext.SaveChanges();

                    await userManager.DeleteAsync(user);
                    user = null;
                }

                if (user == null) {
                    if (!seedDefaultUser) {
                        return;
                    }

                    user = new ApplicationUser() {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, configuredPassword ?? GeneratePassword(email, env, logger));
                    if (result.Succeeded) {
                        await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                        var defaultReportsGenerator = scopedServices.GetRequiredService<DefaultReportGenerator>();
                        await defaultReportsGenerator.GenerateAsync(user);
                    }
                    else {
                        logger.LogError("Could not create default user {Email}: {Errors}", email,
                            string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
                else {
                    await EnsureDefaultUserPasswordAsync(userManager, user, configuredPassword, env, logger);
                    await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private static string GetConfiguredPassword(IConfiguration config, ILogger logger)
        {
            var password = config["defaultUserPassword"];
            if (string.IsNullOrEmpty(password)) {
                return null;
            }

            if (password == LegacyDefaultUserPassword) {
                logger.LogWarning("Ignoring the publicly known 'defaultUserPassword' value.");
                return null;
            }

            return password;
        }

        private static string GeneratePassword(string email, IWebHostEnvironment env, ILogger logger)
        {
            var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
            if (env.IsDevelopment()) {
                logger.LogWarning("Generated password for default user {Email}: {Password} (set 'defaultUserPassword' to choose your own)",
                    email, password);
            }
            else {
                logger.LogWarning("Generated a random password for default user {Email}. Set 'defaultUserPassword' to choose one.", email);
            }
            return password;
        }

        // Applies 'defaultUserPassword' to an existing default user and replaces the legacy published password.
        private static async Task EnsureDefaultUserPasswordAsync(UserManager<ApplicationUser> userManager, ApplicationUser user,
            string configuredPassword, IWebHostEnvironment env, ILogger logger)
        {
            string newPassword;
            if (configuredPassword != null) {
                if (await userManager.CheckPasswordAsync(user, configuredPassword)) {
                    return;
                }
                newPassword = configuredPassword;
            }
            else if (await userManager.CheckPasswordAsync(user, LegacyDefaultUserPassword)) {
                newPassword = GeneratePassword(user.Email, env, logger);
            }
            else {
                return;
            }

            // Validate first so a rejected password never leaves the user without one.
            foreach (var validator in userManager.PasswordValidators) {
                var validation = await validator.ValidateAsync(userManager, user, newPassword);
                if (!validation.Succeeded) {
                    logger.LogError("Could not set the password of default user {Email}: {Errors}", user.Email,
                        string.Join("; ", validation.Errors.Select(e => e.Description)));
                    return;
                }
            }

            var result = await userManager.RemovePasswordAsync(user);
            if (result.Succeeded) {
                result = await userManager.AddPasswordAsync(user, newPassword);
            }
            if (!result.Succeeded) {
                logger.LogError("Could not set the password of default user {Email}: {Errors}", user.Email,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
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
