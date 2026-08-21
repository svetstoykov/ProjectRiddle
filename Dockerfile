# syntax=docker/dockerfile:1
#
# The Pi is arm64: either build on the Pi itself, or
# `docker buildx build --platform linux/arm64 .`. Publishing without a RID is fine —
# the framework-dependent output is portable and the base image supplies the runtime.
# There is no node-gyp step to worry about here: SQLitePCLRaw ships prebuilt
# linux-arm64 binaries in its NuGet package, so native SQLite needs no compilation.

# ---------- Stage 1: frontend ----------
FROM node:22-slim AS frontend
WORKDIR /src/ProjectRiddle.Web

COPY src/ProjectRiddle.Web/package.json src/ProjectRiddle.Web/package-lock.json ./
RUN npm ci

COPY src/ProjectRiddle.Web/ ./
# vite.config.ts writes the production bundle to ../ProjectRiddle.Api/wwwroot.
RUN npm run build

# ---------- Stage 2: backend ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore before copying the rest of the sources, so a source-only change does not
# invalidate the (slow) restore layer.
COPY Directory.Build.props Directory.Build.targets ./
COPY src/ProjectRiddle.Api/ProjectRiddle.Api.csproj src/ProjectRiddle.Api/
COPY src/ProjectRiddle.Core/ProjectRiddle.Core.csproj src/ProjectRiddle.Core/
COPY src/ProjectRiddle.Infrastructure/ProjectRiddle.Infrastructure.csproj src/ProjectRiddle.Infrastructure/
RUN dotnet restore src/ProjectRiddle.Api/ProjectRiddle.Api.csproj

COPY src/ src/
RUN dotnet publish src/ProjectRiddle.Api/ProjectRiddle.Api.csproj -c Release -o /out --no-restore

# The built frontend goes in after publish, which is why the app uses
# UseStaticFiles() rather than MapStaticAssets() — MapStaticAssets builds its
# manifest at compile time from the project's wwwroot, and these files are not
# present until this copy.
COPY --from=frontend /src/ProjectRiddle.Api/wwwroot/. /out/wwwroot/

# ---------- Stage 3: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# The runtime image ships neither curl nor wget, and without one the compose
# healthcheck reports unhealthy forever rather than failing loudly. tzdata is
# required so Time:TimeZoneId (Europe/Sofia) resolves on the host.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl tzdata \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /out ./

# The named volume mounts at /data. An empty named volume inherits the ownership
# of the image's directory at that path, so creating and chowning it here is what
# keeps the non-root process able to write — getting this wrong shows up on the Pi as
# a permission error at first boot, not at build time.
RUN mkdir -p /data && chown -R $APP_UID:$APP_UID /app /data

# TZ is pinned so container-local time agrees with Time:TimeZoneId regardless of
# the Pi's host configuration. 8080 rather than 80: a non-root process cannot bind 80.
ENV TZ=Europe/Sofia \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjectRiddle.Api.dll"]
