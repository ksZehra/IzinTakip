export type FieldErrors = Record<string, string[]>

interface ApiHatasiBilgisi {
  durum: number
  mesaj: string
  alanHatalari?: FieldErrors
  izKodu?: string
  iptalEdildi?: boolean
}

export class ApiError extends Error {
  readonly durum: number
  readonly alanHatalari: FieldErrors
  readonly izKodu: string | undefined
  readonly iptalEdildi: boolean

  constructor({ durum, mesaj, alanHatalari = {}, izKodu, iptalEdildi = false }: ApiHatasiBilgisi) {
    super(mesaj)
    this.name = 'ApiError'
    this.durum = durum
    this.alanHatalari = alanHatalari
    this.izKodu = izKodu
    this.iptalEdildi = iptalEdildi
  }

  get alanHatasiVar(): boolean {
    return Object.keys(this.alanHatalari).length > 0
  }
}
