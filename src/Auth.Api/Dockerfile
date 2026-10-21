FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Auth.Api/Auth.Api.csproj src/Auth.Api/
RUN dotnet restore src/Auth.Api/Auth.Api.csproj

COPY src/Auth.Api/ src/Auth.Api/

WORKDIR /src/src/Auth.Api
RUN dotnet publish Auth.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
