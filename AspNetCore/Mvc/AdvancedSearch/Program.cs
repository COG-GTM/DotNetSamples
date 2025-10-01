using Microsoft.EntityFrameworkCore;
using EasyData.Export;
using Korzh.EasyQuery.Services;
using EqDemo;
using EqDemo.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("EqDemoSqLite"))
);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddEasyQuery()
        .UseSqlManager()
        .AddDefaultExporters()
        .AddDataExporter<PdfDataExporter>("pdf")
        .AddDataExporter<ExcelDataExporter>("excel")
        .RegisterDbGate<Korzh.EasyQuery.DbGates.SqLiteGate>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapEasyQuery(options => {
    options.DefaultModelId = "nwind";
    options.SaveNewQuery = false;
    options.BuildQueryOnSync = true;
    options.UseDbContext<AppDbContext>();
    options.UseQueryStore((_) => new FileQueryStore("App_Data"));
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.EnsureDbInitialized(builder.Configuration, app.Environment);

app.Run();
