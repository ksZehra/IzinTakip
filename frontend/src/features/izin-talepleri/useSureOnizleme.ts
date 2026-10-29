import { onUnmounted, ref, watch, type Ref } from 'vue'

import { sureHesapla, type SureHesaplaRequest } from '@/api/izinTalepleriApi'
import { ApiError } from '@/common/http/ApiError'

const BEKLEME_MS = 400

export function useSureOnizleme(istek: Ref<SureHesaplaRequest | null>) {
  const sureMetni = ref('')
  const hata = ref('')
  let denetleyici: AbortController | undefined
  let bekleme: ReturnType<typeof setTimeout> | undefined

  async function hesapla(gonderilecek: SureHesaplaRequest) {
    denetleyici?.abort()
    const bu = (denetleyici = new AbortController())
    try {
      sureMetni.value = (await sureHesapla(gonderilecek, bu.signal)).sureMetni
      hata.value = ''
    } catch (yakalanan) {
      if (!(yakalanan instanceof ApiError)) throw yakalanan
      if (yakalanan.iptalEdildi) return
      sureMetni.value = ''
      hata.value = yakalanan.alanHatasiVar
        ? Object.values(yakalanan.alanHatalari).flat().join(' ')
        : yakalanan.message
    }
  }

  watch(
    istek,
    (yeni) => {
      clearTimeout(bekleme)
      denetleyici?.abort()
      sureMetni.value = ''
      hata.value = ''
      if (yeni) bekleme = setTimeout(() => void hesapla(yeni), BEKLEME_MS)
    },
    { deep: true },
  )

  onUnmounted(() => {
    clearTimeout(bekleme)
    denetleyici?.abort()
  })

  return { sureMetni, hata }
}
