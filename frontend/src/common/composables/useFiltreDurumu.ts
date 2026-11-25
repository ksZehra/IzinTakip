import { computed, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { filtreyiOku, filtreyiYaz, type FilterValue } from './usePagedList'

export interface FiltreSecenekleri<Filtre> {
  sayiListeleri?: readonly (keyof Filtre & string)[]
  gizliAlanlar?: readonly (keyof Filtre & string)[]
}

function oturumuOku(anahtar: string): Record<string, string> {
  try {
    const ham = sessionStorage.getItem(anahtar)
    const deger: unknown = ham ? JSON.parse(ham) : {}
    return deger && typeof deger === 'object' ? (deger as Record<string, string>) : {}
  } catch {
    return {}
  }
}

function oturumaYaz(anahtar: string, deger: Record<string, string>) {
  try {
    if (Object.keys(deger).length) sessionStorage.setItem(anahtar, JSON.stringify(deger))
    else sessionStorage.removeItem(anahtar)
  } catch {
    return
  }
}

export function useFiltreDurumu<Filtre extends Record<string, FilterValue>>(
  varsayilanFiltre: Filtre,
  { sayiListeleri = [], gizliAlanlar = [] }: FiltreSecenekleri<Filtre> = {},
) {
  const route = useRoute()
  const router = useRouter()
  const oturumAnahtari = `filtre:${route.path}`
  const gizli = new Set<string>(gizliAlanlar)

  const adrestekiler = Object.fromEntries(
    Object.entries(route.query).filter(([alan]) => !gizli.has(alan)),
  )
  const filtre = reactive(
    filtreyiOku(
      varsayilanFiltre,
      { ...adrestekiler, ...oturumuOku(oturumAnahtari) },
      sayiListeleri,
    ),
  )
  const varsayilandanFarkli = computed(
    () =>
      Object.keys(filtreyiYaz(filtre as Record<string, FilterValue>, varsayilanFiltre)).length > 0,
  )

  function filtreyiAyarla(anahtar: string, deger: FilterValue) {
    ;(filtre as Record<string, FilterValue>)[anahtar] = deger
  }

  function filtreyiSifirla() {
    for (const [alan, deger] of Object.entries(varsayilanFiltre)) {
      filtreyiAyarla(alan, Array.isArray(deger) ? [...deger] : deger)
    }
  }

  function kaydet() {
    const yazilacak = filtreyiYaz(filtre, varsayilanFiltre)
    const adres: Record<string, string | string[]> = {}
    const oturum: Record<string, string> = {}
    for (const [alan, deger] of Object.entries(yazilacak)) {
      if (gizli.has(alan) && typeof deger === 'string') oturum[alan] = deger
      else adres[alan] = deger
    }
    oturumaYaz(oturumAnahtari, oturum)
    void router.replace({ query: adres })
  }

  return { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla, kaydet }
}
