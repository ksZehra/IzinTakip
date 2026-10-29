import type { GunYarisi, IzinDurumu, SureTipi } from '@/api/izinTalepleriApi'
import type { Schema } from '@/api/types'

export const durumEtiketleri: Record<IzinDurumu, string> = {
  Beklemede: 'Beklemede',
  Onaylandi: 'Onaylandı',
  Reddedildi: 'Reddedildi',
  IptalEdildi: 'İptal edildi',
}

export const sureTipiEtiketleri: Record<SureTipi, string> = {
  TamGun: 'Tam gün',
  YarimGun: 'Yarım gün',
  Saatlik: 'Saatlik',
}

export const gunYarisiEtiketleri: Record<GunYarisi, string> = {
  Sabah: 'Sabah',
  OgledenSonra: 'Öğleden sonra',
}

export const gunHesabiEtiketleri: Record<Schema<'GunHesabi'>, string> = {
  IsGunu: 'İş günü',
  TakvimGunu: 'Takvim günü',
}

export const haftaGunuEtiketleri: Record<Schema<'HaftaGunleri'>, string> = {
  Pazartesi: 'Pazartesi',
  Sali: 'Salı',
  Carsamba: 'Çarşamba',
  Persembe: 'Perşembe',
  Cuma: 'Cuma',
  Cumartesi: 'Cumartesi',
  Pazar: 'Pazar',
}

export function secenekler<Deger extends string>(etiketler: Record<Deger, string>) {
  return (Object.entries(etiketler) as [Deger, string][]).map(([value, title]) => ({
    value,
    title,
  }))
}
