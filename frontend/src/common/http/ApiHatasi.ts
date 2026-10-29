export type AlanHatalari = Record<string, string[]>

interface ApiHatasiBilgisi {
  durum: number
  mesaj: string
  alanHatalari?: AlanHatalari
  izKodu?: string
  iptalEdildi?: boolean
}

export class ApiHatasi extends Error {
  readonly durum: number
  readonly alanHatalari: AlanHatalari
  readonly izKodu: string | undefined
  readonly iptalEdildi: boolean

  constructor({ durum, mesaj, alanHatalari = {}, izKodu, iptalEdildi = false }: ApiHatasiBilgisi) {
    super(mesaj)
    this.name = 'ApiHatasi'
    this.durum = durum
    this.alanHatalari = alanHatalari
    this.izKodu = izKodu
    this.iptalEdildi = iptalEdildi
  }

  get alanHatasiVar(): boolean {
    return Object.keys(this.alanHatalari).length > 0
  }
}
