using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
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
                        await SetGeneratedPasswordMarkerAsync(userManager, user, isGeneratedPassword);
                        ReportGeneratedPassword(isGeneratedPassword, defaultUserPassword);
                    }
                    else {
                        Console.WriteLine("Default user was not created: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
                else {
                    if (defaultUserPassword != PublishedDemoPassword
                        && await userManager.CheckPasswordAsync(user, PublishedDemoPassword)) {
                        // Account seeded by an earlier version that still accepts the published password.
                        if (await TryReplacePasswordAsync(userManager, user, defaultUserPassword, isGeneratedPassword)) {
                            Console.WriteLine($"Rotated the published password of the default user '{defaultUserEmail}'.");
                        }
                        else {
                            await userManager.RemovePasswordAsync(user);
                            Console.WriteLine($"Removed the published password of the default user '{defaultUserEmail}'. Configure a valid 'defaultUserPassword' and restart to sign in.");
                        }
                    }
                    else if (!await userManager.HasPasswordAsync(user)) {
                        if (await TryReplacePasswordAsync(userManager, user, defaultUserPassword, isGeneratedPassword)) {
                            Console.WriteLine($"Set the password of the default user '{defaultUserEmail}'.");
                        }
                    }
                    else if (await HasUnchangedGeneratedPasswordAsync(userManager, user)) {
                        if (!isGeneratedPassword) {
                            if (await TryReplacePasswordAsync(userManager, user, defaultUserPassword, false)) {
                                Console.WriteLine($"Replaced the generated password of the default user '{defaultUserEmail}' with 'defaultUserPassword'.");
                            }
                        }
                        else {
                            Console.WriteLine($"Development: default user '{defaultUserEmail}' still has the password generated on its first run. Set 'defaultUserPassword' to replace it.");
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

        // Holds the security stamp from when a generated password was set, so a password the user changed afterwards is never overwritten.
        private const string GeneratedPasswordClaimType = "eqdemo:generated-password-stamp";

        private static async Task<bool> TryReplacePasswordAsync(UserManager<IdentityUser> userManager, IdentityUser user, string password, bool isGeneratedPassword)
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, resetToken, password);
            if (!result.Succeeded) {
                Console.WriteLine("Default user password was not changed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                return false;
            }

            await SetGeneratedPasswordMarkerAsync(userManager, user, isGeneratedPassword);
            ReportGeneratedPassword(isGeneratedPassword, password);
            return true;
        }

        private static async Task SetGeneratedPasswordMarkerAsync(UserManager<IdentityUser> userManager, IdentityUser user, bool isGeneratedPassword)
        {
            var markers = (await userManager.GetClaimsAsync(user)).Where(c => c.Type == GeneratedPasswordClaimType).ToList();
            if (markers.Count > 0) {
                await userManager.RemoveClaimsAsync(user, markers);
            }
            if (isGeneratedPassword) {
                await userManager.AddClaimAsync(user, new Claim(GeneratedPasswordClaimType, await userManager.GetSecurityStampAsync(user)));
            }
        }

        private static async Task<bool> HasUnchangedGeneratedPasswordAsync(UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var stamp = await userManager.GetSecurityStampAsync(user);
            return (await userManager.GetClaimsAsync(user)).Any(c => c.Type == GeneratedPasswordClaimType && c.Value == stamp);
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
