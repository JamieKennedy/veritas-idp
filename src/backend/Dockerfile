# syntax=docker/dockerfile:1.7@sha256:a57df69d0ea827fb7266491f2813635de6f17269be881f696fbfdf2d83dda33e

FROM mcr.microsoft.com/dotnet/sdk:10.0.301@sha256:ea8bde36c11b6e7eec2656d0e59101d4462f6bd630730f2c8201ed0572b295d5 AS build
ARG VERSION=0.0.0-development
WORKDIR /src

COPY . .
RUN dotnet restore Veritas.slnx
RUN dotnet publish src/backend/edge/admin-api/Veritas.Admin.API/Veritas.Admin.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/admin-api \
    /p:Version=${VERSION} \
    /p:UseAppHost=false \
    -maxcpucount:1
RUN dotnet publish src/backend/tooling/db-migrator/Veritas.Tooling.DbMigrator/Veritas.Tooling.DbMigrator.csproj \
    --configuration Release \
    --no-restore \
    --output /app/migrator \
    /p:Version=${VERSION} \
    /p:UseAppHost=false \
    -maxcpucount:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10@sha256:1fa23fc4872d95fd71c2833ebe65d7e84a43b2d51a31d119516852f13d9505a7 AS final
ARG VERSION=0.0.0-development
ARG REVISION=unknown
ARG SOURCE=https://github.com/JamieKennedy/veritas

LABEL org.opencontainers.image.title="Veritas Backend" \
      org.opencontainers.image.description="Veritas Admin API and database migrator" \
      org.opencontainers.image.source="${SOURCE}" \
      org.opencontainers.image.revision="${REVISION}" \
      org.opencontainers.image.version="${VERSION}"

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

# hadolint ignore=DL3008
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /var/lib/veritas/data-protection /var/log/veritas \
    && chown -R "${APP_UID}:${APP_UID}" /var/lib/veritas /var/log/veritas

COPY --from=build --chown=${APP_UID}:${APP_UID} /app/admin-api /app/admin-api
COPY --from=build --chown=${APP_UID}:${APP_UID} /app/migrator /app/migrator

USER ${APP_UID}
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=6 \
    CMD curl --fail --silent --show-error http://127.0.0.1:8080/alive || exit 1

ENTRYPOINT ["dotnet"]
CMD ["/app/admin-api/Veritas.Admin.API.dll"]
