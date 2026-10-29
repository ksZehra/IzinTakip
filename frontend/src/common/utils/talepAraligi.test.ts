import { describe, expect, it } from 'vitest'

import { talepAraligiMetni } from './talepAraligi'

describe('talepAraligiMetni', () => {
  it('tam gün talebi tarih aralığıyla gösterir', () => {
    expect(
      talepAraligiMetni({
        sureTipi: 'TamGun',
        baslangic: '2027-03-01T09:00:00',
        bitis: '2027-03-03T18:00:00',
      }),
    ).toBe('1 Mart 2027 – 3 Mart 2027')
    expect(
      talepAraligiMetni({
        sureTipi: 'TamGun',
        baslangic: '2027-03-01T09:00:00',
        bitis: '2027-03-01T18:00:00',
      }),
    ).toBe('1 Mart 2027')
  })

  it('yarım gün ve saatlik talebi saatleriyle gösterir', () => {
    expect(
      talepAraligiMetni({
        sureTipi: 'Saatlik',
        baslangic: '2027-03-01T10:00:00',
        bitis: '2027-03-01T12:30:00',
      }),
    ).toBe('1 Mart 2027, 10:00–12:30')
  })
})
