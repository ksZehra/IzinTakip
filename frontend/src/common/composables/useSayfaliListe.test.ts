import { describe, expect, it } from 'vitest'

import { filtreyiOku, filtreyiYaz } from './useSayfaliListe'

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

  it('boş ya da eksik değerde varsayılanı korur', () => {
    expect(filtreyiOku(varsayilan, { durum: '' })).toEqual(varsayilan)
  })
})

describe('filtreyiYaz', () => {
  it('yalnız varsayılandan farklı dolu değerleri adrese yazar', () => {
    expect(filtreyiYaz({ ...varsayilan, izinTuruId: 3, arama: '' }, varsayilan)).toEqual({
      izinTuruId: '3',
    })
  })
})
