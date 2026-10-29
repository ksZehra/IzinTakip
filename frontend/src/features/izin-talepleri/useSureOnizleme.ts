import { onUnmounted, ref, watch, type Ref } from 'vue'

import { sureHesapla, type SureHesaplaIstegi } from '@/api/izinTalepleriApi'
import { ApiHatasi } from '@/common/http/ApiHatasi'

const BEKLEME_MS = 400

export function useSureOnizleme(istek: Ref<SureHesaplaIstegi | null>) {
  const sureMetni = ref('')
  const hata = ref('')
  let denetleyici: AbortController | undefined
  let bekleme: ReturnType<typeof setTimeout> | undefined

  async function hesapla(gonderilecek: SureHesaplaIstegi) {
    denetleyici?.abort()
    const bu = (denetleyici = new AbortController())
    try {
      sureMetni.value = (await sureHesapla(gonderilecek, bu.signal)).sureMetni
      hata.value = ''
    } catch (yakalanan) {
      if (!(yakalanan instanceof ApiHatasi)) throw yakalanan
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
