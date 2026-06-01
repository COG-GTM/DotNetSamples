# Build context = repo root (for nuget.config). See docker-compose.yml at repo root.
# `dotnet build` runs `npm install && npm run build` (Rollup 4) via a PreBuild target.
# Rollup 4 requires Node 18+.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY --from=node:20-bookworm-slim /usr/local/bin /usr/local/bin
COPY --from=node:20-bookworm-slim /usr/local/lib/node_modules /usr/local/lib/node_modules
WORKDIR /src
COPY nuget.config ./
COPY ["AspNetCore/Razor-Mvc/RazorTypeScript.AdHocReporting/", "app/"]
WORKDIR /src/app
RUN dotnet restore "EqDemo.AspNetCoreRazorTypeScript.AdhocReporting.csproj"
RUN dotnet publish "EqDemo.AspNetCoreRazorTypeScript.AdhocReporting.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out ./
COPY ["AspNetCore/Razor-Mvc/RazorTypeScript.AdHocReporting/App_Data/", "./App_Data/"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "EqDemo.AspNetCoreRazorTypeScript.AdhocReporting.dll"]
