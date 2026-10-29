import type { IzinTalebiSatiri } from '@/api/izinTalepleriApi'
import { saatGoster, tarihGoster } from '@/common/utils/tarih'

export function talepAraligiMetni(
  talep: Pick<IzinTalebiSatiri, 'sureTipi' | 'baslangic' | 'bitis'>,
): string {
  const [basTarih = '', basSaat = ''] = talep.baslangic.split('T')
  const [bitTarih = '', bitSaat = ''] = talep.bitis.split('T')
  if (talep.sureTipi !== 'TamGun') {
    return `${tarihGoster(basTarih)}, ${saatGoster(basSaat)}–${saatGoster(bitSaat)}`
  }
  return basTarih === bitTarih
    ? tarihGoster(basTarih)
    : `${tarihGoster(basTarih)} – ${tarihGoster(bitTarih)}`
}
