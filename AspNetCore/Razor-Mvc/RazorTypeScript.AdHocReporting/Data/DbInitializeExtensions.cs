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

namespace EqDemo
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
                    await CheckAddDefaultUserAsync(scope.ServiceProvider, config);
                }
            }
        }

        // Password seeded by older versions of this sample; only used to detect and retire accounts that still accept it.
        private const string PublishedLegacyCredential = "demo";

        private static async Task CheckAddDefaultUserAsync(IServiceProvider scopedServices, IConfiguration config)
        {
            var demoUser = DemoUserOptions.FromConfiguration(config);
            var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();

            try {
                var user = string.IsNullOrWhiteSpace(demoUser.Email) ? null : await userManager.FindByEmailAsync(demoUser.Email);
                if (user != null && await userManager.CheckPasswordAsync(user, PublishedLegacyCredential)) {
                    var removed = await userManager.RemovePasswordAsync(user);
                    Console.WriteLine(removed.Succeeded
                        ? "Published password removed from the existing default demo user."
                        : "Failed to remove the published password from the default demo user: " + DescribeErrors(removed));
                }

                if (!demoUser.IsConfigured) {
                    Console.WriteLine($"Default demo user is not created: set '{DemoUserOptions.PasswordKey}' via user-secrets or environment variables.");
                    return;
                }

                if (demoUser.Password == PublishedLegacyCredential || !await IsValidPasswordAsync(userManager, demoUser.Password)) {
                    Console.WriteLine($"'{DemoUserOptions.PasswordKey}' is the published password or does not satisfy the Identity password rules; default demo user not created.");
                    return;
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
                    user = new IdentityUser() {
                        UserName = demoUser.Email,
                        Email = demoUser.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, demoUser.Password);
                    if (result.Succeeded) {
                        await userManager.AddToRoleAsync(user, DefaultEqAuthProvider.EqManagerRole);
                        var defaultReportsGenerator = scopedServices.GetRequiredService<DefaultReportGenerator>();
                        await defaultReportsGenerator.GenerateAsync(user);
                    }
                    else {
                        Console.WriteLine("Default demo user was not created: " + DescribeErrors(result));
                    }
                }
                else {
                    if (!await userManager.HasPasswordAsync(user)) {
                        var added = await userManager.AddPasswordAsync(user, demoUser.Password);
                        if (!added.Succeeded) {
                            Console.WriteLine("Failed to set the configured password on the default demo user: " + DescribeErrors(added));
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

        private static string DescribeErrors(IdentityResult result)
            => string.Join("; ", result.Errors.Select(e => e.Description));

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
