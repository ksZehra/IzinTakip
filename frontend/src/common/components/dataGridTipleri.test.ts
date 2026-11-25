import { describe, expect, it } from 'vitest'

import {
  doluFiltreSayisi,
  etkinFiltreler,
  filtreAnahtarlari,
  tarihAraligi,
  tarihDurumu,
  tarihHatasi,
  tarihOzeti,
  type GridKolon,
  yerelFiltrele,
  yerelSirala,
} from './dataGridTipleri'

const kolonlar: GridKolon<object>[] = [
  { key: 'ad', title: 'Ad', filtre: { tur: 'metin', anahtar: 'arama' } },
  {
    key: 'departman',
    title: 'Departman',
    filtre: {
      tur: 'cokluSecim',
      anahtar: 'departmanId',
      secenekler: [
        { title: 'Satış', value: 1 },
        { title: 'Yazılım', value: 2 },
        { title: 'İK', value: 3 },
      ],
    },
  },
  {
    key: 'giris',
    title: 'Giriş',
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

const bos = { arama: '', departmanId: [], baslangic: '', bitis: '', aktif: null }

describe('doluFiltreSayisi', () => {
  it('boş filtrelerde sıfır döner', () => {
    expect(doluFiltreSayisi(kolonlar, bos)).toBe(0)
  })

  it('aralığın bir ucu dolu olsa bile o kolonu bir kez sayar', () => {
    expect(
      doluFiltreSayisi(kolonlar, {
        arama: 'a',
        departmanId: [1, 2],
        baslangic: '2020-01-01',
        bitis: '2021-01-01',
        aktif: false,
      }),
    ).toBe(4)
  })

  it('filtresiz kolonu saymaz', () => {
    expect(doluFiltreSayisi(kolonlar, { not: 'x' })).toBe(0)
  })
})

describe('filtreAnahtarlari', () => {
  it('tarih filtresinde başlangıç ve bitiş anahtarlarını verir', () => {
    expect(filtreAnahtarlari({ tur: 'tarih', baslangicAnahtari: 'b', bitisAnahtari: 'c' })).toEqual(
      ['b', 'c'],
    )
  })
})

describe('tarihAraligi', () => {
  it('her koşulu başlangıç ve bitişe çevirir', () => {
    expect(tarihAraligi('eq', '2020-08-10', '')).toEqual({
      baslangic: '2020-08-10',
      bitis: '2020-08-10',
    })
    expect(tarihAraligi('ge', '2020-08-10', '')).toEqual({ baslangic: '2020-08-10', bitis: '' })
    expect(tarihAraligi('le', '2020-08-10', '')).toEqual({ baslangic: '', bitis: '2020-08-10' })
    expect(tarihAraligi('gt', '2020-08-10', '')).toEqual({ baslangic: '2020-08-11', bitis: '' })
    expect(tarihAraligi('lt', '2020-08-10', '')).toEqual({ baslangic: '', bitis: '2020-08-09' })
    expect(tarihAraligi('bw', '2020-01-01', '2020-12-31')).toEqual({
      baslangic: '2020-01-01',
      bitis: '2020-12-31',
    })
  })

  it('ay ve yıl sınırlarında gün ekler ve çıkarır', () => {
    expect(tarihAraligi('gt', '2020-12-31', '').baslangic).toBe('2021-01-01')
    expect(tarihAraligi('lt', '2020-03-01', '').bitis).toBe('2020-02-29')
  })

  it('tarih boşsa filtre uygulamaz', () => {
    expect(tarihAraligi('gt', '', '')).toEqual({ baslangic: '', bitis: '' })
  })
})

describe('tarihHatasi', () => {
  it('arasında koşulunda iki tarih ister ve ters aralığı reddeder', () => {
    expect(tarihHatasi('bw', '2020-01-01', '')).toBe('İki tarihi de seçin.')
    expect(tarihHatasi('bw', '2021-01-01', '2020-01-01')).toBe(
      'Bitiş tarihi başlangıçtan önce olamaz.',
    )
    expect(tarihHatasi('bw', '2020-01-01', '2021-01-01')).toBe('')
    expect(tarihHatasi('eq', '', '')).toBe('')
  })
})

describe('tarihDurumu', () => {
  it('başlangıç ve bitişten koşulu çıkarır', () => {
    expect(tarihDurumu(undefined, '2020-08-10', '2020-08-10').operator).toBe('eq')
    expect(tarihDurumu(undefined, '2020-01-01', '2020-12-31').operator).toBe('bw')
    expect(tarihDurumu(undefined, '2020-01-01', '').operator).toBe('ge')
    expect(tarihDurumu(undefined, '', '2020-01-01').operator).toBe('le')
    expect(tarihDurumu(undefined, '', '')).toEqual({ operator: 'eq', a: '', b: '' })
  })

  it('kayıtlı büyüktür ve küçüktür koşulunu özgün tarihle geri kurar', () => {
    expect(tarihDurumu('gt', '2020-08-11', '')).toEqual({ operator: 'gt', a: '2020-08-10', b: '' })
    expect(tarihDurumu('lt', '', '2020-08-09')).toEqual({ operator: 'lt', a: '2020-08-10', b: '' })
  })

  it('kayıtlı koşul değerlerle uyuşmuyorsa değerlerden çıkarır', () => {
    expect(tarihDurumu('gt', '2020-01-01', '2020-12-31').operator).toBe('bw')
  })
})

describe('etkinFiltreler', () => {
  it('boş filtrelerde etiket üretmez', () => {
    expect(etkinFiltreler(kolonlar, bos, {})).toEqual([])
  })

  it('her dolu kolon için okunur bir etiket üretir', () => {
    const etiketler = etkinFiltreler(
      kolonlar,
      {
        arama: 'Ayşe',
        departmanId: [1, 2, 3],
        baslangic: '2020-08-11',
        bitis: '',
        aktif: false,
      },
      { giris: 'gt' },
    ).map((e) => e.metin)

    expect(etiketler).toEqual([
      'Ad: Ayşe',
      'Departman: Satış, Yazılım +1',
      'Giriş: Büyüktür 10.08.2020',
      'Durum: Pasif',
    ])
  })

  it('arasında koşulunu iki tarihle yazar', () => {
    expect(tarihOzeti({ operator: 'bw', a: '2020-01-01', b: '2020-12-31' })).toBe(
      '01.01.2020 ile 31.12.2020 arası',
    )
  })
})

interface Kayit {
  ad: string
  roller: string[]
  aktif: boolean
  tarih: string
  gun: number | null
}

const kayitlar: Kayit[] = [
  { ad: 'İsmail', roller: ['IK'], aktif: true, tarih: '2026-03-10', gun: 14 },
  { ad: 'ayşe', roller: ['Calisan', 'Yonetici'], aktif: false, tarih: '2026-05-01', gun: null },
  { ad: 'Zehra', roller: ['Calisan'], aktif: true, tarih: '2025-12-31', gun: 2 },
]

const yerelKolonlar: GridKolon<Kayit>[] = [
  { key: 'ad', title: 'Ad', sirala: true, filtre: { tur: 'metin', anahtar: 'ad' } },
  {
    key: 'roller',
    title: 'Roller',
    filtre: {
      tur: 'cokluSecim',
      anahtar: 'rol',
      secenekler: [
        { title: 'İK', value: 'IK' },
        { title: 'Yönetici', value: 'Yonetici' },
      ],
    },
  },
  {
    key: 'aktif',
    title: 'Durum',
    deger: (k) => (k.aktif ? 'Aktif' : 'Pasif'),
    sirala: true,
    filtre: {
      tur: 'tekliSecim',
      anahtar: 'aktif',
      secenekler: [
        { title: 'Aktif', value: true },
        { title: 'Pasif', value: false },
      ],
    },
  },
  {
    key: 'tarih',
    title: 'Tarih',
    deger: (k) => k.tarih.split('-').reverse().join('.'),
    sirala: true,
    filtre: { tur: 'tarih', baslangicAnahtari: 'baslangic', bitisAnahtari: 'bitis' },
  },
  { key: 'gunMetni', title: 'Gün', ham: (k) => k.gun, sirala: true },
]

const bosFiltre = { ad: '', rol: [], aktif: null, baslangic: '', bitis: '' }
const adlar = (liste: Kayit[]) => liste.map((k) => k.ad)

describe('yerelFiltrele', () => {
  it('filtre yoksa satırları olduğu gibi döner', () => {
    expect(yerelFiltrele(yerelKolonlar, kayitlar, bosFiltre)).toBe(kayitlar)
  })

  it('metni Türkçe küçük harfe göre görünen değerde arar', () => {
    expect(adlar(yerelFiltrele(yerelKolonlar, kayitlar, { ...bosFiltre, ad: 'İS' }))).toEqual([
      'İsmail',
    ])
    expect(adlar(yerelFiltrele(yerelKolonlar, kayitlar, { ...bosFiltre, ad: 'AYŞE' }))).toEqual([
      'ayşe',
    ])
  })

  it('çoklu seçimde hücredeki değerlerden biri seçiliyse eşleşir', () => {
    expect(
      adlar(yerelFiltrele(yerelKolonlar, kayitlar, { ...bosFiltre, rol: ['IK', 'Yonetici'] })),
    ).toEqual(['İsmail', 'ayşe'])
  })

  it('tekli seçimi ham değerle, tarihi uçlar dahil karşılaştırır ve filtreleri birleştirir', () => {
    expect(adlar(yerelFiltrele(yerelKolonlar, kayitlar, { ...bosFiltre, aktif: false }))).toEqual([
      'ayşe',
    ])
    expect(
      adlar(
        yerelFiltrele(yerelKolonlar, kayitlar, {
          ...bosFiltre,
          aktif: true,
          baslangic: '2026-01-01',
          bitis: '2026-03-10',
        }),
      ),
    ).toEqual(['İsmail'])
  })
})

describe('yerelSirala', () => {
  it('metni Türkçe alfabeye, sayıyı değere göre sıralar; boş değer artanda sona kalır', () => {
    expect(adlar(yerelSirala(yerelKolonlar, kayitlar, 'ad:asc'))).toEqual([
      'ayşe',
      'İsmail',
      'Zehra',
    ])
    expect(adlar(yerelSirala(yerelKolonlar, kayitlar, 'gunMetni:asc'))).toEqual([
      'Zehra',
      'İsmail',
      'ayşe',
    ])
    expect(adlar(yerelSirala(yerelKolonlar, kayitlar, 'tarih:desc'))).toEqual([
      'ayşe',
      'İsmail',
      'Zehra',
    ])
  })

  it('sıralanmayan kolonda ya da geçersiz yönde sırayı değiştirmez', () => {
    expect(yerelSirala(yerelKolonlar, kayitlar, 'roller:asc')).toBe(kayitlar)
    expect(yerelSirala(yerelKolonlar, kayitlar, 'ad:yukari')).toBe(kayitlar)
    expect(yerelSirala(yerelKolonlar, kayitlar, '')).toBe(kayitlar)
  })
})
