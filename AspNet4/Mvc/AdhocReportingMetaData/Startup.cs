using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Korzh.EasyQuery.Services;
using Korzh.EasyQuery.Db;

using EqDemo.Models;
using EqDemo.Services;

namespace EqDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            DbConnectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        public IConfiguration Configuration { get; }
        public string DbConnectionString { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(DbConnectionString)
            );

            services.AddDefaultIdentity<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddEasyQuery()
                    .UseSqlManager()
                    .AddDefaultExporters()
                    .RegisterDbGate<Korzh.EasyQuery.DbGates.SqlServerGate>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints => {
                endpoints.MapEasyQuery(options => {
                    options.DefaultModelId = "adhoc-reporting";
                    options.StoreModelInCache = true;
                    options.UseSessionCache();
                    options.SaveNewQuery = true;
                    options.ConnectionString = DbConnectionString;
                    options.UseDbContext<ApplicationDbContext>();
                    options.UseDbConnectionModelLoader(settings => {
                        settings.AddTableFilter(tbl => !tbl.Name.StartsWith("sys"));
                        settings.AddFieldFilter(fld => !fld.IsForeignKey);
                        settings.AddFieldFilter(fld => {
                            if (fld.Name == "CompanyName") fld.Name = "Customer Name";
                            return true;
                        });
                    });
                    options.UseQueryStore((_) => new FileQueryStore("App_Data"));
                });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
