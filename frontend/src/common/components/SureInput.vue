<script setup lang="ts">
import { computed, onMounted } from 'vue'

import { dakikayaCevir, dakikayiParcala, type SureParcalari } from '@/common/utils/sure'
import { useAyarlarStore } from '@/stores/ayarlarStore'

const props = defineProps<{
  etiket: string
  eksiOlabilir?: boolean
  bosOlabilir?: boolean
  hatalar?: string[]
}>()
const dakika = defineModel<number | null>({ required: true })

const ayarlarStore = useAyarlarStore()
const gunluk = computed(() => ayarlarStore.ayarlar?.gunlukCalismaDakikasi ?? 480)
const parcalar = computed(() => dakikayiParcala(dakika.value ?? 0, gunluk.value))

function guncelle(alan: keyof SureParcalari, deger: number | boolean | null) {
  const yeni = { ...parcalar.value, [alan]: deger ?? (alan === 'eksi' ? false : 0) }
  const toplam = dakikayaCevir(yeni, gunluk.value)
  dakika.value = props.bosOlabilir && toplam === 0 && !yeni.eksi ? null : toplam
}

onMounted(() => void ayarlarStore.yukle())
</script>

<template>
  <fieldset class="sure-alani">
    <legend class="text-body-medium mb-2">{{ etiket }}</legend>
    <div class="d-flex flex-wrap ga-2 align-center">
      <v-number-input
        :model-value="dakika === null ? null : parcalar.gun"
        label="Gün"
        :required="!bosOlabilir"
        :min="0"
        control-variant="stacked"
        class="sure-parcasi"
        @update:model-value="guncelle('gun', $event)"
      />
      <v-number-input
        :model-value="dakika === null ? null : parcalar.saat"
        label="Saat"
        :required="!bosOlabilir"
        :min="0"
        control-variant="stacked"
        class="sure-parcasi"
        @update:model-value="guncelle('saat', $event)"
      />
      <v-number-input
        :model-value="dakika === null ? null : parcalar.dakika"
        label="Dakika"
        :required="!bosOlabilir"
        :min="0"
        :max="59"
        control-variant="stacked"
        class="sure-parcasi"
        @update:model-value="guncelle('dakika', $event)"
      />
      <v-checkbox
        v-if="eksiOlabilir"
        :model-value="parcalar.eksi"
        label="Eksi (Düşülecek)"
        hide-details
        @update:model-value="guncelle('eksi', $event)"
      />
    </div>
    <p v-if="bosOlabilir" class="text-body-small text-medium-emphasis mt-1">
      Hepsini 0 bırakırsanız sınır yoktur.
    </p>
    <v-messages :messages="hatalar ?? []" :active="(hatalar?.length ?? 0) > 0" color="error" />
  </fieldset>
</template>

<style scoped>
.sure-alani {
  border: 0;
  padding: 0;
  margin: 0;
}

.sure-parcasi {
  flex: 1 1 110px;
  max-width: 160px;
}
</style>
