import { computed, reactive, ref, shallowRef, watch, type Ref, type UnwrapRef } from 'vue'
import { useRoute, useRouter, type LocationQuery } from 'vue-router'

import { ApiError } from '@/common/http/ApiError'

export type FilterValue = string | number | boolean | null | (string | number)[]
export type Page<Satir> = { items: Satir[]; totalCount: number }
export type ListQuery<Filtre> = Filtre & { page: number; pageSize: number }

const BEKLEME_MS = 300

export function filtreyiOku<Filtre extends Record<string, FilterValue>>(
  varsayilan: Filtre,
  sorgu: LocationQuery,
  sayiListeleri: readonly string[] = [],
): Filtre {
  const sonuc: Record<string, FilterValue> = { ...varsayilan }
  for (const [alan, ilkDeger] of Object.entries(varsayilan)) {
    const ham = sorgu[alan]
    if (Array.isArray(ilkDeger)) {
      const liste = (Array.isArray(ham) ? ham : [ham]).filter(
        (deger): deger is string => typeof deger === 'string' && deger !== '',
      )
      if (liste.length > 0) sonuc[alan] = sayiListeleri.includes(alan) ? liste.map(Number) : liste
      continue
    }
    if (typeof ham !== 'string') continue
    // Boş değer, varsayılanı dolu bir filtrenin bilerek temizlendiğini gösterir.
    if (ham === '') sonuc[alan] = typeof ilkDeger === 'string' ? '' : null
    else if (typeof ilkDeger === 'boolean' || (ilkDeger === null && /^(true|false)$/.test(ham)))
      sonuc[alan] = ham === 'true'
    else if (typeof ilkDeger === 'number' || (ilkDeger === null && /^\d+$/.test(ham)))
      sonuc[alan] = Number(ham)
    else sonuc[alan] = ham
  }
  return sonuc as Filtre
}

function bos(deger: FilterValue | undefined) {
  return (
    deger === null || deger === undefined || deger === '' || (Array.isArray(deger) && !deger.length)
  )
}

function esit(a: FilterValue | undefined, b: FilterValue | undefined) {
  if (Array.isArray(a) && Array.isArray(b)) {
    return a.length === b.length && a.every((deger, sira) => deger === b[sira])
  }
  return a === b
}

export function filtreyiYaz(
  filtre: Record<string, FilterValue>,
  varsayilan: Record<string, FilterValue>,
): Record<string, string | string[]> {
  return Object.fromEntries(
    Object.entries(filtre)
      .filter(
        ([alan, deger]) => !esit(deger, varsayilan[alan]) && !(bos(deger) && bos(varsayilan[alan])),
      )
      .map(([alan, deger]) => [
        alan,
        Array.isArray(deger)
          ? deger.length
            ? deger.map(String)
            : ['']
          : bos(deger)
            ? ''
            : String(deger),
      ]),
  )
}

export function usePagedList<Satir, Filtre extends Record<string, FilterValue>>(
  getir: (sorgu: ListQuery<Filtre>, signal: AbortSignal) => Promise<Page<Satir>>,
  varsayilanFiltre: Filtre,
  // Boş varsayılan diziden eleman tipi anlaşılamaz; adresten sayı olarak okunacak listeler burada verilir.
  { sayiListeleri = [] }: { sayiListeleri?: readonly (keyof Filtre & string)[] } = {},
) {
  const route = useRoute()
  const router = useRouter()

  const filtre = reactive(filtreyiOku(varsayilanFiltre, route.query, sayiListeleri))
  const sayfa = ref(1)
  const sayfaBoyutu = ref(20)
  const satirlar = shallowRef<Satir[]>([]) as Ref<Satir[]>
  const toplam = ref(0)
  const yukleniyor = ref(false)
  const varsayilandanFarkli = computed(
    () =>
      Object.keys(filtreyiYaz(filtre as Record<string, FilterValue>, varsayilanFiltre)).length > 0,
  )
  let denetleyici: AbortController | undefined
  let bekleme: ReturnType<typeof setTimeout> | undefined

  async function yukle(): Promise<void> {
    denetleyici?.abort()
    const istek = (denetleyici = new AbortController())
    yukleniyor.value = true
    try {
      const sonuc = await getir(
        { ...(filtre as Filtre), page: sayfa.value, pageSize: sayfaBoyutu.value },
        istek.signal,
      )
      satirlar.value = sonuc.items
      toplam.value = sonuc.totalCount
    } catch (hata) {
      if (!(hata instanceof ApiError)) throw hata
    } finally {
      if (denetleyici === istek) yukleniyor.value = false
    }
  }

  function filtreyiAyarla(anahtar: string, deger: FilterValue) {
    ;(filtre as Record<string, FilterValue>)[anahtar] = deger
  }

  function filtreyiSifirla() {
    for (const [alan, deger] of Object.entries(varsayilanFiltre)) {
      filtreyiAyarla(alan, Array.isArray(deger) ? [...deger] : deger)
    }
  }

  watch(
    () => ({ ...filtre }) as UnwrapRef<Filtre>,
    () => {
      clearTimeout(bekleme)
      bekleme = setTimeout(() => {
        sayfa.value = 1
        void router.replace({ query: filtreyiYaz(filtre, varsayilanFiltre) })
        void yukle()
      }, BEKLEME_MS)
    },
    { deep: true },
  )
  watch([sayfa, sayfaBoyutu], () => void yukle())

  void yukle()

  return {
    filtre,
    sayfa,
    sayfaBoyutu,
    satirlar,
    toplam,
    yukleniyor,
    varsayilandanFarkli,
    yukle,
    filtreyiAyarla,
    filtreyiSifirla,
  }
}
