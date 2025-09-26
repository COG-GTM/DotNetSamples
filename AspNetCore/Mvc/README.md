# ASP.NET Core MVC Migration

This directory contains modernized ASP.NET Core versions of the ASP.NET 4.x MVC projects.

## Projects

### AdhocReporting
- **Source**: `AspNet4/Mvc/AdhocReporting/`
- **Target Framework**: .NET 8.0
- **Key Changes**:
  - Replaced `EasyQueryApiController` inheritance with `MapEasyQuery()` middleware
  - Converted OWIN-based identity to ASP.NET Core Identity
  - Updated Entity Framework 6.x to Entity Framework Core
  - Replaced `ApplicationDbContext.Create()` with dependency injection
  - Modernized `ReportStore` service with proper DI patterns

### AdvancedSearch
- **Source**: `AspNet4/Mvc/AdvancedSearch/`
- **Target Framework**: .NET 8.0
- **Key Changes**:
  - Replaced `EasyQueryApiController` inheritance with `MapEasyQuery()` middleware
  - Updated Entity Framework 6.x to Entity Framework Core
  - Replaced `ApplicationDbContext.Create()` with dependency injection
  - Simplified configuration without authentication requirements

## Package Migration

| ASP.NET 4.x Package | ASP.NET Core Package |
|---------------------|---------------------|
| `Korzh.EasyQuery.AspNet4` | `Korzh.EasyQuery.AspNetCore` |
| `Korzh.EasyQuery.EntityFramework6` | `Korzh.EasyQuery.EntityFrameworkCore.Relational` |
| `Microsoft.AspNet.Identity.Owin` | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| `EntityFramework` | `Microsoft.EntityFrameworkCore.Sqlite/SqlServer` |
| `System.Data.SqlClient` | `Microsoft.Data.SqlClient` |

## Architecture Changes

### Controller Pattern → Middleware Pattern
**Before (ASP.NET 4.x)**:
```csharp
[RoutePrefix("api/adhoc-reporting")]
[Authorize]
public class EasyReportController : EasyQueryApiController
{
    protected override void ConfigureEasyQueryOptions(EasyQueryOptions options)
    {
        // Configuration
    }
}
```

**After (ASP.NET Core)**:
```csharp
app.MapEasyQuery(options => {
    options.Endpoint = "/api/adhoc-reporting";
    options.DefaultModelId = "adhoc-reporting";
    // Configuration
});
```

### Dependency Injection
**Before (ASP.NET 4.x)**:
```csharp
var dbContext = ApplicationDbContext.Create();
```

**After (ASP.NET Core)**:
```csharp
// In Program.cs
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(connectionString);
});

// In services
public ReportStore(IServiceProvider services)
{
    _dbContext = services.GetRequiredService<AppDbContext>();
}
```

### Authentication
**Before (ASP.NET 4.x)**:
```csharp
HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>()
```

**After (ASP.NET Core)**:
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
```

## Running the Projects

1. Navigate to the project directory
2. Run `dotnet restore` to restore packages
3. Run `dotnet run` to start the application
4. The EasyQuery endpoints will be available at:
   - AdhocReporting: `/api/adhoc-reporting`
   - AdvancedSearch: `/api/easyquery`
