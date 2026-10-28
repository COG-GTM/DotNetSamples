using System;
using System.Linq;

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

        public static string defaultUserEmail => GetDefaultUserEmail(_config);

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices, IConfiguration config, IWebHostEnvironment env)
        {
            var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
            var logger = scopedServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializeExtensions));

            try {
                var email = GetDefaultUserEmail(config);
                var configuredPassword = GetConfiguredPassword(config, env, logger);
                var user = await userManager.FindByEmailAsync(email);
                var resetDemoUser = config.GetValue<bool>("resetDefaultUser");
                if (resetDemoUser && user != null) {
                    var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                    dbContext.Reports.RemoveRange(dbContext.Reports.Where(r => r.OwnerId == user.Id));
                    dbContext.SaveChanges();

                    await userManager.DeleteAsync(user);
                    user = null;
                }

                if (user == null) {
                    // The default user is a well-known account, so only create it where it's wanted.
                    // With needAuth=false anonymous requests act as this user (see ReportStore), so it's required there.
                    if (!config.GetValue("seedDefaultUser", env.IsDevelopment() || !config.GetValue<bool>("needAuth"))) {
                        return;
                    }

                    user = new IdentityUser() {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, configuredPassword ?? GeneratePassword(email, env, logger));
                    if (result.Succeeded) {
                        await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                        var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                        var defaultReportsGenerator = new DefaultReportGenerator(env, dbContext);
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

        private static string GetConfiguredPassword(IConfiguration config, IWebHostEnvironment env, ILogger logger)
        {
            var password = config["defaultUserPassword"];
            if (string.IsNullOrEmpty(password)) {
                return null;
            }

            if (password == LegacyDefaultUserPassword && !env.IsDevelopment()) {
                logger.LogWarning("Ignoring the publicly known 'defaultUserPassword' value outside Development.");
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
        private static async Task EnsureDefaultUserPasswordAsync(UserManager<IdentityUser> userManager, IdentityUser user,
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

            await userManager.RemovePasswordAsync(user);
            var result = await userManager.AddPasswordAsync(user, newPassword);
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
