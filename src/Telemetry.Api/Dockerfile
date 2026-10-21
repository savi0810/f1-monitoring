FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Telemetry.Api/Telemetry.Api.csproj src/Telemetry.Api/
RUN dotnet restore src/Telemetry.Api/Telemetry.Api.csproj

COPY src/Telemetry.Api/ src/Telemetry.Api/

WORKDIR /src/src/Telemetry.Api
RUN dotnet publish Telemetry.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Telemetry.Api.dll"]
