import { describe, expect, it } from 'vitest'

import { apiTarihi, saatGoster, tarihGoster, tarihNesnesi, tarihSaatGoster } from './tarih'

describe('tarih', () => {
  it('tarihi yerel gün olarak API biçimine çevirir', () => {
    expect(apiTarihi(new Date(2026, 2, 5, 23, 59))).toBe('2026-03-05')
  })

  it('yalnız tarih içeren değeri yerel gün olarak okur', () => {
    const tarih = tarihNesnesi('2026-03-05')

    expect([tarih.getFullYear(), tarih.getMonth(), tarih.getDate(), tarih.getHours()]).toEqual([
      2026, 2, 5, 0,
    ])
  })

  it('tarihi ve saati Türkçe gösterir', () => {
    expect(tarihGoster('2026-03-05')).toBe('5 Mart 2026')
    expect(tarihSaatGoster('2026-03-05T13:30:00')).toBe('5 Mart 2026 13:30')
  })

  it('saati saniyesiz gösterir', () => {
    expect(saatGoster('09:00:00')).toBe('09:00')
  })
})
