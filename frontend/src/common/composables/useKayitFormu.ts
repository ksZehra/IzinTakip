import { computed, ref, type Ref } from 'vue'

import { useFormGonder } from './useFormGonder'
import { useToast } from './useToast'

interface KayitFormuSecenekleri<Kayit, Istek> {
  bosForm: () => Istek
  formaCevir: (kayit: Kayit) => Istek
  ekle: (istek: Istek) => Promise<unknown>
  guncelle: (id: number, istek: Istek) => Promise<unknown>
  kaydedildi: () => Promise<void> | void
}

export function useKayitFormu<Kayit extends { id: number }, Istek>(
  secenekler: KayitFormuSecenekleri<Kayit, Istek>,
) {
  const toast = useToast()
  const { gonder, gonderiliyor, alanHatasi, alanHatalari } = useFormGonder()
  const acik = ref(false)
  const duzenlenenId = ref<number | null>(null)
  const form = ref(secenekler.bosForm()) as Ref<Istek>

  const yeniKayit = computed(() => duzenlenenId.value === null)

  function yeni() {
    duzenlenenId.value = null
    form.value = secenekler.bosForm()
    alanHatalari.value = {}
    acik.value = true
  }

  function duzenle(kayit: Kayit) {
    duzenlenenId.value = kayit.id
    form.value = secenekler.formaCevir(kayit)
    alanHatalari.value = {}
    acik.value = true
  }

  async function kaydet() {
    const id = duzenlenenId.value
    const basarili = await gonder(async () => {
      await (id === null ? secenekler.ekle(form.value) : secenekler.guncelle(id, form.value))
      return true
    })
    if (!basarili) return
    acik.value = false
    toast.basariGoster('Kaydedildi.')
    await secenekler.kaydedildi()
  }

  return { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi }
}
