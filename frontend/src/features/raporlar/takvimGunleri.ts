import type { TakvimKaydi } from '@/api/raporlarApi'
import { apiTarihi, tarihNesnesi } from '@/common/utils/tarih'

const haftaGunleri = ['Pazar', 'Pazartesi', 'Sali', 'Carsamba', 'Persembe', 'Cuma', 'Cumartesi']

export interface TakvimGunu {
  tarih: string
  kayitlar: TakvimKaydi[]
}

export function ayinSinirlari(yil: number, ay: number): { baslangic: string; bitis: string } {
  return { baslangic: apiTarihi(new Date(yil, ay, 1)), bitis: apiTarihi(new Date(yil, ay + 1, 0)) }
}

export function gunlereDagit(
  kayitlar: TakvimKaydi[],
  baslangic: string,
  bitis: string,
  calismaGunleri: string[],
): TakvimGunu[] {
  const gunler: TakvimGunu[] = []
  for (let gun = tarihNesnesi(baslangic); apiTarihi(gun) <= bitis; gun.setDate(gun.getDate() + 1)) {
    const tarih = apiTarihi(gun)
    if (!calismaGunleri.includes(haftaGunleri[gun.getDay()]!)) continue
    const gunun = kayitlar.filter(
      (kayit) => kayit.baslangic.slice(0, 10) <= tarih && tarih <= kayit.bitis.slice(0, 10),
    )
    if (gunun.length > 0) gunler.push({ tarih, kayitlar: gunun })
  }
  return gunler
}
