import { describe, expect, it } from 'vitest'

import { filtreyiOku, filtreyiYaz } from './usePagedList'

const varsayilan = { durum: null, izinTuruId: null, sadeceOkunmamis: false, arama: '' }

describe('filtreyiOku', () => {
  it('adres sorgusunu varsayılan değerin türüne göre okur', () => {
    expect(
      filtreyiOku(varsayilan, {
        durum: 'Beklemede',
        izinTuruId: '3',
        sadeceOkunmamis: 'true',
        arama: 'ayşe',
        bilinmeyen: 'x',
      }),
    ).toEqual({ durum: 'Beklemede', izinTuruId: 3, sadeceOkunmamis: true, arama: 'ayşe' })
  })

  it('eksik değerde varsayılanı korur', () => {
    expect(filtreyiOku(varsayilan, {})).toEqual(varsayilan)
  })

  it('boş değeri temizlenmiş filtre olarak okur', () => {
    expect(filtreyiOku({ aktif: true, arama: 'x' }, { aktif: '', arama: '' })).toEqual({
      aktif: null,
      arama: '',
    })
  })

  it('varsayılanı boş alanda true/false değerini mantıksal okur', () => {
    expect(filtreyiOku({ aktif: null }, { aktif: 'false' })).toEqual({ aktif: false })
  })
})

describe('filtreyiYaz', () => {
  it('yalnız varsayılandan farklı dolu değerleri adrese yazar', () => {
    expect(filtreyiYaz({ ...varsayilan, izinTuruId: 3, arama: '' }, varsayilan)).toEqual({
      izinTuruId: '3',
    })
  })

  it('varsayılanı dolu filtre temizlenince boş değer yazar', () => {
    expect(filtreyiYaz({ aktif: null }, { aktif: true })).toEqual({ aktif: '' })
  })
})
