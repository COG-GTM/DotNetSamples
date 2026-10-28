# RazorTypeScript.AdHocReporting

## Default demo user

On startup the app creates a default `eq-manager` user only when its password is configured outside the source code:

```
dotnet user-secrets set "DemoUser:Password" "<choose-a-password>"
# or: export DemoUser__Password="<choose-a-password>"
```

The user's email comes from `DemoUser:Email` in `appsettings.json` (default `demo@korzh.com`).
Without `DemoUser:Password` no demo user is created, and an existing demo user that still has the
old published password `demo` has its password removed, so it can no longer sign in.
