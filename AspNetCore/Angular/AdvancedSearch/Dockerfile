# Build context = repo root (for nuget.config). See docker-compose.yml at repo root.
# `dotnet publish` runs `npm install && npm run build -- --prod` (Angular 12) via PublishRunWebpack.
# Angular 12 / webpack 5 builds on Node 16 (no --openssl-legacy-provider needed there).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY --from=node:16-bullseye /usr/local/bin /usr/local/bin
COPY --from=node:16-bullseye /usr/local/lib/node_modules /usr/local/lib/node_modules
WORKDIR /src
COPY nuget.config ./
COPY ["AspNetCore/Angular/AdvancedSearch/", "app/"]
WORKDIR /src/app
RUN dotnet restore "EqDemo.Angular.AdvancedSearch.csproj"
RUN dotnet publish "EqDemo.Angular.AdvancedSearch.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out ./
COPY ["AspNetCore/Angular/AdvancedSearch/App_Data/", "./App_Data/"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "EqDemo.Angular.AdvancedSearch.dll"]
