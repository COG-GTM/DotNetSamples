# Build context = repo root (for nuget.config). See docker-compose.yml at repo root.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY ["AspNetCore/Razor-Mvc/Razor.AdHocReporting/EqDemo.AspNetCoreRazor.AdhocReporting.csproj", "app/"]
RUN dotnet restore "app/EqDemo.AspNetCoreRazor.AdhocReporting.csproj"
COPY ["AspNetCore/Razor-Mvc/Razor.AdHocReporting/", "app/"]
WORKDIR /src/app
RUN dotnet publish "EqDemo.AspNetCoreRazor.AdhocReporting.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out ./
COPY ["AspNetCore/Razor-Mvc/Razor.AdHocReporting/App_Data/", "./App_Data/"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "EqDemo.AspNetCoreRazor.AdhocReporting.dll"]
