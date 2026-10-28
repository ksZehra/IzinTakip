FROM mcr.microsoft.com/dotnet/sdk:10.0.403 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY IzinTakip.Domain/IzinTakip.Domain.csproj IzinTakip.Domain/
COPY IzinTakip.Infrastructure/IzinTakip.Infrastructure.csproj IzinTakip.Infrastructure/
COPY IzinTakip.Api/IzinTakip.Api.csproj IzinTakip.Api/
RUN dotnet restore IzinTakip.Api/IzinTakip.Api.csproj

COPY IzinTakip.Domain/ IzinTakip.Domain/
COPY IzinTakip.Infrastructure/ IzinTakip.Infrastructure/
COPY IzinTakip.Api/ IzinTakip.Api/
RUN dotnet publish IzinTakip.Api/IzinTakip.Api.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14 AS runtime
WORKDIR /app
RUN mkdir -p /app/belgeler /app/logs && chown "$APP_UID" /app/belgeler /app/logs
COPY --from=build /app .

ENV DosyaDeposu__Klasor=/app/belgeler \
    LogKlasoru=/app/logs
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "IzinTakip.Api.dll"]
