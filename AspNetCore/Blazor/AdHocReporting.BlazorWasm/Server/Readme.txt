This code includes a dependency on Duende IdentityServer.
This is an open source product with a reciprocal license agreement. If you plan to use Duende IdentityServer in production this may require a license fee.
To see how to use Azure Active Directory for your identity please see https://aka.ms/aspnetidentityserver
To see if you require a commercial license for Duende IdentityServer please see https://aka.ms/identityserverlicense

Default demo user
-----------------
The default "eq-manager" user (DemoUser:Email in appsettings.json) is seeded only in the Development environment.
Its password is not stored in the source code. Set it with:
    dotnet user-secrets set "DemoUser:Password" "<choose-a-password>"
or the DemoUser__Password environment variable. When it is not set, a random password is generated and written
to the server log. Outside Development no demo user is seeded, and an existing demo user that still accepts the
password published by older versions of this sample has its password removed.
