import { reactive, ref, shallowRef, watch, type Ref, type UnwrapRef } from 'vue'
import { useRoute, useRouter, type LocationQuery } from 'vue-router'

import { ApiHatasi } from '@/common/http/ApiHatasi'

export type FiltreDegeri = string | number | boolean | null
export type Sayfa<Satir> = { items: Satir[]; totalCount: number }
export type ListeSorgusu<Filtre> = Filtre & { page: number; pageSize: number }

const BEKLEME_MS = 300

export function filtreyiOku<Filtre extends Record<string, FiltreDegeri>>(
  varsayilan: Filtre,
  sorgu: LocationQuery,
): Filtre {
  const sonuc: Record<string, FiltreDegeri> = { ...varsayilan }
  for (const [alan, ilkDeger] of Object.entries(varsayilan)) {
    const ham = sorgu[alan]
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

function bos(deger: FiltreDegeri | undefined) {
  return deger === null || deger === undefined || deger === ''
}

export function filtreyiYaz(
  filtre: Record<string, FiltreDegeri>,
  varsayilan: Record<string, FiltreDegeri>,
): Record<string, string> {
  return Object.fromEntries(
    Object.entries(filtre)
      .filter(
        ([alan, deger]) => deger !== varsayilan[alan] && !(bos(deger) && bos(varsayilan[alan])),
      )
      .map(([alan, deger]) => [alan, bos(deger) ? '' : String(deger)]),
  )
}

export function useSayfaliListe<Satir, Filtre extends Record<string, FiltreDegeri>>(
  getir: (sorgu: ListeSorgusu<Filtre>, signal: AbortSignal) => Promise<Sayfa<Satir>>,
  varsayilanFiltre: Filtre,
  sayfaBoyutuVarsayilani = 20,
) {
  const route = useRoute()
  const router = useRouter()

  const filtre = reactive(filtreyiOku(varsayilanFiltre, route.query))
  const sayfa = ref(1)
  const sayfaBoyutu = ref(sayfaBoyutuVarsayilani)
  const satirlar = shallowRef<Satir[]>([]) as Ref<Satir[]>
  const toplam = ref(0)
  const yukleniyor = ref(false)
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
      if (!(hata instanceof ApiHatasi)) throw hata
    } finally {
      if (denetleyici === istek) yukleniyor.value = false
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

  return { filtre, sayfa, sayfaBoyutu, satirlar, toplam, yukleniyor, yukle }
}
