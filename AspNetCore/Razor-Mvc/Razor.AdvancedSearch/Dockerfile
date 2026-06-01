# Build context = repo root (for nuget.config). See docker-compose.yml at repo root.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY ["AspNetCore/Razor-Mvc/Razor.AdvancedSearch/EqDemo.AspNetCoreRazor.AdvancedSearch.csproj", "app/"]
RUN dotnet restore "app/EqDemo.AspNetCoreRazor.AdvancedSearch.csproj"
COPY ["AspNetCore/Razor-Mvc/Razor.AdvancedSearch/", "app/"]
WORKDIR /src/app
RUN dotnet publish "EqDemo.AspNetCoreRazor.AdvancedSearch.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out ./
# App_Data (Northwind sample DB seed + query store) is not published by default.
COPY ["AspNetCore/Razor-Mvc/Razor.AdvancedSearch/App_Data/", "./App_Data/"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "EqDemo.AspNetCoreRazor.AdvancedSearch.dll"]
