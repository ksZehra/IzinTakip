# İzin Takip — Frontend

Vue 3, TypeScript ve Vuetify ile yazılmış arayüz. Tarayıcı yalnız `IzinTakip.Gateway` ile konuşur; token tarayıcıya gelmez.

## Komutlar

| Komut                             | Ne yapar                                                                                  |
| --------------------------------- | ----------------------------------------------------------------------------------------- |
| `npm install`                     | Paketleri kurar (Node sürümü `.nvmrc`'de)                                                 |
| `npx playwright install chromium` | Testlerin kullandığı tarayıcıyı kurar (bir kez)                                           |
| `npm run dev`                     | Geliştirme sunucusu: `http://localhost:5173`; `/api` ve `/bff` istekleri gateway'e (5080) |
| `npm run build`                   | Tip kontrolü ve üretim derlemesi (`dist/`)                                                |
| `npm run type-check`              | Yalnız tip kontrolü                                                                       |
| `npm run lint`                    | ESLint (uyarı da hata sayılır)                                                            |
| `npm run format` · `format:check` | Prettier ile biçimlendirir · yalnız kontrol eder                                          |
| `npm test`                        | Birim (Node) ve bileşen (Chromium) testleri                                               |
| `npm run test:e2e`                | Uçtan uca testler (Docker yığını açık olmalı; ana README'ye bakın)                        |
| `npm run api-tipleri`             | Çalışan API'nin OpenAPI belgesinden `src/api/schema.d.ts` tiplerini yeniden üretir        |

## Klasörler

| Klasör          | İçerik                                                                       |
| --------------- | ---------------------------------------------------------------------------- |
| `src/features/` | Her ekranın sayfaları ve route'ları (`izin-talepleri`, `raporlar`, ...)      |
| `src/api/`      | Ekran başına API istemcileri; tipler OpenAPI'den üretilir, elle yazılmaz     |
| `src/common/`   | Ortak bileşenler, composable'lar, HTTP katmanı, yetki sabitleri, yardımcılar |
| `src/layouts/`  | Ana düzen, menü, bildirim zili, kullanıcı menüsü                             |
| `src/router/`   | Route tanımları, giriş ve yetki koruması, menü                               |
| `src/stores/`   | Pinia store'ları: oturum, ayarlar, bildirimler                               |
| `e2e/`          | Playwright senaryoları                                                       |

## Kurallar

- Doğrulama backend'de yapılır; 400 yanıtındaki alan hataları ilgili alanın altına yazılır.
- Alana bağlı olmayan hatalar ve işlem sonuçları tek toast mekanizmasıyla gösterilir.
- Liste filtreleri ve sayfa numarası adres satırında tutulur; bağlantı paylaşılınca aynı liste açılır.
- Sayfalar axe ile WCAG 2 AA için denetlenir; dialog'lar kapanınca odak açan düğmeye döner.
