FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Alerts.Api/Alerts.Api.csproj src/Alerts.Api/
COPY src/F1Monitor.Contracts/F1Monitor.Contracts.csproj src/F1Monitor.Contracts/
RUN dotnet restore src/Alerts.Api/Alerts.Api.csproj

COPY src/Alerts.Api/ src/Alerts.Api/
COPY src/F1Monitor.Contracts/ src/F1Monitor.Contracts/

WORKDIR /src/src/Alerts.Api
RUN dotnet publish Alerts.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Alerts.Api.dll"]
