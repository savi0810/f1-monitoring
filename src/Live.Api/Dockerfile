FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Live.Api/Live.Api.csproj src/Live.Api/
COPY src/F1Monitor.Contracts/F1Monitor.Contracts.csproj src/F1Monitor.Contracts/
RUN dotnet restore src/Live.Api/Live.Api.csproj

COPY src/Live.Api/ src/Live.Api/
COPY src/F1Monitor.Contracts/ src/F1Monitor.Contracts/

WORKDIR /src/src/Live.Api
RUN dotnet publish Live.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Live.Api.dll"]
