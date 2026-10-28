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


### 2. Start the Angular app

* Open `AdHocReporting.Client` folder in a terminal (Command Prompt)
* Run `npm install` command to install NPM packages
* Run the application with `npm run start` command

Now you can open the application in your browser using this URL: [https://localhost:4200](https://localhost:4200)

### 3. Sign in

The reporting API (`/api/adhoc-reporting`) is available to signed-in users only, and each user sees only their own reports.
Anonymous visitors are redirected to the login page (anonymous API calls get `401 Unauthorized`).
Sign in with the seeded demo account (`defaultUserEmail` / `defaultUserPassword` in `appsettings.json`, `demo@korzh.com` / `demo` by default) or register a new user.
 
