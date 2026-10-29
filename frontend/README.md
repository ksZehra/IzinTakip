# IzinTakip — Frontend

Vue 3 + TypeScript + Vuetify arayüzü. Tarayıcı yalnız `IzinTakip.Gateway` ile konuşur; ayrıntılar `docs/gelistirme-rehberi.md` §17'de.

| Komut                             | İş                                                                                    |
| --------------------------------- | ------------------------------------------------------------------------------------- |
| `npm install`                     | Paketleri kurar (Node sürümü `.nvmrc`'de)                                             |
| `npx playwright install chromium` | Testlerin kullandığı tarayıcıyı kurar (bir kez)                                       |
| `npm run dev`                     | Geliştirme sunucusu: `http://localhost:5173`; `/api` ve `/bff` gateway'e (5080) gider |
| `npm run build`                   | Tip kontrolü + üretim build'i (`dist/`)                                               |
| `npm run lint` · `npm run format` | ESLint · Prettier                                                                     |
| `npm test`                        | Birim (Node) ve bileşen (Chromium) testleri                                           |
