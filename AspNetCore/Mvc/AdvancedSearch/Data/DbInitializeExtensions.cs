using Microsoft.EntityFrameworkCore;
using Korzh.DbUtils;
using EqDemo.Data;

namespace EqDemo
{
    public static class DbInitializeExtensions
    {
        public static void EnsureDbInitialized(this WebApplication app, IConfiguration config, IWebHostEnvironment env)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (context.Database.EnsureCreated())
            {
                Korzh.DbUtils.DbInitializer.Create(options => {
                    options.UseSqlite(config.GetConnectionString("EqDemoSqLite"));
                    options.UseZipPacker(Path.Combine(env.ContentRootPath, "App_Data", "EqDemoData.zip"));
                })
                .Seed();
            }
        }
    }
}
