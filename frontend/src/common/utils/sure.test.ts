import { describe, expect, it } from 'vitest'

import { dakikayaCevir, dakikayiParcala } from './sure'

const gunluk = 480

describe('sure', () => {
  it('gün, saat ve dakikayı toplam dakikaya çevirir', () => {
    expect(dakikayaCevir({ gun: 2, saat: 3, dakika: 30, eksi: false }, gunluk)).toBe(1170)
  })

  it('eksi süreyi negatif dakika olarak verir', () => {
    expect(dakikayaCevir({ gun: 1, saat: 0, dakika: 0, eksi: true }, gunluk)).toBe(-480)
  })

  it('dakikayı gün, saat ve dakikaya ayırır', () => {
    expect(dakikayiParcala(1170, gunluk)).toEqual({ gun: 2, saat: 3, dakika: 30, eksi: false })
    expect(dakikayiParcala(-240, gunluk)).toEqual({ gun: 0, saat: 4, dakika: 0, eksi: true })
  })
})
