FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PruebaTecnicaCip.sln ./
COPY src/PruebaTecnicaCip.Web/PruebaTecnicaCip.Web.csproj src/PruebaTecnicaCip.Web/
RUN dotnet restore src/PruebaTecnicaCip.Web/PruebaTecnicaCip.Web.csproj

COPY src/PruebaTecnicaCip.Web/ src/PruebaTecnicaCip.Web/
RUN dotnet publish src/PruebaTecnicaCip.Web/PruebaTecnicaCip.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PruebaTecnicaCip.Web.dll"]
