import { ref } from 'vue'

import { ApiHatasi, type AlanHatalari } from '@/common/http/ApiHatasi'

export function useFormGonder() {
  const gonderiliyor = ref(false)
  const alanHatalari = ref<AlanHatalari>({})
  const hataMesaji = ref('')

  async function gonder<T>(islem: () => Promise<T>): Promise<T | undefined> {
    if (gonderiliyor.value) {
      return undefined
    }
    gonderiliyor.value = true
    alanHatalari.value = {}
    hataMesaji.value = ''
    try {
      return await islem()
    } catch (hata) {
      if (hata instanceof ApiHatasi) {
        alanHatalari.value = hata.alanHatalari
        hataMesaji.value = hata.alanHatasiVar ? '' : hata.message
        return undefined
      }
      throw hata
    } finally {
      gonderiliyor.value = false
    }
  }

  function alanHatasi(alan: string): string[] {
    return alanHatalari.value[alan] ?? []
  }

  return { gonderiliyor, alanHatalari, hataMesaji, gonder, alanHatasi }
}
