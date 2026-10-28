using Microsoft.Extensions.Configuration;

namespace EqDemo.Services
{
    public sealed class DemoUserOptions
    {
        public const string SectionName = "DemoUser";
        public const string PasswordKey = SectionName + ":Password";
        private const string DefaultEmail = "demo@korzh.com";

        public string Email { get; init; }
        public string Password { get; init; }

        public static DemoUserOptions FromConfiguration(IConfiguration config)
        {
            var section = config.GetSection(SectionName);
            var email = section["Email"];
            return new DemoUserOptions {
                Email = string.IsNullOrWhiteSpace(email) ? DefaultEmail : email,
                Password = section["Password"]
            };
        }
    }
}
