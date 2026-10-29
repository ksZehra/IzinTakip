<script setup lang="ts">
import { onMounted, ref, shallowRef, watch } from 'vue'

import { calisanlariGetir } from '@/api/calisanlarApi'

interface Secenek {
  id: number
  adSoyad: string
}

const props = defineProps<{
  etiket: string
  seciliAd?: string | null
  yerTutucu?: string
  hatalar?: string[]
}>()
const calisanId = defineModel<number | null>({ required: true })

const BEKLEME_MS = 300
const secenekler = shallowRef<Secenek[]>([])
const arama = ref('')
const yukleniyor = ref(false)
let bekleme: ReturnType<typeof setTimeout> | undefined
let sonIstek = 0

async function ara(metin: string) {
  const bu = ++sonIstek
  yukleniyor.value = true
  try {
    const sonuc = await calisanlariGetir({ arama: metin, aktif: true, pageSize: 20 })
    if (bu !== sonIstek) return
    const bulunanlar = sonuc.items.map((c) => ({ id: c.id, adSoyad: `${c.ad} ${c.soyad}` }))
    const secili =
      calisanId.value !== null &&
      props.seciliAd &&
      !bulunanlar.some((c) => c.id === calisanId.value)
        ? [{ id: calisanId.value, adSoyad: props.seciliAd }]
        : []
    secenekler.value = [...secili, ...bulunanlar]
  } finally {
    if (bu === sonIstek) yukleniyor.value = false
  }
}

watch(arama, (metin) => {
  clearTimeout(bekleme)
  bekleme = setTimeout(() => void ara(metin ?? ''), BEKLEME_MS)
})

watch(
  () => props.seciliAd,
  () => void ara(arama.value ?? ''),
)

onMounted(() => void ara(''))
</script>

<template>
  <v-autocomplete
    v-model="calisanId"
    v-model:search="arama"
    :items="secenekler"
    item-title="adSoyad"
    item-value="id"
    :label="etiket"
    :placeholder="yerTutucu"
    :persistent-placeholder="!!yerTutucu"
    :loading="yukleniyor"
    :error-messages="hatalar"
    no-filter
    clearable
    no-data-text="Eşleşen çalışan yok."
  />
</template>
