# Build context = repo root (for nuget.config). See docker-compose.yml at repo root.
# SPA project: `dotnet publish` runs `npm install && npm run build` (CRA) via an MSBuild target,
# so the build stage needs Node. The legacy toolchain (react-scripts) builds cleanly on Node 16.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Bring Node 16 into the .NET SDK image.
COPY --from=node:16-bullseye /usr/local/bin /usr/local/bin
COPY --from=node:16-bullseye /usr/local/lib/node_modules /usr/local/lib/node_modules
WORKDIR /src
COPY nuget.config ./
COPY ["AspNetCore/React/AdvancedSearch/", "app/"]
WORKDIR /src/app
RUN dotnet restore "EqDemo.AspNetCoreReact.AdvancedSearch.csproj"
RUN dotnet publish "EqDemo.AspNetCoreReact.AdvancedSearch.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /out ./
COPY ["AspNetCore/React/AdvancedSearch/App_Data/", "./App_Data/"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "EqDemo.AspNetCoreReact.AdvancedSearch.dll"]
