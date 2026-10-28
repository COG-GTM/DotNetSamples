# RazorTypeScript.AdHocReporting

## Default demo user

On startup the app creates a default `eq-manager` user only when its password is configured outside the source code:

```
dotnet user-secrets set "DemoUser:Password" "<choose-a-password>"
# or: export DemoUser__Password="<choose-a-password>"
```

The user's email comes from `DemoUser:Email` in `appsettings.json` (default `demo@korzh.com`).
On every start, an existing demo user that still accepts the old published password `demo` has that
password removed. Without a valid `DemoUser:Password` (missing, `demo`, or failing the password rules)
no demo user is created; once one is configured, a passwordless demo user gets it assigned.
