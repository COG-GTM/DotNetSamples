using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using Korzh.EasyQuery.Services;
using EasyData.Export;

namespace EqDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => {
                options.UseSqlite(Configuration.GetConnectionString("EqDemoSqLite"));
                //options.UseSqlServer(Configuration.GetConnectionString("EqDemoDb"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowAllPolicy",
                    builder => {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                        builder.WithExposedHeaders("Content-Disposition");
                    });
            });

            services.AddControllersWithViews();

            services.AddEasyQuery()
                    .UseSqlManager()
                    .AddDefaultExporters()
                    .AddDataExporter<PdfDataExporter>("pdf")
                    .AddDataExporter<ExcelDataExporter>("excel");
                
                  // Uncomment if you want to load model directly from DB               
                  // .RegisterDbGate<SqLiteGate>();
                  // .RegisterDbGate<SqlServerGate>();

            //to support non-Unicode code pages in PDF Exporter
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("AllowAllPolicy");

            app.UseHttpsRedirection();
            // In production, prebuilt SPA assets are copied into wwwroot by the
            // PublishRunWebpack target and served via the default static-files middleware.
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapEasyQuery(options => {
                    options.DefaultModelId = "nwind";

                    options.SaveNewQuery = false;

                    options.UseDbContext<AppDbContext>();

                    // Uncomment if you want to donwload model directly from DB
                    // options.UseDbConnectionModelLoader();

                    options.UseQueryStore((_) => new FileQueryStore("App_Data"));
                });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                // SPA fallback: serve the prebuilt index.html for client-side routes in production.
                // In Development the SPA is launched via Microsoft.AspNetCore.SpaProxy using the
                // SpaProxyServerUrl/SpaProxyLaunchCommand MSBuild properties from the .csproj.
                endpoints.MapFallbackToFile("index.html");
            });

            //Init demo database (if necessary)
            app.EnsureDbInitialized(Configuration, env);
        }
    }
}
