import type { FilterValue } from '@/common/composables/usePagedList'

export interface GridSecenek<Deger extends string | number | boolean = string | number | boolean> {
  title: string
  value: Deger
}

export type GridFiltre =
  | { tur: 'metin'; anahtar: string }
  | { tur: 'tekliSecim'; anahtar: string; secenekler: GridSecenek[] }
  | { tur: 'cokluSecim'; anahtar: string; secenekler: GridSecenek<string | number>[] }
  | { tur: 'tarih'; baslangicAnahtari: string; bitisAnahtari: string }

export interface GridKolon<Satir> {
  key: string
  title: string
  deger?: (satir: Satir) => unknown
  sirala?: boolean
  filtre?: GridFiltre
  hizala?: 'start' | 'center' | 'end'
  // Yerel modda süzme ve sıralamada kullanılan ham değer; verilmezse satırın `key` alanı.
  ham?: (satir: Satir) => unknown
}

export type TarihOperatoru = 'eq' | 'gt' | 'ge' | 'lt' | 'le' | 'bw'

export const tarihOperatorSecenekleri: GridSecenek<TarihOperatoru>[] = [
  { title: 'Eşittir (=)', value: 'eq' },
  { title: 'Büyüktür (>)', value: 'gt' },
  { title: 'Büyük Eşittir (≥)', value: 'ge' },
  { title: 'Küçüktür (<)', value: 'lt' },
  { title: 'Küçük Eşittir (≤)', value: 'le' },
  { title: 'Arasında', value: 'bw' },
]

const tarihOperatorAdlari: Record<Exclude<TarihOperatoru, 'bw'>, string> = {
  eq: 'Eşittir',
  gt: 'Büyüktür',
  ge: 'Büyük Eşittir',
  lt: 'Küçüktür',
  le: 'Küçük Eşittir',
}

export function filtreAnahtarlari(tanim: GridFiltre): string[] {
  return tanim.tur === 'tarih' ? [tanim.baslangicAnahtari, tanim.bitisAnahtari] : [tanim.anahtar]
}

export function filtreDoluMu(deger: FilterValue | undefined) {
  return !(
    deger === null ||
    deger === undefined ||
    deger === '' ||
    (Array.isArray(deger) && deger.length === 0)
  )
}

export function bosDegerler(tanim: GridFiltre): [string, FilterValue][] {
  switch (tanim.tur) {
    case 'metin':
      return [[tanim.anahtar, '']]
    case 'tekliSecim':
      return [[tanim.anahtar, null]]
    case 'cokluSecim':
      return [[tanim.anahtar, []]]
    case 'tarih':
      return [
        [tanim.baslangicAnahtari, ''],
        [tanim.bitisAnahtari, ''],
      ]
  }
}

export function doluFiltreSayisi<Satir>(
  kolonlar: GridKolon<Satir>[],
  filtre: Record<string, FilterValue>,
): number {
  return kolonlar.filter(
    (kolon) =>
      kolon.filtre &&
      filtreAnahtarlari(kolon.filtre).some((anahtar) => filtreDoluMu(filtre[anahtar])),
  ).length
}

function gunEkle(tarih: string, gun: number) {
  const sonuc = new Date(`${tarih}T00:00:00Z`)
  sonuc.setUTCDate(sonuc.getUTCDate() + gun)
  return sonuc.toISOString().slice(0, 10)
}

export function kisaTarih(tarih: string) {
  return tarih.split('-').reverse().join('.')
}

export function tarihAraligi(operator: TarihOperatoru, a: string, b: string) {
  if (!a) return { baslangic: '', bitis: '' }
  switch (operator) {
    case 'eq':
      return { baslangic: a, bitis: a }
    case 'gt':
      return { baslangic: gunEkle(a, 1), bitis: '' }
    case 'ge':
      return { baslangic: a, bitis: '' }
    case 'lt':
      return { baslangic: '', bitis: gunEkle(a, -1) }
    case 'le':
      return { baslangic: '', bitis: a }
    case 'bw':
      return { baslangic: a, bitis: b }
  }
}

export function tarihHatasi(operator: TarihOperatoru, a: string, b: string) {
  if (operator !== 'bw') return ''
  if (!a !== !b) return 'İki tarihi de seçin.'
  return a && b && a > b ? 'Bitiş tarihi başlangıçtan önce olamaz.' : ''
}

export interface TarihDurumu {
  operator: TarihOperatoru
  a: string
  b: string
}

export function tarihDurumu(
  kayitliOperator: TarihOperatoru | undefined,
  baslangic: string,
  bitis: string,
): TarihDurumu {
  if (baslangic && bitis) {
    return baslangic === bitis
      ? { operator: 'eq', a: baslangic, b: '' }
      : { operator: 'bw', a: baslangic, b: bitis }
  }
  if (baslangic) {
    return kayitliOperator === 'gt'
      ? { operator: 'gt', a: gunEkle(baslangic, -1), b: '' }
      : { operator: 'ge', a: baslangic, b: '' }
  }
  if (bitis) {
    return kayitliOperator === 'lt'
      ? { operator: 'lt', a: gunEkle(bitis, 1), b: '' }
      : { operator: 'le', a: bitis, b: '' }
  }
  return { operator: 'eq', a: '', b: '' }
}

export function tarihOzeti({ operator, a, b }: TarihDurumu) {
  if (!a) return ''
  return operator === 'bw'
    ? `${kisaTarih(a)} ile ${kisaTarih(b)} arası`
    : `${tarihOperatorAdlari[operator]} ${kisaTarih(a)}`
}

export interface EtkinFiltre {
  kolonKey: string
  metin: string
}

export function etkinFiltreler<Satir>(
  kolonlar: GridKolon<Satir>[],
  filtre: Record<string, FilterValue>,
  operatorler: Record<string, TarihOperatoru | undefined>,
): EtkinFiltre[] {
  const sonuc: EtkinFiltre[] = []
  for (const kolon of kolonlar) {
    const tanim = kolon.filtre
    if (!tanim || !filtreAnahtarlari(tanim).some((anahtar) => filtreDoluMu(filtre[anahtar])))
      continue
    let deger = ''
    if (tanim.tur === 'metin') {
      deger = String(filtre[tanim.anahtar])
    } else if (tanim.tur === 'tekliSecim') {
      deger = tanim.secenekler.find((s) => s.value === filtre[tanim.anahtar])?.title ?? ''
    } else if (tanim.tur === 'cokluSecim') {
      const secili = filtre[tanim.anahtar]
      const adlar = tanim.secenekler
        .filter((s) => Array.isArray(secili) && secili.includes(s.value))
        .map((s) => s.title)
      deger =
        adlar.length > 2 ? `${adlar.slice(0, 2).join(', ')} +${adlar.length - 2}` : adlar.join(', ')
    } else {
      deger = tarihOzeti(
        tarihDurumu(
          operatorler[kolon.key],
          String(filtre[tanim.baslangicAnahtari] ?? ''),
          String(filtre[tanim.bitisAnahtari] ?? ''),
        ),
      )
    }
    sonuc.push({ kolonKey: kolon.key, metin: `${kolon.title}: ${deger}` })
  }
  return sonuc
}

function hamDeger<Satir>(kolon: GridKolon<Satir>, satir: Satir): unknown {
  return kolon.ham ? kolon.ham(satir) : (satir as Record<string, unknown>)[kolon.key]
}

function metne(deger: unknown): string {
  if (typeof deger === 'string') return deger
  return typeof deger === 'number' || typeof deger === 'boolean' ? String(deger) : ''
}

function kucukHarf(deger: unknown) {
  return metne(deger).toLocaleLowerCase('tr')
}

function eslesir<Satir>(
  kolon: GridKolon<Satir>,
  tanim: GridFiltre,
  satir: Satir,
  filtre: Record<string, FilterValue>,
): boolean {
  const ham = hamDeger(kolon, satir)
  switch (tanim.tur) {
    case 'metin': {
      const gorunen = kolon.deger ? kolon.deger(satir) : ham
      return kucukHarf(gorunen).includes(kucukHarf(filtre[tanim.anahtar]))
    }
    case 'tekliSecim':
      return ham === filtre[tanim.anahtar]
    case 'cokluSecim': {
      const secili = filtre[tanim.anahtar]
      // Hücrede birden çok değer olabilir (ör. kullanıcının rolleri); biri seçiliyse satır eşleşir.
      const degerler = Array.isArray(ham) ? ham : [ham]
      return Array.isArray(secili) && degerler.some((d) => secili.includes(d as string | number))
    }
    case 'tarih': {
      const tarih = metne(ham).slice(0, 10)
      const baslangic = metne(filtre[tanim.baslangicAnahtari])
      const bitis = metne(filtre[tanim.bitisAnahtari])
      return tarih !== '' && (!baslangic || tarih >= baslangic) && (!bitis || tarih <= bitis)
    }
  }
}

export function yerelFiltrele<Satir>(
  kolonlar: GridKolon<Satir>[],
  satirlar: Satir[],
  filtre: Record<string, FilterValue>,
): Satir[] {
  const etkin = kolonlar.filter(
    (kolon) =>
      kolon.filtre &&
      filtreAnahtarlari(kolon.filtre).some((anahtar) => filtreDoluMu(filtre[anahtar])),
  )
  if (!etkin.length) return satirlar
  return satirlar.filter((satir) =>
    etkin.every((kolon) => eslesir(kolon, kolon.filtre!, satir, filtre)),
  )
}

function bosMu(deger: unknown) {
  return deger === null || deger === undefined || deger === ''
}

function karsilastir(a: unknown, b: unknown): number {
  if (bosMu(a) || bosMu(b)) return Number(bosMu(a)) - Number(bosMu(b))
  if (typeof a === 'number' && typeof b === 'number') return a - b
  if (typeof a === 'boolean' && typeof b === 'boolean') return Number(a) - Number(b)
  return metne(a).localeCompare(metne(b), 'tr', { numeric: true })
}

export function yerelSirala<Satir>(
  kolonlar: GridKolon<Satir>[],
  satirlar: Satir[],
  sirala: string,
): Satir[] {
  const [key, yon] = sirala.split(':')
  const kolon = kolonlar.find((k) => k.key === key && k.sirala)
  if (!kolon || (yon !== 'asc' && yon !== 'desc')) return satirlar
  const carpan = yon === 'desc' ? -1 : 1
  return [...satirlar].sort((a, b) => carpan * karsilastir(hamDeger(kolon, a), hamDeger(kolon, b)))
}
