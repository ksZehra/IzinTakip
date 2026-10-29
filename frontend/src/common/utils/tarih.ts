const tarihBicimi = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
})
const tarihSaatBicimi = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

const ikiHane = (sayi: number) => String(sayi).padStart(2, '0')

export function apiTarihi(tarih: Date): string {
  return `${tarih.getFullYear()}-${ikiHane(tarih.getMonth() + 1)}-${ikiHane(tarih.getDate())}`
}

export function tarihNesnesi(deger: string): Date {
  if (/^\d{4}-\d{2}-\d{2}$/.test(deger)) {
    const [yil, ay, gun] = deger.split('-').map(Number)
    return new Date(yil!, ay! - 1, gun)
  }
  return new Date(deger)
}

export function tarihGoster(deger: string): string {
  return tarihBicimi.format(tarihNesnesi(deger))
}

export function tarihSaatGoster(deger: string): string {
  return tarihSaatBicimi.format(tarihNesnesi(deger))
}

export function saatGoster(deger: string): string {
  return deger.slice(0, 5)
}

export function bugun(): string {
  return apiTarihi(new Date())
}
