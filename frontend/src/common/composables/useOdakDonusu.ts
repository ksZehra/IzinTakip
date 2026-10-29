import { onBeforeUnmount, watch, type Ref } from 'vue'

// Dialog v-model ile açıldığında Vuetify odağı tetikleyen düğmeye geri vermez.
export function useOdakDonusu(acik: Ref<boolean>) {
  let tetikleyen: HTMLElement | null = null

  watch(
    acik,
    (acildi) => {
      if (acildi && document.activeElement instanceof HTMLElement) {
        tetikleyen = document.activeElement
      }
    },
    { immediate: true },
  )

  function odagiGeriVer() {
    if (tetikleyen?.isConnected) tetikleyen.focus()
    tetikleyen = null
  }

  onBeforeUnmount(odagiGeriVer)

  return { odagiGeriVer }
}
