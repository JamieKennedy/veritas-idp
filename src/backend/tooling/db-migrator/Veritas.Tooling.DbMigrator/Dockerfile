FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish src/backend/tooling/db-migrator/Veritas.Tooling.DbMigrator/Veritas.Tooling.DbMigrator.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false \
    -maxcpucount:1

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Veritas.Tooling.DbMigrator.dll"]
