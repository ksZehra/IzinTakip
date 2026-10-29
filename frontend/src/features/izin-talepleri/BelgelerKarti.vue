<script setup lang="ts">
import { mdiDelete, mdiDownload, mdiFileDocumentOutline, mdiUpload } from '@mdi/js'
import { onMounted, ref, shallowRef } from 'vue'

import {
  belgeIndir,
  belgeleriGetir,
  belgeSil,
  belgeYukle,
  type IzinBelgesi,
} from '@/api/belgelerApi'
import { useFormGonder } from '@/common/composables/useFormGonder'
import { useToast } from '@/common/composables/useToast'

const props = defineProps<{ talepId: number; degistirilebilir: boolean }>()
const emit = defineEmits<{ degisti: [] }>()

const toast = useToast()
const { gonder, gonderiliyor, alanHatasi } = useFormGonder()
const belgeler = shallowRef<IzinBelgesi[]>([])
const secilenDosya = ref<File | null>(null)

async function yukle() {
  belgeler.value = await belgeleriGetir(props.talepId)
}

async function dosyayiYukle() {
  const dosya = secilenDosya.value
  if (!dosya) return
  const basarili = await gonder(async () => {
    await belgeYukle(props.talepId, dosya)
    return true
  })
  if (!basarili) return
  secilenDosya.value = null
  toast.basariGoster('Belge yüklendi.')
  await yukle()
  emit('degisti')
}

async function sil(belge: IzinBelgesi) {
  await belgeSil(props.talepId, belge.id)
  toast.basariGoster('Belge silindi.')
  await yukle()
  emit('degisti')
}

const boyutMetni = (bayt: number) =>
  bayt >= 1024 * 1024 ? `${(bayt / 1024 / 1024).toFixed(1)} MB` : `${Math.ceil(bayt / 1024)} KB`

onMounted(yukle)
</script>

<template>
  <v-card title="Belgeler">
    <v-card-text class="d-flex flex-column ga-4">
      <v-list v-if="belgeler.length > 0" density="compact">
        <v-list-item
          v-for="belge in belgeler"
          :key="belge.id"
          :prepend-icon="mdiFileDocumentOutline"
          :title="belge.dosyaAdi"
          :subtitle="boyutMetni(belge.boyut)"
        >
          <template #append>
            <v-btn
              :icon="mdiDownload"
              variant="text"
              size="small"
              :aria-label="`${belge.dosyaAdi} dosyasını indir`"
              @click="belgeIndir(talepId, belge)"
            />
            <v-btn
              v-if="degistirilebilir"
              :icon="mdiDelete"
              variant="text"
              size="small"
              color="error"
              :aria-label="`${belge.dosyaAdi} dosyasını sil`"
              @click="sil(belge)"
            />
          </template>
        </v-list-item>
      </v-list>
      <p v-else class="text-medium-emphasis">Bu talebe belge eklenmemiş.</p>

      <v-form v-if="degistirilebilir" class="d-flex flex-wrap ga-2" @submit.prevent="dosyayiYukle">
        <v-file-input
          v-model="secilenDosya"
          label="Belge (PDF, JPG, PNG; en çok 5 MB)"
          accept=".pdf,.jpg,.jpeg,.png"
          prepend-icon=""
          class="flex-1-1"
          :error-messages="alanHatasi('dosya')"
        />
        <v-btn
          type="submit"
          :prepend-icon="mdiUpload"
          :disabled="!secilenDosya"
          :loading="gonderiliyor"
          class="align-self-start mt-1"
        >
          Yükle
        </v-btn>
      </v-form>
    </v-card-text>
  </v-card>
</template>
