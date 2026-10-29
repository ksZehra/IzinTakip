import { describe, expect, it } from 'vitest'

import { dosyaAdiniOku } from './dosyaIndir'

describe('dosyaAdiniOku', () => {
  it('UTF-8 kodlu dosya adını tercih eder', () => {
    expect(
      dosyaAdiniOku(
        "attachment; filename=rapor.pdf; filename*=UTF-8''sa%C4%9Fl%C4%B1k-raporu.pdf",
        'belge',
      ),
    ).toBe('sağlık-raporu.pdf')
  })

  it('düz dosya adını okur, başlık yoksa varsayılanı verir', () => {
    expect(dosyaAdiniOku('attachment; filename="izin-talepleri-20270301.xlsx"', 'x')).toBe(
      'izin-talepleri-20270301.xlsx',
    )
    expect(dosyaAdiniOku(undefined, 'izin-talepleri.xlsx')).toBe('izin-talepleri.xlsx')
  })
})
