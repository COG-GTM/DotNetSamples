using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;

using EasyData.Export;
using Korzh.EasyQuery.Services;

using EqDemo;
using EqDemo.Models;
using EqDemo.Services;
using EqDemo.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(builder.Configuration.GetConnectionString("EqDemoDb"));
});

builder.Services.AddDefaultIdentity<IdentityUser>(opts => {
    opts.Password.RequiredLength = 4;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireDigit = false;
})
 .AddRoles<IdentityRole>()
 .AddDefaultUI()
 .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<CookiePolicyOptions>(options => {
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.ConfigureApplicationCookie(options => {
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

builder.Services.AddCors(options => {
    options.AddPolicy(name: "AllowAllPolicy",
        builder => {
            builder.AllowAnyOrigin();
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.WithExposedHeaders("Content-Disposition");
        });
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddEasyQuery()
                .AddDefaultExporters()
                .AddDataExporter<PdfDataExporter>("pdf")
                .AddDataExporter<ExcelDataExporter>("excel")
                .UseSessionCache()
                .UseSqlManager();

builder.Services.AddScoped<DefaultReportGenerator>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}
else {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

var redirectOptions = new RewriteOptions()
    .AddRedirect("(?i:identity/account/forgotpassword(/.*)?$)", "/")
    .AddRedirect("(?i:identity/account/manage(/.*)?$)", "/");
app.UseRewriter(redirectOptions);

app.UseHttpsRedirection();
app.UseCors("AllowAllPolicy");
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapEasyQuery(options => {
    options.DefaultModelId = "adhoc-reporting";
    options.SaveNewQuery = true;
    options.SaveQueryOnSync = true;
    options.Endpoint = "/api/adhoc-reporting";
    options.StoreModelInCache = true;
    options.StoreQueryInCache = true;

    options.UseDbContextWithoutIdentity<AppDbContext>(loaderOptions => {
        loaderOptions.AddFilter(entity => {
            return entity.ClrType != typeof(Report);
        });
    });

    options.UseQueryStore((manager) => new ReportStore(manager.Services));

    options.UseDefaultAuthProvider((provider) => {
        provider.RequireRole("eq-manager", EqAction.NewQuery, EqAction.SaveQuery, EqAction.RemoveQuery);
    });

    options.AddPreFetchTunerWithHttpContext((manager, context) => {
    });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.EnsureDbInitializedAsync(builder.Configuration, app.Environment)
    .GetAwaiter()
    .GetResult();

app.Run();
