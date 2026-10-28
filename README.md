# İzin Takip

Şirket çalışanlarının izin taleplerini açtığı, yöneticilerin onayladığı ve İK'nın raporladığı web uygulaması.
Backend ASP.NET Core Web API (.NET 10), frontend Vue 3 (hazırlanıyor).

## Özellikler

- **İzin talebi:** tam gün, yarım gün (sabah / öğleden sonra) ve saatlik. Süre iş takvimine göre dakika olarak hesaplanır;
  hafta sonu, resmi tatil ve arife (yarım gün tatil) düşülür.
- **Kurallar:** izin türüne göre alınabilir biçimler, talep başına ve yıllık sınır, çakışma, bakiye ve geçmiş tarih kontrolü.
- **Onay akışı:** talep departman yöneticisine, yöneticinin kendi talebi İK'ya düşer. Onay, red (gerekçeli), iptal.
  Kimse kendi talebini onaylayamaz (SuperAdmin hariç). Belge isteyen türde belge yüklenmeden onay verilmez.
- **Yıllık hak:** işe giriş yıldönümünde kıdem ve yaş kurallarına göre otomatik yazılır; kurallar arayüzden değiştirilir.
- **Bildirim:** uygulama içi bildirim + e-posta (talep, onay, red, iptal, bekleyen talep hatırlatması).
- **Raporlar:** izin takvimi, bakiye raporu, talep listesi; Excel'e aktarım.
- **Yetki ve kapsam:** çalışan kendini, yönetici ekibini, İK tüm şirketi görür. Rol ve yetki yönetimi yalnız SuperAdmin'dedir.
- **Tanımlar:** departman, çalışan, izin türü, resmi tatil, hak kuralı ve iş takvimi ayarları arayüzden yönetilir.

## Teknoloji

| Alan | Seçim |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API (controller), EF Core 10, SQL Server |
| Kimlik | ASP.NET Core Identity, JWT |
| Doğrulama ve hata | FluentValidation, ProblemDetails (RFC 9457) |
| Log | Serilog (konsol + günlük JSON dosya) |
| E-posta, Excel | MailKit, ClosedXML |
| Test | xUnit v3, WebApplicationFactory, Testcontainers (SQL Server) |
| Altyapı | Docker Compose, GitHub Actions |

## Mimari

```
backend/
  IzinTakip.Domain          iş kuralları, entity'ler, hesaplayıcılar (paket bağımlılığı yok)
  IzinTakip.Infrastructure  EF Core, Identity, e-posta, dosya deposu, Excel
  IzinTakip.Api             controller'lar, service'ler, doğrulama, arka plan görevleri
  IzinTakip.Tests           birim ve entegrasyon testleri
frontend/                   Vue 3 arayüzü
docs/                       geliştirme rehberi
```

Her ekranın backend kodu `IzinTakip.Api/Features/<Ekran>/` klasöründe durur: controller, service, istek/yanıt modelleri ve doğrulayıcılar.
İş kuralları Domain'de saf C# olarak yazılır ve veritabanı olmadan test edilir.
Ayrıntılı kurallar, kararlar ve gerekçeleri: [docs/gelistirme-rehberi.md](docs/gelistirme-rehberi.md).

## Kurulum

Gerekenler: .NET 10 SDK, Docker.

1. `.env.example` dosyasını `.env` olarak kopyalayın, `MSSQL_SA_PASSWORD` değerini doldurun.
2. Veritabanı ve e-posta test sunucusu:
   ```bash
   docker compose up -d --wait
   ```
3. Bağlantı dizesi ve gizli ayarlar (`backend/` içinde):
   ```bash
   dotnet user-secrets set "ConnectionStrings:IzinTakip" "Server=127.0.0.1,14333;Database=IzinTakip;User Id=sa;Password=<parola>;Encrypt=True;TrustServerCertificate=True" --project IzinTakip.Api
   dotnet user-secrets set "Jwt:SigningKey" "<en az 32 karakter>" --project IzinTakip.Api
   dotnet user-secrets set "Seed:SuperAdmin:Parola" "<parola>" --project IzinTakip.Api
   dotnet user-secrets set "Seed:Demo:Parola" "<parola>" --project IzinTakip.Api
   ```
4. Veritabanını oluşturun ve çalıştırın:
   ```bash
   dotnet tool restore
   dotnet ef database update --project IzinTakip.Infrastructure --startup-project IzinTakip.Api
   dotnet run --project IzinTakip.Api
   ```

Development ortamında API dokümanı `/scalar` adresindedir; gönderilen e-postalar `http://127.0.0.1:8026` adresinde görünür.
İlk açılışta roller, SuperAdmin (`admin@izintakip.local`), varsayılan izin türleri, hak kuralları ve demo verisi oluşur.

## Docker ile çalıştırma

API container'ı `app` profilindedir. `.env` dosyasında `JWT_SIGNING_KEY` ve `SUPERADMIN_PAROLA` değerlerini doldurun:

```bash
docker compose --profile app up -d --build
```

## Testler

Testler gerçek SQL Server'ı Testcontainers ile açar; Docker çalışıyor olmalıdır.

```bash
cd backend
dotnet test
```
