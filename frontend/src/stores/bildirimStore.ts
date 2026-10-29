import { defineStore } from 'pinia'
import { ref } from 'vue'

import { okunmamisSayisiniGetir } from '@/api/bildirimlerApi'

const YENILEME_ARALIGI_MS = 60_000

export const useBildirimStore = defineStore('bildirim', () => {
  const okunmamisSayisi = ref(0)
  let zamanlayici: ReturnType<typeof setInterval> | undefined

  async function sayiyiYenile(): Promise<void> {
    try {
      okunmamisSayisi.value = await okunmamisSayisiniGetir()
    } catch {
      return
    }
  }

  function gorunurlukDegisti() {
    if (document.visibilityState === 'visible') {
      void sayiyiYenile()
    }
  }

  function izlemeyiBaslat() {
    izlemeyiDurdur()
    void sayiyiYenile()
    zamanlayici = setInterval(() => {
      if (document.visibilityState === 'visible') {
        void sayiyiYenile()
      }
    }, YENILEME_ARALIGI_MS)
    document.addEventListener('visibilitychange', gorunurlukDegisti)
  }

  function izlemeyiDurdur() {
    okunmamisSayisi.value = 0
    clearInterval(zamanlayici)
    zamanlayici = undefined
    document.removeEventListener('visibilitychange', gorunurlukDegisti)
  }

  return { okunmamisSayisi, sayiyiYenile, izlemeyiBaslat, izlemeyiDurdur }
})
