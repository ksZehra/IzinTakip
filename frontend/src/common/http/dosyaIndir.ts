import { httpClient } from './httpClient'

export function dosyaAdiniOku(icerikYerlesimi: string | undefined, varsayilan: string): string {
  const kodlanmis = icerikYerlesimi?.match(/filename\*=UTF-8''([^;]+)/i)?.[1]
  if (kodlanmis) {
    return decodeURIComponent(kodlanmis)
  }
  return icerikYerlesimi?.match(/filename="?([^";]+)"?/i)?.[1] ?? varsayilan
}

export async function dosyaIndir(
  adres: string,
  parametreler: Record<string, unknown> | undefined,
  varsayilanAd: string,
): Promise<void> {
  const yanit = await httpClient.get<Blob>(adres, { params: parametreler, responseType: 'blob' })
  const baglanti = document.createElement('a')
  baglanti.href = URL.createObjectURL(yanit.data)
  baglanti.download = dosyaAdiniOku(
    yanit.headers['content-disposition'] as string | undefined,
    varsayilanAd,
  )
  baglanti.click()
  URL.revokeObjectURL(baglanti.href)
}
