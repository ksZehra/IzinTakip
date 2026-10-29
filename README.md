# İzin Takip

[![CI](https://github.com/ksZehra/IzinTakip/actions/workflows/ci.yml/badge.svg)](https://github.com/ksZehra/IzinTakip/actions/workflows/ci.yml)

Şirketlerin izin sürecini baştan sona yöneten web uygulaması. Çalışan izin talebini açar, yöneticisi onaylar,
İK bakiyeleri ve raporları izler. Süreler iş takvimine göre dakika hassasiyetinde hesaplanır; her değişiklik kayıt altındadır.

- **Backend:** ASP.NET Core Web API (.NET 10), EF Core, SQL Server
- **Frontend:** Vue 3, TypeScript, Vuetify
- **Güvenlik katmanı:** YARP ile BFF gateway; token tarayıcıya hiç gelmez
- **Kalite:** 238 backend testi, 57 frontend testi, 16 uçtan uca senaryo (masaüstü ve mobil), her push'ta CI

## Özellikler

**İzin talebi ve süre hesabı**
- Tam gün, yarım gün (sabah / öğleden sonra) ve saatlik izin.
- Süre, şirketin iş takvimine göre dakika olarak hesaplanır: hafta sonu, resmi tatil ve arife (yarım gün) düşülür, öğle arası sayılmaz.
- Talep açılmadan önce hesaplanan süre ekranda önizlenir.

**İş kuralları**
- İzin türüne göre alınabilecek biçimler, talep başına ve yıllık üst sınır.
- Çakışan talep, yetersiz bakiye ve geçmiş tarih kontrolü.
- Belge isteyen izin türünde belge yüklenmeden onay verilmez.

**Onay akışı**
- Talep departman yöneticisine düşer; yöneticinin kendi talebi İK'ya gider.
- Onay, gerekçeli red ve iptal. Kimse kendi talebini onaylayamaz (SuperAdmin hariç).
- Bekleyen talepler için onaycılara her sabah hatırlatma gider.

**Yıllık izin hakkı**
- İşe giriş yıl dönümünde kıdem ve yaşa göre otomatik yazılır.
- Hak kuralları arayüzden değiştirilir; İK gerektiğinde elle düzeltme satırı ekler.

**Bildirim**
- Uygulama içi bildirim ve e-posta: yeni talep, onay, red, iptal, hatırlatma.
- E-postalar kuyruktan gönderilir; geçici hatada yeniden denenir, kalıcı hatada sistemi durdurmaz.

**Raporlar**
- Ekip ve şirket izin takvimi, bakiye raporu, filtrelenebilir talep listesi.
- Talep ve bakiye listeleri Excel'e aktarılır.

**Yetki ve veri kapsamı**
- Çalışan yalnız kendini, yönetici ekibini, İK bütün şirketi görür. Kapsam dışındaki kayıt hiç yokmuş gibi davranır (404).
- Roller ve yetkiler arayüzden yönetilir; sistem rolleri ve son SuperAdmin korunur.

**İşlem kaydı (audit log)**
- Çalışan, hak, tanım, kullanıcı ve rol değişiklikleri kendiliğinden kaydedilir: kim, ne zaman, hangi alanı, eski ve yeni değeriyle.
- Kayıtlar ekranda okunur biçimde gösterilir: alan adları Türkçe, kişi ve departmanlar adıyla, süreler "2 gün 4 saat" gibi.
- Parola değerleri asla kayda yazılmaz.

**Arayüz**
- Türkçe; telefon, tablet ve masaüstüne uyumlu.
- Erişilebilir: klavyeyle ve ekran okuyucuyla kullanılır, sayfalar WCAG 2 AA için otomatik denetlenir.
- Sayfalar ihtiyaç anında yüklenir; filtreler adres satırında tutulur, paylaşılan bağlantı aynı listeyi açar.

## Roller

| Rol | Ne yapar |
|---|---|
| Çalışan | Kendi adına talep açar, iptal eder, bakiyesini görür |
| Yönetici | Ekibinin taleplerini onaylar veya reddeder, ekibinin raporlarını görür |
| İK | Bütün şirketin talepleri, raporları, çalışanları, tanımları ve kullanıcı hesapları |
| SuperAdmin | Her şey; rol ve yetki yönetimi, işlem kayıtları |

Yeni roller, sabit yetki listesinden seçilerek arayüzden oluşturulur.

## Mimari

```
Tarayıcı ──► Gateway (BFF, YARP) ──► API ──► SQL Server
             şifreli cookie           JWT
```

```
backend/
  IzinTakip.Gateway         tarayıcının tek kapısı: giriş/çıkış, cookie → JWT, /api yönlendirmesi, güvenlik başlıkları
  IzinTakip.Domain          iş kuralları, entity'ler, süre ve hak hesapları (dış paket bağımlılığı yok)
  IzinTakip.Infrastructure  EF Core, Identity, migration'lar, e-posta, dosya deposu, Excel, işlem kaydı
  IzinTakip.Api             controller'lar, service'ler, doğrulama, arka plan işleri
  IzinTakip.Tests           birim, entegrasyon ve gateway testleri
frontend/
  src/features/<ekran>/     her ekranın sayfaları ve route'ları
  src/api/                  OpenAPI'den üretilen tiplerle yazılmış API istemcisi
  src/common/               ortak bileşenler, composable'lar, HTTP katmanı
  e2e/                      Playwright senaryoları
```

**Öne çıkan kararlar**
- **Feature klasörleri:** her ekranın backend kodu `IzinTakip.Api/Features/<Ekran>/` altında bir arada durur: controller, service, istek/yanıt modelleri, doğrulayıcılar.
- **Saf Domain:** iş kuralları veritabanından bağımsız C# sınıflarıdır; milisaniyeler içinde test edilir.
- **BFF gateway:** JWT tarayıcıya gelmez; gateway onu şifreli, `HttpOnly`, `SameSite=Strict` cookie'de tutar. CSRF başlığı, giriş hız sınırı, CSP ve güvenlik başlıkları gateway'dedir.
- **Tek hata biçimi:** bütün hatalar ProblemDetails (RFC 9457) olarak döner; doğrulama hataları alan bazında forma yazılır.
- **Tipli sözleşme:** frontend'deki API tipleri elle yazılmaz, API'nin OpenAPI belgesinden üretilir.
- **Gerçek veritabanıyla test:** entegrasyon testleri Testcontainers ile gerçek SQL Server'a karşı çalışır.

## Teknoloji

| Alan | Kullanılanlar |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API (controller), EF Core 10, SQL Server 2022 |
| Kimlik | ASP.NET Core Identity, JWT, Data Protection ile şifreli cookie |
| Gateway | YARP, Rate Limiting |
| Doğrulama ve hata | FluentValidation, ProblemDetails |
| Log ve izleme | Serilog (konsol + günlük JSON dosyası), health check'ler |
| E-posta ve Excel | MailKit, ClosedXML |
| API dokümanı | OpenAPI, Scalar |
| Frontend | Vue 3, TypeScript, Vite, Vuetify, Pinia, Vue Router, axios |
| Test | xUnit v3, WebApplicationFactory, Testcontainers; Vitest (Node + Chromium), Playwright, axe |
| Altyapı | Docker, Docker Compose, GitHub Actions |

## Hızlı başlangıç (Docker)

Yalnız Docker gerekir (Windows ve macOS'ta Docker Desktop). .NET ve Node kurmadan bütün uygulama çalışır.

1. Ayar dosyasını oluşturun:
   ```bash
   cp .env.example .env
   ```
2. `.env` içinde şu üç değeri doldurun:
   - `MSSQL_SA_PASSWORD`: SQL Server parolası. En az 8 karakter; büyük harf, küçük harf, rakam ve sembolden en az üçünü içermeli, yoksa SQL Server açılmaz.
   - `JWT_SIGNING_KEY`: en az 32 karakterlik rastgele bir metin. Dosyadaki değer yalnız örnektir, değiştirin.
   - `SUPERADMIN_PAROLA`: ilk yönetici hesabının parolası.

   Örnek çalışanlar ve taleplerle denemek için `DEMO_VERI=true` yapıp `DEMO_PAROLA` verin.
3. Çalıştırın (ilk derleme birkaç dakika sürer):
   ```bash
   docker compose --profile app up -d --build --wait
   ```
4. `http://localhost:8088` adresini açın; `admin@izintakip.local` ve `SUPERADMIN_PAROLA` ile giriş yapın.
   Demo verisi açıksa `ik@`, `yonetici@` ve `calisan@izintakip.local` hesapları `DEMO_PAROLA` ile girer.
   Gönderilen e-postalar `http://127.0.0.1:8026` adresinde görünür.

İlk açılışta veritabanı şeması kurulur; roller, SuperAdmin hesabı, varsayılan izin türleri, hak kuralları ve iş takvimi eklenir.
API dışarıya açık değildir, yalnız gateway üzerinden erişilir.
Konteynerler `izintakip-sqlserver`, `izintakip-mailpit`, `izintakip-api` ve `izintakip-gateway` adlarıyla çalışır (örneğin `docker logs izintakip-api`).

Portlar doluysa `.env`'deki `GATEWAY_PORT`, `SQLSERVER_PORT` ve `MAILPIT_*` değerlerini değiştirin.
Durdurmak için `docker compose --profile app down`; verileri de silmek için sonuna `-v` ekleyin.

`SUPERADMIN_PAROLA` yalnız ilk kurulumda kullanılır. Sonradan `.env`'de değiştirmek mevcut hesabın parolasını değiştirmez.

## Geliştirme ortamı

Gerekenler: .NET 10 SDK, Node.js 26 (sürüm `frontend/.nvmrc`'de), Docker.

1. `.env.example` dosyasını `.env` olarak kopyalayın, `MSSQL_SA_PASSWORD` değerini doldurun.
2. Veritabanını ve e-posta test sunucusunu başlatın:
   ```bash
   docker compose up -d --wait
   ```
3. Bağlantı dizesini ve gizli ayarları girin (`backend/` içinde):
   ```bash
   dotnet user-secrets set "ConnectionStrings:IzinTakip" "Server=127.0.0.1,14333;Database=IzinTakip;User Id=sa;Password=<parola>;Encrypt=True;TrustServerCertificate=True" --project IzinTakip.Api
   dotnet user-secrets set "Jwt:SigningKey" "<en az 32 karakter>" --project IzinTakip.Api
   dotnet user-secrets set "Seed:SuperAdmin:Parola" "<parola>" --project IzinTakip.Api
   dotnet user-secrets set "Seed:Demo:Parola" "<parola>" --project IzinTakip.Api
   ```
4. Veritabanını oluşturun ve API'yi çalıştırın:
   ```bash
   dotnet tool restore
   dotnet ef database update --project IzinTakip.Infrastructure --startup-project IzinTakip.Api
   dotnet run --project IzinTakip.Api
   ```
5. Gateway'i ayrı bir terminalde çalıştırın (`http://localhost:5080`):
   ```bash
   dotnet run --project IzinTakip.Gateway
   ```
6. Arayüzü çalıştırın (`http://localhost:5173`):
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

| Adres | Ne var |
|---|---|
| `http://localhost:5173` | Arayüz (Vite) |
| `http://localhost:5137/scalar` | API dokümanı (Scalar) |
| `http://localhost:5137/health` | API sağlık kontrolü (veritabanı dahil) |
| `http://127.0.0.1:8026` | Gönderilen e-postalar (Mailpit) |

Docker yığını ile geliştirme ortamı aynı veritabanını kullanır; ikisini aynı anda çalıştırmayın.

## Testler

| Katman | Ne test edilir | Araç |
|---|---|---|
| Domain | süre hesabı, hak kuralları, talep durumları | xUnit |
| API | her endpoint, yetki ve kapsam, doğrulama, hata yanıtları, arka plan işleri | xUnit, WebApplicationFactory, Testcontainers (gerçek SQL Server) |
| Gateway | cookie, CSRF, hız sınırı, API'ye ulaşılamama durumu | xUnit |
| Frontend | composable'lar, yardımcılar, router koruması, bileşenler | Vitest (Node ve gerçek Chromium) |
| Uçtan uca | giriş, talep → onay → iptal, raporlar, tanımlar, yönetim; masaüstü ve mobil | Playwright, axe (erişilebilirlik) |

Backend testleri için Docker çalışıyor olmalıdır:

```bash
cd backend
dotnet test
```

Frontend testleri (`frontend/` içinde; bileşen testleri için bir kez `npx playwright install chromium`):

```bash
npm run lint
npm run type-check
npm test
```

Uçtan uca testler Docker yığınına (`http://localhost:8088`) bağlanır. `.env`'de `DEMO_VERI=true`, `DEMO_PAROLA` ve yüksek bir
`GIRIS_DAKIKADA_DENEME` verip yığını kurduktan sonra:

```bash
E2E_PAROLA=<demo parolası> E2E_ADMIN_PAROLA=<SuperAdmin parolası> npm run test:e2e
```

## Sürekli entegrasyon

Her push'ta GitHub Actions üç iş çalıştırır:

- **backend:** derleme, bütün testler, API ve gateway Docker imajlarının derlenmesi
- **frontend:** `npm audit`, lint, format kontrolü, tip kontrolü, testler, üretim derlemesi
- **e2e:** bütün yığını Docker Compose ile kurar ve Playwright senaryolarını çalıştırır
