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

        // Value seeded by older versions of this sample; only used to detect and retire accounts that still accept it.
        private const string PublishedLegacyCredential = "demo";

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices)
        {
            var demoUser = DemoUserOptions.FromConfiguration(_config);
            var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
            try {
                var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                var user = string.IsNullOrWhiteSpace(demoUser.Email) ? null : await userManager.FindByEmailAsync(demoUser.Email);

                var usesPublishedCredential = user != null && await userManager.CheckPasswordAsync(user, PublishedLegacyCredential);

                if (!demoUser.IsConfigured) {
                    // Accounts seeded by older versions still accept the credential that was published in the source code.
                    if (usesPublishedCredential) {
                        await userManager.RemovePasswordAsync(user);
                        Console.WriteLine("Password sign-in disabled for the existing default demo user.");
                    }
                    Console.WriteLine($"Default demo user is not created: set '{DemoUserOptions.PasswordKey}' via user-secrets or environment variables.");
                    return;
                }

                if (!await IsValidPasswordAsync(userManager, demoUser.Password)) {
                    Console.WriteLine($"'{DemoUserOptions.PasswordKey}' does not satisfy the Identity password rules; default demo user left unchanged.");
                    return;
                }

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
                    else {
                        Console.WriteLine("Default demo user was not created: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
                else {
                    if (usesPublishedCredential && !await userManager.CheckPasswordAsync(user, demoUser.Password)) {
                        // Rotate an account seeded by older versions to the configured password; other accounts keep theirs.
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        await userManager.ResetPasswordAsync(user, token, demoUser.Password);
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

        private static async Task<bool> IsValidPasswordAsync(UserManager<IdentityUser> userManager, string password)
        {
            foreach (var validator in userManager.PasswordValidators) {
                var result = await validator.ValidateAsync(userManager, null, password);
                if (!result.Succeeded) {
                    return false;
                }
            }
            return true;
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
