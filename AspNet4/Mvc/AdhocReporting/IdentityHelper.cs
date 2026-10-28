using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using EqDemo.Models;
using EqDemo.Services;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Web.Configuration;

namespace EqDemo
{
    public static class IdentityHelper
    {

        public static void SeedEqManagerRole()
        {

            const string eqManagerRole = "eq-manager";

            using (var context = ApplicationDbContext.Create())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                if (!roleManager.RoleExists(eqManagerRole)) {
                    roleManager.Create(new IdentityRole { Name = eqManagerRole });
                }
            }
        }

        public const string DefaultUserEmailFallback = "demo@korzh.com";

        // Published with earlier versions of this sample, so never left on a seeded account.
        private const string LegacyDefaultUserPassword = "demo";

        public static string DefaultUserEmail
        {
            get {
                var email = ConfigurationManager.AppSettings["defaultUserEmail"];
                return string.IsNullOrWhiteSpace(email) ? DefaultUserEmailFallback : email;
            }
        }

        // <compilation debug="true"> is this sample's development mode (Web.Release.config removes it).
        private static bool IsDevelopment
        {
            get {
                var compilation = ConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                return compilation != null && compilation.Debug;
            }
        }

        private static bool GetBoolSetting(string key, bool defaultValue)
        {
            bool value;
            return bool.TryParse(ConfigurationManager.AppSettings[key], out value) ? value : defaultValue;
        }

        public static void SeedDefaultUser()
        {
            var defaultUserEmail = DefaultUserEmail;
            var configuredPassword = GetConfiguredPassword();

            using (var dbContext = ApplicationDbContext.Create())
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                // Configure validation logic for passwords
                userManager.PasswordValidator = new PasswordValidator
                {
                    RequiredLength = 4,
                    RequireNonLetterOrDigit = false,
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireUppercase = false,
                };

                // The default user is a well-known account, so only create it where it's wanted.
                var seedDefaultUser = GetBoolSetting("seedDefaultUser", IsDevelopment);
                var resetDemoUser = GetBoolSetting("resetDefaultUser", false);

                var user = userManager.FindByEmail(defaultUserEmail);

                //remove default user if "resetDefaultUser" option is set to true
                if (resetDemoUser && seedDefaultUser && user != null) {
                    dbContext.Reports.RemoveRange(dbContext.Reports.Where(r => r.OwnerId == user.Id));
                    dbContext.SaveChanges();

                    userManager.DeleteAsync(user).GetAwaiter().GetResult();
                    user = null;
                }

                if (user == null) {
                    if (!seedDefaultUser) {
                        return;
                    }

                    user = new ApplicationUser {
                        Email = defaultUserEmail,
                        UserName = defaultUserEmail,
                        EmailConfirmed = true
                    };

                    var result = userManager.Create(user, configuredPassword ?? GeneratePassword(defaultUserEmail));
                    if (result.Succeeded) {
                        var reportGenerator = new DefaultReportGenerator(dbContext);
                        reportGenerator.Generate(user);
                    }
                    else {
                        Trace.TraceError("Could not create default user {0}: {1}", defaultUserEmail, string.Join("; ", result.Errors));
                    }
                }
                else {
                    EnsureDefaultUserPassword(userManager, user, configuredPassword);
                }
            }
        }

        private static string GetConfiguredPassword()
        {
            var password = ConfigurationManager.AppSettings["defaultUserPassword"];
            if (string.IsNullOrEmpty(password)) {
                return null;
            }

            if (password == LegacyDefaultUserPassword) {
                Trace.TraceWarning("Ignoring the publicly known 'defaultUserPassword' value.");
                return null;
            }

            return password;
        }

        private static string GeneratePassword(string email)
        {
            var bytes = new byte[18];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(bytes);
            }
            var password = Convert.ToBase64String(bytes);
            if (IsDevelopment) {
                Trace.TraceWarning("Generated password for default user {0}: {1} (set the 'defaultUserPassword' app setting to choose your own)", email, password);
            }
            else {
                Trace.TraceWarning("Generated a random password for default user {0}. Set the 'defaultUserPassword' app setting to choose one.", email);
            }
            return password;
        }

        // Applies 'defaultUserPassword' to an existing default user and replaces the legacy published password.
        private static void EnsureDefaultUserPassword(UserManager<ApplicationUser> userManager, ApplicationUser user, string configuredPassword)
        {
            string newPassword;
            if (configuredPassword != null) {
                if (userManager.CheckPassword(user, configuredPassword)) {
                    return;
                }
                newPassword = configuredPassword;
            }
            else if (userManager.CheckPassword(user, LegacyDefaultUserPassword)) {
                newPassword = GeneratePassword(user.Email);
            }
            else {
                return;
            }

            // Validate first so a rejected password never leaves the user without one.
            var result = userManager.PasswordValidator.ValidateAsync(newPassword).GetAwaiter().GetResult();
            if (result.Succeeded) {
                result = userManager.RemovePassword(user.Id);
            }
            if (result.Succeeded) {
                result = userManager.AddPassword(user.Id, newPassword);
            }
            if (!result.Succeeded) {
                Trace.TraceError("Could not set the password of default user {0}: {1}", user.Email, string.Join("; ", result.Errors));
            }
        }
    }
}
