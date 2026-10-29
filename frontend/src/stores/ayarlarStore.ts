import { defineStore } from 'pinia'
import { ref } from 'vue'

import { ayarlariGetir, type Ayarlar } from '@/api/ayarlarApi'

export const useAyarlarStore = defineStore('ayarlar', () => {
  const ayarlar = ref<Ayarlar | null>(null)

  async function yukle(): Promise<Ayarlar> {
    ayarlar.value ??= await ayarlariGetir()
    return ayarlar.value
  }

  function guncellendi(yeni: Ayarlar) {
    ayarlar.value = yeni
  }

  return { ayarlar, yukle, guncellendi }
})
