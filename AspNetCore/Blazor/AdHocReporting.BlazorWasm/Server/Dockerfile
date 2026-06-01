# Build context = repo root (for nuget.config). See docker-compose.yml at repo root.
# Blazor WebAssembly: the Server project also builds/hosts the Client (WASM) + Shared.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY ["AspNetCore/Blazor/AdHocReporting.BlazorWasm/", "app/"]
WORKDIR /src/app/Server
RUN dotnet restore "EqDemo.BlazorWasm.AdhocReporting.Server.csproj"
RUN dotnet publish "EqDemo.BlazorWasm.AdhocReporting.Server.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /out ./
COPY ["AspNetCore/Blazor/AdHocReporting.BlazorWasm/Server/App_Data/", "./App_Data/"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "EqDemo.BlazorWasm.AdhocReporting.Server.dll"]
