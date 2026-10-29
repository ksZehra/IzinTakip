import { describe, expect, it } from 'vitest'

import type { TakvimKaydi } from '@/api/raporlarApi'

import { ayinSinirlari, gunlereDagit } from './takvimGunleri'

const hafta = ['Pazartesi', 'Sali', 'Carsamba', 'Persembe', 'Cuma']

function kayit(id: number, baslangic: string, bitis: string): TakvimKaydi {
  return {
    izinTalebiId: id,
    calisanId: id,
    calisanAdSoyad: `Çalışan ${id}`,
    departmanAdi: 'Yazılım',
    izinTuruAdi: 'Yıllık',
    sureTipi: 'TamGun',
    baslangic,
    bitis,
    durum: 'Onaylandi',
  }
}

describe('ayinSinirlari', () => {
  it('ayın ilk ve son gününü verir', () => {
    expect(ayinSinirlari(2028, 1)).toEqual({ baslangic: '2028-02-01', bitis: '2028-02-29' })
  })
})

describe('gunlereDagit', () => {
  it('çok günlü izni kapsadığı çalışma günlerine dağıtır, hafta sonunu atlar', () => {
    const gunler = gunlereDagit(
      [kayit(1, '2028-03-03T09:00:00', '2028-03-06T18:00:00')],
      '2028-03-01',
      '2028-03-31',
      hafta,
    )

    expect(gunler.map((gun) => gun.tarih)).toEqual(['2028-03-03', '2028-03-06'])
  })

  it('aynı günün izinlerini birlikte verir, izinsiz günü listelemez', () => {
    const gunler = gunlereDagit(
      [
        kayit(1, '2028-03-07T09:00:00', '2028-03-07T12:30:00'),
        kayit(2, '2028-03-07T09:00:00', '2028-03-07T18:00:00'),
      ],
      '2028-03-06',
      '2028-03-08',
      hafta,
    )

    expect(gunler).toHaveLength(1)
    expect(gunler[0]!.kayitlar.map((k) => k.izinTalebiId)).toEqual([1, 2])
  })
})
