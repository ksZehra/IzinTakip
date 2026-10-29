import { computed, ref, type Ref } from 'vue'

import { useFormSubmit } from './useFormSubmit'
import { useToast } from './useToast'

interface CrudFormOptions<Kayit, Istek> {
  bosForm: () => Istek
  formaCevir: (kayit: Kayit) => Istek
  ekle: (istek: Istek) => Promise<unknown>
  guncelle: (id: number, istek: Istek) => Promise<unknown>
  kaydedildi: () => Promise<void> | void
}

export function useCrudForm<Kayit extends { id: number }, Istek>(
  secenekler: CrudFormOptions<Kayit, Istek>,
) {
  const toast = useToast()
  const { gonder, gonderiliyor, alanHatasi, alanHatalari } = useFormSubmit()
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
