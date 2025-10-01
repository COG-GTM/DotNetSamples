# .NET Migration Playbook: ASP.NET 4.x to ASP.NET Core

## Overview
This playbook provides a comprehensive guide for migrating ASP.NET 4.x MVC applications to modern ASP.NET Core, based on successful migration of EasyQuery-based applications.

## Prerequisites
- Target Framework: ASP.NET Core (.NET 8.0 recommended)
- Source: ASP.NET MVC 5.x with .NET Framework 4.x
- Understanding of dependency injection patterns
- Familiarity with Entity Framework Core

## Migration Strategy

### Phase 1: Analysis and Planning

#### 1.1 Inventory Current Architecture
```bash
# Examine existing controllers
find . -name "*Controller.cs" -path "*/AspNet4/*" | head -10

# Check package dependencies
grep -r "PackageReference\|packages.config" --include="*.csproj" --include="*.config"

# Identify OWIN usage
grep -r "GetOwinContext\|Startup.cs" --include="*.cs"
```

#### 1.2 Priority Assessment
**High Priority Controllers:**
- Controllers inheriting from `EasyQueryApiController`
- Controllers using `[RoutePrefix]` attributes
- Controllers with OWIN-based identity (`HttpContext.GetOwinContext()`)

**Medium Priority:**
- Standard MVC controllers
- Controllers with minimal dependencies

#### 1.3 Package Mapping
| ASP.NET 4.x Package | ASP.NET Core Equivalent |
|---------------------|-------------------------|
| `Korzh.EasyQuery.AspNet4` | `Korzh.EasyQuery.AspNetCore` |
| `Korzh.EasyQuery.EntityFramework6` | `Korzh.EasyQuery.EntityFrameworkCore.Relational` |
| `Microsoft.AspNet.Identity.Owin` | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| `Microsoft.Owin.Security.*` | `Microsoft.AspNetCore.Authentication.*` |
| `EntityFramework` | `Microsoft.EntityFrameworkCore.*` |
| `System.Data.SqlClient` | `Microsoft.Data.SqlClient` |

### Phase 2: Project Structure Setup

#### 2.1 Create New ASP.NET Core Project Structure
```
AspNetCore/Mvc/{ProjectName}/
├── Controllers/
├── Data/
├── Models/
├── Services/
├── Views/
├── Program.cs
├── appsettings.json
└── {ProjectName}.csproj
```

#### 2.2 Project File Template (.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.2" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.2" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.2" />
  <PackageReference Include="Korzh.EasyQuery.AspNetCore" Version="7.4.0" />
  <PackageReference Include="Korzh.EasyQuery.EntityFrameworkCore.Relational" Version="7.4.0" />
  <PackageReference Include="Korzh.DbUtils" Version="5.1.1" />
</Project>
```

### Phase 3: Core Infrastructure Migration

#### 3.1 Program.cs Configuration Template
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Korzh.EasyQuery.Services;
using EqDemo.Data;
using EqDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EqDemoDb")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

// EasyQuery services
builder.Services.AddEasyQuery()
    .UseSqlite(builder.Configuration.GetConnectionString("EqDemoDb"));

builder.Services.AddScoped<ReportStore>();
builder.Services.AddScoped<DefaultReportGenerator>();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// EasyQuery middleware
app.MapEasyQuery("/api/adhoc-reporting", options => {
    options.UseDbContext<AppDbContext>();
    options.UseManager<EqDemo.Services.EqAuthProvider>();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Initialize database
await app.EnsureDbInitializedAsync(app.Configuration, app.Environment);

app.Run();
```

#### 3.2 Database Context Migration
```csharp
// From: ApplicationDbContext.Create() pattern
public static ApplicationDbContext Create()
{
    return new ApplicationDbContext();
}

// To: Dependency injection pattern
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Report> Reports { get; set; }
    // ... other DbSets
}
```

### Phase 4: Controller Migration Patterns

#### 4.1 EasyQuery Controller Migration
```csharp
// FROM: ASP.NET 4.x Controller
[RoutePrefix("api/adhoc-reporting")]
[Authorize]
public class EasyReportController : EasyQueryApiController
{
    protected override DbContext GetDbContext()
    {
        return ApplicationDbContext.Create();
    }
}

// TO: ASP.NET Core Middleware Configuration
// Remove controller entirely, replace with middleware in Program.cs:
app.MapEasyQuery("/api/adhoc-reporting", options => {
    options.UseDbContext<AppDbContext>();
    options.UseManager<EqAuthProvider>();
});
```

#### 4.2 Identity Controller Migration
```csharp
// FROM: OWIN-based Identity
public class AccountController : Controller
{
    private ApplicationSignInManager _signInManager;
    private ApplicationUserManager _userManager;

    public ApplicationSignInManager SignInManager
    {
        get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        private set => _signInManager = value;
    }
}

// TO: ASP.NET Core Identity with DI
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }
}
```

#### 4.3 Service Migration Pattern
```csharp
// FROM: Manual instantiation
public class ReportStore
{
    public void SaveReport(Report report)
    {
        using (var context = ApplicationDbContext.Create())
        {
            // operations
        }
    }
}

// TO: Dependency injection
public class ReportStore
{
    private readonly AppDbContext _context;

    public ReportStore(AppDbContext context)
    {
        _context = context;
    }

    public void SaveReport(Report report)
    {
        // operations using _context
    }
}
```

### Phase 5: Database Initialization Migration

#### 5.1 DbInitializer Pattern Update
```csharp
// FROM: Direct DbInitializer usage
var dbInitializer = new DbInitializer(context.Database.GetDbConnection());
await dbInitializer.SeedAsync(dataPath);

// TO: Options-based pattern
Korzh.DbUtils.DbInitializer.Create(options => {
    options.UseSqlite(connectionString);
    options.UseZipPacker(dataPath);
})
.Seed();
```

### Phase 6: Model Updates

#### 6.1 Navigation Properties
Ensure Entity Framework Core navigation properties are properly defined:
```csharp
public class Order
{
    // Existing properties...
    public virtual List<OrderDetail> Items { get; set; }
    
    // Add for EF Core compatibility
    public virtual List<OrderDetail> OrderDetails { get; set; }
}

public class Product
{
    // Existing properties...
    
    // Add navigation property
    public virtual List<OrderDetail> OrderDetails { get; set; }
}
```

#### 6.2 Identity Model Updates
```csharp
// FROM: Custom ApplicationUser
public class ApplicationUser : IdentityUser
{
    // Custom properties
}

// TO: Standard IdentityUser (if no custom properties needed)
// Use IdentityUser directly, or extend if custom properties required
```

### Phase 7: Configuration Migration

#### 7.1 Connection Strings (appsettings.json)
```json
{
  "ConnectionStrings": {
    "EqDemoDb": "Data Source=EqDemo.db",
    "EqDemoSqLite": "Data Source=EqDemo.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Phase 8: View Migration

#### 8.1 Layout Updates
```html
<!-- Update Bootstrap and jQuery references -->
<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
<script src="~/lib/jquery/dist/jquery.min.js"></script>
<script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>

<!-- Add ASP.NET Core tag helpers -->
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

### Phase 9: Testing and Verification

#### 9.1 Build Verification
```bash
# Test compilation
dotnet build

# Check for warnings
dotnet build --verbosity normal

# Restore packages
dotnet restore
```

#### 9.2 Runtime Testing Checklist
- [ ] Application starts without errors
- [ ] Database initializes correctly
- [ ] EasyQuery endpoints respond
- [ ] Authentication/authorization works
- [ ] All views render properly

### Phase 10: Common Issues and Solutions

#### 10.1 Compilation Errors
**Error**: `The type or namespace name 'AspNet' does not exist`
**Solution**: Update using statements from `Microsoft.AspNet.*` to `Microsoft.AspNetCore.*`

**Error**: `'DbInitializer' does not contain a definition for 'SeedAsync'`
**Solution**: Use the options-based DbInitializer.Create() pattern

**Error**: Navigation property not found
**Solution**: Add missing navigation properties to model classes

#### 10.2 Runtime Issues
**Issue**: EasyQuery endpoints not working
**Solution**: Ensure middleware is properly configured with correct path and options

**Issue**: Authentication not working
**Solution**: Verify Identity services are registered and middleware order is correct

### Phase 11: Git Workflow

#### 11.1 Branch Strategy
```bash
# Create feature branch
git checkout -b devin/$(date +%s)-aspnet4-to-core-migration

# Stage specific files (never use git add .)
git add AspNetCore/Mvc/

# Commit with descriptive message
git commit -m "Migrate ASP.NET 4.x MVC projects to ASP.NET Core

- Create new AspNetCore/Mvc/{ProjectName} projects targeting .NET 8.0
- Replace EasyQueryApiController inheritance with MapEasyQuery middleware
- Convert OWIN-based identity to ASP.NET Core Identity
- Update Entity Framework 6.x to Entity Framework Core
- Replace ApplicationDbContext.Create() with dependency injection
- Update package references to ASP.NET Core equivalents"

# Push and create PR
git push origin devin/$(date +%s)-aspnet4-to-core-migration
```

#### 11.2 PR Template
```markdown
# Migrate ASP.NET 4.x MVC Projects to Modern ASP.NET Core

## Summary
This PR migrates ASP.NET 4.x MVC projects to modern ASP.NET Core (.NET 8.0).

### Key Architecture Changes:
- **Controller → Middleware**: Replaced EasyQueryApiController inheritance with MapEasyQuery middleware
- **OWIN → Core Identity**: Converted HttpContext.GetOwinContext() to dependency injection
- **EF6 → EF Core**: Updated Entity Framework 6.x to Entity Framework Core
- **Manual DI → Built-in DI**: Replaced ApplicationDbContext.Create() with constructor injection

### Package Migrations:
- `Korzh.EasyQuery.AspNet4` → `Korzh.EasyQuery.AspNetCore`
- `Korzh.EasyQuery.EntityFramework6` → `Korzh.EasyQuery.EntityFrameworkCore.Relational`
- `Microsoft.AspNet.Identity.Owin` → `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

### Verification:
- [x] Both projects compile successfully (`dotnet build`)
- [x] All package references updated
- [x] EasyQuery middleware properly configured
- [x] Identity services integrated
```

## Best Practices

### Do's
- ✅ Always target the latest stable .NET version (.NET 8.0)
- ✅ Use dependency injection throughout
- ✅ Follow ASP.NET Core middleware patterns
- ✅ Maintain existing functionality
- ✅ Test compilation after each major change
- ✅ Use existing ASP.NET Core samples as templates

### Don'ts
- ❌ Don't modify unrelated code
- ❌ Don't fix warnings unrelated to migration
- ❌ Don't add unnecessary packages
- ❌ Don't use `git add .` (stage files explicitly)
- ❌ Don't skip build verification
- ❌ Don't break existing functionality

## Troubleshooting Guide

### Build Failures
1. Check using statements are updated
2. Verify all package references are ASP.NET Core compatible
3. Ensure navigation properties are properly defined
4. Confirm DbInitializer API usage is correct

### Runtime Issues
1. Verify middleware registration order
2. Check connection strings in appsettings.json
3. Ensure Identity services are properly configured
4. Validate EasyQuery middleware configuration

## Migration Checklist

### Pre-Migration
- [ ] Analyze existing controllers and dependencies
- [ ] Identify OWIN usage patterns
- [ ] Map package dependencies
- [ ] Create migration plan

### During Migration
- [ ] Create new ASP.NET Core project structure
- [ ] Update project files with correct package references
- [ ] Migrate Program.cs configuration
- [ ] Convert controllers to middleware where applicable
- [ ] Update service registration patterns
- [ ] Migrate database context and models
- [ ] Update views and layouts

### Post-Migration
- [ ] Verify compilation (`dotnet build`)
- [ ] Test basic functionality
- [ ] Create comprehensive PR
- [ ] Document migration patterns
- [ ] Update README files

## Success Metrics
- All projects compile without errors
- EasyQuery functionality preserved
- Authentication/authorization working
- Database operations functional
- Views render correctly
- No regression in existing features

---

*This playbook is based on successful migration of EasyQuery-based ASP.NET 4.x applications to ASP.NET Core .NET 8.0.*
