This is an ASP.NET Core (.NET 5) project that implements the [ad-hoc reporting scenario](https://korzh.com/demo/easyquery-asp-net-core-razor/adhoc-reporting). 

__Razor Pages__ is used as a view engine. You will find the main page in Pages/Index.cshtml.  
EasyQuery API is established by EasyQuery middleware (`endpoints.MapEasyQuery(...)` call in `Startup.cs`).

## Default demo user

The seeder creates a default `eq-manager` user only when its password is configured outside the source code:

```
dotnet user-secrets set "DemoUser:Password" "<choose-a-password>"
# or: export DemoUser__Password="<choose-a-password>"
```

The user's email comes from `DemoUser:Email` in `appsettings.json` (default `demo@korzh.com`).
