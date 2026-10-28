using Microsoft.Extensions.Configuration;

namespace EqDemo.Services
{
    public sealed class DemoUserOptions
    {
        public const string SectionName = "DemoUser";
        public const string PasswordKey = SectionName + ":Password";

        public string Email { get; init; }
        public string Password { get; init; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(Password);

        public static DemoUserOptions FromConfiguration(IConfiguration config)
        {
            var section = config.GetSection(SectionName);
            return new DemoUserOptions {
                Email = section["Email"],
                Password = section["Password"]
            };
        }
    }
}
