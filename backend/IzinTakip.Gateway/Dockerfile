FROM mcr.microsoft.com/dotnet/sdk:10.0.403 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY IzinTakip.Gateway/IzinTakip.Gateway.csproj IzinTakip.Gateway/
RUN dotnet restore IzinTakip.Gateway/IzinTakip.Gateway.csproj

COPY IzinTakip.Gateway/ IzinTakip.Gateway/
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
