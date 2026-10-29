import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import * as authApi from '@/api/authApi'
import type { Oturum } from '@/api/authApi'
import type { Yetki } from '@/common/auth/yetkiler'
import { useToast } from '@/common/composables/useToast'
import { ApiHatasi } from '@/common/http/ApiHatasi'

export const useAuthStore = defineStore('auth', () => {
  const oturum = ref<Oturum | null>(null)
  const yuklendi = ref(false)

  const girisYapildi = computed(() => oturum.value !== null)

  function yetkiVar(...yetkiler: Yetki[]): boolean {
    return yetkiler.some((yetki) => oturum.value?.yetkiler.includes(yetki) ?? false)
  }

  async function oturumuYukle(): Promise<void> {
    try {
      oturum.value = await authApi.oturumuGetir()
      yuklendi.value = true
    } catch (hata) {
      if (!(hata instanceof ApiHatasi)) {
        throw hata
      }
      oturum.value = null
      yuklendi.value = hata.durum === 401
      if (hata.durum !== 401) {
        useToast().hataGoster(hata.message)
      }
    }
  }

  async function girisYap(eposta: string, parola: string): Promise<void> {
    await authApi.girisYap(eposta, parola)
    await oturumuYukle()
  }

  async function cikisYap(): Promise<void> {
    try {
      await authApi.cikisYap()
    } finally {
      temizle()
    }
  }

  function temizle(): void {
    oturum.value = null
    yuklendi.value = true
  }

  return { oturum, yuklendi, girisYapildi, yetkiVar, oturumuYukle, girisYap, cikisYap, temizle }
})
