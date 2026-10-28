# EasyQuery ASP.NET Core + Angular 18 (ad-hoc reporting scenario)


## Overview

This samples consists of two projects: 

* AdHocReporting.Server.csproj - the backend web-service app that runs on ASP.NET Core 8.0
* AdHocReporting.Server.csproj - the client-side Angular 18 application

To start the sample you will need to run both of these two projects.

## Getting started

### 1. Start the backend app

* Open `AdHocReporting.Server` folder in a terminal (Command Prompt)
* Run `dotnet run` command

The backend seeds a default `eq-manager` user (`defaultUserEmail` in `appsettings.json`, `demo@korzh.com` by default).
Its password is never stored in the repository:

* Set it with `dotnet user-secrets set "defaultUserPassword" "<your-password>"` or the `defaultUserPassword` environment variable.
* If it is not set in the Development environment, a random password is generated on first run and printed to the console.
* Outside Development the app refuses to start until `defaultUserPassword` is configured (the old published value `demo` is rejected too).
  An existing default user that still accepts `demo` is switched to the configured password on startup.


### 2. Start the Angular app

* Open `AdHocReporting.Client` folder in a terminal (Command Prompt)
* Run `npm install` command to install NPM packages
* Run the application with `npm run start` command

Now you can open the application in your browser using this URL: [https://localhost:4200](https://localhost:4200)
 
