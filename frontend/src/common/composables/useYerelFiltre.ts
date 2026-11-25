import { computed, reactive, watch, type UnwrapRef } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { filtreyiOku, filtreyiYaz, type FilterValue } from './usePagedList'

export function useYerelFiltre<Filtre extends Record<string, FilterValue>>(
  varsayilanFiltre: Filtre,
  { sayiListeleri = [] }: { sayiListeleri?: readonly (keyof Filtre & string)[] } = {},
) {
  const route = useRoute()
  const router = useRouter()

  const filtre = reactive(filtreyiOku(varsayilanFiltre, route.query, sayiListeleri))
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

  watch(
    () => ({ ...filtre }) as UnwrapRef<Filtre>,
    () => void router.replace({ query: filtreyiYaz(filtre, varsayilanFiltre) }),
    { deep: true },
  )

  return { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla }
}
