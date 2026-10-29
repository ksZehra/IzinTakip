import { describe, expect, it } from 'vitest'

import { alanEtiketi, degerGoster } from './degisiklikGoster'

describe('alanEtiketi', () => {
  it('bilinen alanı Türkçe etiketle gösterir', () => {
    expect(alanEtiketi('IseGirisTarihi')).toBe('İşe giriş tarihi')
    expect(alanEtiketi('Dakika')).toBe('Süre')
  })

  it('bilinmeyen alanı olduğu gibi bırakır', () => {
    expect(alanEtiketi('YeniAlan')).toBe('YeniAlan')
  })
})

describe('degerGoster', () => {
  it('boş değeri çizgiyle gösterir', () => {
    expect(degerGoster('Ad', null)).toBe('—')
  })

  it('mantıksal değerleri Evet ve Hayır yapar', () => {
    expect(degerGoster('Aktif', 'True')).toBe('Evet')
    expect(degerGoster('Aktif', 'False')).toBe('Hayır')
  })

  it('tarihi gün.ay.yıl biçiminde yazar', () => {
    expect(degerGoster('KazanimTarihi', '2026-10-29')).toBe('29.10.2026')
  })

  it('hak kaynağını ve gün hesabını Türkçe yazar', () => {
    expect(degerGoster('Kaynak', 'Elle')).toBe('Elle girildi')
    expect(degerGoster('GunHesabi', 'TakvimGunu')).toBe('Takvim günü')
  })

  it('rol adını etiketler', () => {
    expect(degerGoster('RoleId', 'Yonetici')).toBe('Yönetici')
  })

  it('sunucunun hazır yazdığı değere dokunmaz', () => {
    expect(degerGoster('Dakika', '2 gün 3 saat')).toBe('2 gün 3 saat')
    expect(degerGoster('Ad', 'Ayşe')).toBe('Ayşe')
  })
})
