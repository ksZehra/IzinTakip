import { beforeEach, describe, expect, it, vi } from 'vitest'
import { page, userEvent } from 'vitest/browser'

import { renderWithPlugins } from '@/testing/renderWithPlugins'

import DataGrid from './DataGrid.vue'
import type { GridKolon } from './dataGridTipleri'

interface Kisi extends Record<string, unknown> {
  id: number
  ad: string
  departman: string
}

const kolonlar: GridKolon<Kisi>[] = [
  { key: 'ad', title: 'Ad Soyad', sirala: true, filtre: { tur: 'metin', anahtar: 'arama' } },
  {
    key: 'departman',
    title: 'Departman',
    filtre: {
      tur: 'cokluSecim',
      anahtar: 'departmanId',
      secenekler: [
        { title: 'Satış', value: 1 },
        { title: 'Yazılım', value: 2 },
      ],
    },
  },
  {
    key: 'giris',
    title: 'İşe Giriş Tarihi',
    filtre: { tur: 'tarih', baslangicAnahtari: 'baslangic', bitisAnahtari: 'bitis' },
  },
  {
    key: 'durum',
    title: 'Durum',
    filtre: {
      tur: 'tekliSecim',
      anahtar: 'aktif',
      secenekler: [
        { title: 'Aktif', value: true },
        { title: 'Pasif', value: false },
      ],
    },
  },
  { key: 'not', title: 'Not' },
]

function cizimSecenekleri(ek: Record<string, unknown> = {}) {
  return {
    props: {
      kolonlar,
      satirlar: [{ id: 1, ad: 'Ayşe Yılmaz', departman: 'Satış' }],
      toplam: 1,
      yukleniyor: false,
      filtre: { arama: '', departmanId: [], baslangic: '', bitis: '', aktif: null },
      sayfa: 1,
      sayfaBoyutu: 20,
      sirala: '',
      ...ek,
    },
  }
}

describe('DataGrid', () => {
  beforeEach(async () => {
    await page.viewport(1280, 800)
  })

  it('filtresi tanımlı her kolonun başlığında filtre simgesi gösterir', async () => {
    const { screen } = await renderWithPlugins(DataGrid, cizimSecenekleri())

    for (const ad of ['Ad Soyad', 'Departman', 'İşe Giriş Tarihi', 'Durum']) {
      await expect.element(screen.getByRole('button', { name: `${ad} filtresi` })).toBeVisible()
    }
    expect(screen.getByRole('button', { name: 'Not filtresi' }).elements()).toHaveLength(0)
  })

  it('metin filtresini Uygula ile filtre anahtarıyla bildirir', async () => {
    const filtreDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ onFiltreDegisti: filtreDegisti }),
    )

    await screen.getByRole('button', { name: 'Ad Soyad filtresi' }).click()
    await userEvent.fill(screen.getByLabelText('Ad Soyad filtre metni'), 'Ayşe')
    expect(filtreDegisti).not.toHaveBeenCalled()
    await screen.getByRole('button', { name: 'Uygula' }).click()

    expect(filtreDegisti).toHaveBeenCalledWith('arama', 'Ayşe')
  })

  it('çoklu seçimde işaretlenen değerleri dizi olarak bildirir', async () => {
    const filtreDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ onFiltreDegisti: filtreDegisti }),
    )

    await screen.getByRole('button', { name: 'Departman filtresi' }).click()
    await screen.getByLabelText('Yazılım', { exact: true }).click()
    await screen.getByRole('button', { name: 'Uygula' }).click()

    expect(filtreDegisti).toHaveBeenCalledWith('departmanId', [2])
  })

  it('tekli seçimde seçeneğe tıklayınca ayrıca Uygula istemeden bildirir', async () => {
    const filtreDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ onFiltreDegisti: filtreDegisti }),
    )

    await screen.getByRole('button', { name: 'Durum filtresi' }).click()
    await screen.getByRole('radio', { name: 'Pasif' }).click()

    expect(filtreDegisti).toHaveBeenCalledWith('aktif', false)
  })

  it('tarihte eşittir koşulunu başlangıç ve bitişi aynı gün yaparak bildirir', async () => {
    const filtreDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ onFiltreDegisti: filtreDegisti }),
    )

    await screen.getByRole('button', { name: 'İşe Giriş Tarihi filtresi' }).click()
    await userEvent.fill(screen.getByLabelText('Tarih', { exact: true }), '2020-08-10')
    await expect.element(screen.getByText('Listelenecek: Eşittir 10.08.2020')).toBeVisible()
    await screen.getByRole('button', { name: 'Uygula' }).click()

    expect(filtreDegisti).toHaveBeenCalledWith('baslangic', '2020-08-10')
    expect(filtreDegisti).toHaveBeenCalledWith('bitis', '2020-08-10')
  })

  it('tarihte arasında koşulunda iki tarih ister ve ters aralığı reddeder', async () => {
    const filtreDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ onFiltreDegisti: filtreDegisti }),
    )

    await screen.getByRole('button', { name: 'İşe Giriş Tarihi filtresi' }).click()
    await screen.getByText('Eşittir (=)', { exact: true }).click()
    await screen.getByRole('option', { name: 'Arasında' }).click()
    await userEvent.fill(screen.getByLabelText('Başlangıç', { exact: true }), '2021-01-01')
    await expect.element(screen.getByText('İki tarihi de seçin.')).toBeVisible()
    await userEvent.fill(screen.getByLabelText('Bitiş', { exact: true }), '2020-01-01')
    await expect.element(screen.getByText('Bitiş tarihi başlangıçtan önce olamaz.')).toBeVisible()
    expect(filtreDegisti).not.toHaveBeenCalled()
  })

  it('etkin filtreleri etiket olarak gösterir ve kaldırınca boş değerle bildirir', async () => {
    const filtreDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({
        onFiltreDegisti: filtreDegisti,
        filtre: { arama: 'Ayşe', departmanId: [1, 2], baslangic: '', bitis: '', aktif: true },
      }),
    )

    await expect.element(screen.getByText('Ad Soyad: Ayşe')).toBeVisible()
    await expect.element(screen.getByText('Departman: Satış, Yazılım')).toBeVisible()
    await expect.element(screen.getByText('Durum: Aktif')).toBeVisible()

    await screen.getByRole('button', { name: 'Ad Soyad: Ayşe filtresini kaldır' }).click()

    expect(filtreDegisti).toHaveBeenCalledWith('arama', '')
  })

  it('metin filtresine yazmak sıralamayı değiştirmez', async () => {
    const siralaDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ 'onUpdate:sirala': siralaDegisti }),
    )

    await screen.getByRole('button', { name: 'Ad Soyad filtresi' }).click()
    await userEvent.fill(screen.getByLabelText('Ad Soyad filtre metni'), 'Ayşe')

    expect(siralaDegisti).not.toHaveBeenCalled()
  })

  it('sıralanabilir kolon başlığına tıklayınca artan sıralamayı bildirir', async () => {
    const siralaDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ 'onUpdate:sirala': siralaDegisti }),
    )

    await screen.getByText('Ad Soyad', { exact: true }).click()

    expect(siralaDegisti).toHaveBeenLastCalledWith('ad:asc')
  })

  it('sıralama kapalı kolona tıklamak sıralamayı değiştirmez', async () => {
    const siralaDegisti = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ 'onUpdate:sirala': siralaDegisti }),
    )

    await screen.getByText('Not', { exact: true }).click()

    expect(siralaDegisti).not.toHaveBeenCalled()
  })

  it('filtre varsayılandan farklıysa sıfırlama düğmesini gösterir', async () => {
    const sifirla = vi.fn()
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({ sifirlanabilir: true, onSifirla: sifirla }),
    )

    await screen.getByRole('button', { name: 'Filtreleri Sıfırla' }).click()

    expect(sifirla).toHaveBeenCalledOnce()
  })

  it('dar ekranda başlık simgelerini göstermez, Filtreler paneli açar', async () => {
    await page.viewport(414, 896)
    const { screen } = await renderWithPlugins(DataGrid, cizimSecenekleri())

    expect(screen.getByRole('button', { name: 'Ad Soyad filtresi' }).elements()).toHaveLength(0)
    await screen.getByRole('button', { name: 'Filtreler' }).click()

    await expect.element(screen.getByRole('button', { name: 'Ad Soyad' })).toBeVisible()
  })
  it('yerel modda satırları tarayıcıda süzer ve sıralar, sayfalama göstermez', async () => {
    const { screen } = await renderWithPlugins(
      DataGrid,
      cizimSecenekleri({
        yerel: true,
        toplam: undefined,
        satirlar: [
          { id: 1, ad: 'Ayşe Yılmaz', departman: 'Satış' },
          { id: 2, ad: 'Burak Arslan', departman: 'Yazılım' },
          { id: 3, ad: 'Can Öztürk', departman: 'Yazılım' },
        ],
        filtre: { arama: 'r', departmanId: [], baslangic: '', bitis: '', aktif: null },
        sirala: 'ad:desc',
      }),
    )

    await expect.element(screen.getByText('Burak Arslan')).toBeVisible()
    const satirlar = screen
      .getByRole('row')
      .elements()
      .map((satir) => satir.textContent)
    expect(satirlar).toEqual([expect.any(String), 'Can ÖztürkYazılım', 'Burak ArslanYazılım'])
    expect(screen.getByText('Sayfa başına satır', { exact: false }).elements()).toHaveLength(0)
  })
})
