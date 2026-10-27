FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/F1.Collector/F1.Collector.csproj src/F1.Collector/
RUN dotnet restore src/F1.Collector/F1.Collector.csproj

COPY src/F1.Collector/ src/F1.Collector/

WORKDIR /src/src/F1.Collector
RUN dotnet publish F1.Collector.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "F1.Collector.dll"]
