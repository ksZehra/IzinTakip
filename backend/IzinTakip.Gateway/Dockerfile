FROM node:26.13.0-alpine3.24 AS frontend
WORKDIR /frontend

COPY frontend/package.json frontend/package-lock.json frontend/.npmrc ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0.403 AS build
WORKDIR /src

COPY backend/global.json backend/Directory.Build.props backend/Directory.Packages.props ./
COPY backend/IzinTakip.Gateway/IzinTakip.Gateway.csproj IzinTakip.Gateway/
RUN dotnet restore IzinTakip.Gateway/IzinTakip.Gateway.csproj

COPY backend/IzinTakip.Gateway/ IzinTakip.Gateway/
COPY --from=frontend /frontend/dist/ IzinTakip.Gateway/wwwroot/
RUN dotnet publish IzinTakip.Gateway/IzinTakip.Gateway.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14 AS runtime
WORKDIR /app
RUN mkdir -p /app/keys /app/logs && chown "$APP_UID" /app/keys /app/logs
COPY --from=build /app .

ENV DataProtection__Klasor=/app/keys \
    LogKlasoru=/app/logs
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "IzinTakip.Gateway.dll"]
