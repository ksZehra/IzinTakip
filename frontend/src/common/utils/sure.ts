export interface SureParcalari {
  gun: number
  saat: number
  dakika: number
  eksi: boolean
}

export function dakikayaCevir(sure: SureParcalari, gunlukCalismaDakikasi: number): number {
  const toplam = sure.gun * gunlukCalismaDakikasi + sure.saat * 60 + sure.dakika
  return sure.eksi ? -toplam : toplam
}

export function dakikayiParcala(dakika: number, gunlukCalismaDakikasi: number): SureParcalari {
  const mutlak = Math.abs(dakika)
  const gunIciDakika = mutlak % gunlukCalismaDakikasi
  return {
    gun: Math.floor(mutlak / gunlukCalismaDakikasi),
    saat: Math.floor(gunIciDakika / 60),
    dakika: gunIciDakika % 60,
    eksi: dakika < 0,
  }
}
