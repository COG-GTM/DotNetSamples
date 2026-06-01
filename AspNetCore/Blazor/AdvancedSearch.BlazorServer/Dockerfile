# Build context = repo root (for nuget.config). See docker-compose.yml at repo root.
# net6.0 project built with the .NET 8 SDK; runs on the .NET 6 ASP.NET runtime.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY ["AspNetCore/Blazor/AdvancedSearch.BlazorServer/EqDemo.BlazorServer.AdvancedSearch.csproj", "app/"]
RUN dotnet restore "app/EqDemo.BlazorServer.AdvancedSearch.csproj"
COPY ["AspNetCore/Blazor/AdvancedSearch.BlazorServer/", "app/"]
WORKDIR /src/app
RUN dotnet publish "EqDemo.BlazorServer.AdvancedSearch.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /out ./
COPY ["AspNetCore/Blazor/AdvancedSearch.BlazorServer/App_Data/", "./App_Data/"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "EqDemo.BlazorServer.AdvancedSearch.dll"]
