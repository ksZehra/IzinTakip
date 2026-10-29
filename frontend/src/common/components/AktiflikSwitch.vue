<script setup lang="ts">
import { ref, watch } from 'vue'

import { ApiError } from '@/common/http/ApiError'

const props = defineProps<{
  aktif: boolean
  etiket: string
  degistir: (aktif: boolean) => Promise<void>
  kilitli?: boolean
}>()

const deger = ref(props.aktif)
const calisiyor = ref(false)

watch(
  () => props.aktif,
  (yeni) => (deger.value = yeni),
)

async function guncelle(yeni: boolean | null) {
  const istenen = yeni ?? false
  deger.value = istenen
  calisiyor.value = true
  try {
    await props.degistir(istenen)
  } catch (hata) {
    deger.value = !istenen
    if (!(hata instanceof ApiError)) throw hata
  } finally {
    calisiyor.value = false
  }
}
</script>

<template>
  <v-switch
    :model-value="deger"
    color="primary"
    hide-details
    density="compact"
    :loading="calisiyor"
    :disabled="calisiyor || kilitli"
    :aria-label="etiket"
    @update:model-value="guncelle"
  />
</template>
