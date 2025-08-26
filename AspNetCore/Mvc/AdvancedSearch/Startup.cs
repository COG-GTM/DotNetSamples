using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using EasyData;
using EasyData.Export;

using Korzh.EasyQuery.Services;
using Korzh.EasyQuery.Db;

using EqDemo.Services;
using EqDemo.Models;
using Microsoft.Data.SqlClient;

namespace EqDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            DbConnectionString = Configuration.GetConnectionString("EqDemoDb");
        }

        public IConfiguration Configuration { get; }

        public string DbConnectionString { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(DbConnectionString)
            );

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddEasyQuery()
                    .UseSqlManager()
                    .AddDefaultExporters()
                    .AddDataExporter<PdfDataExporter>("pdf")
                    .AddDataExporter<ExcelDataExporter>("excel")
                    .UseSessionCache()
                    .RegisterDbGate<Korzh.EasyQuery.DbGates.SqlServerGate>();

            services.AddControllersWithViews();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapEasyQuery(options => {
                    options.DefaultModelId = "nwind";
                    options.BuildQueryOnSync = true;
                    options.SaveNewQuery = false;
                    options.ConnectionString = DbConnectionString;
                    options.UseDbContext<ApplicationDbContext>();
                    options.StoreModelInCache = true;
                    options.StoreQueryInCache = true;

                    options.UseQueryStore((_) => new FileQueryStore("App_Data"));
                });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.EnsureDbInitialized(DbConnectionString, env);
        }
    }
}
