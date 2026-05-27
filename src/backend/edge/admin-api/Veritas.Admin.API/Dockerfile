FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish src/backend/edge/admin-api/Veritas.Admin.API/Veritas.Admin.API.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false \
    -maxcpucount:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Veritas.Admin.API.dll"]
