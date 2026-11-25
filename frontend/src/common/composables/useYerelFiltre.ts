import { watch, type UnwrapRef } from 'vue'

import { useFiltreDurumu, type FiltreSecenekleri } from './useFiltreDurumu'
import type { FilterValue } from './usePagedList'

export function useYerelFiltre<Filtre extends Record<string, FilterValue>>(
  varsayilanFiltre: Filtre,
  secenekler: FiltreSecenekleri<Filtre> = {},
) {
  const { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla, kaydet } = useFiltreDurumu(
    varsayilanFiltre,
    secenekler,
  )

  watch(() => ({ ...filtre }) as UnwrapRef<Filtre>, kaydet, { deep: true })

  return { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla }
}
