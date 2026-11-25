<script setup lang="ts">
import { Trash2Icon, DownloadIcon, FileTextIcon, UploadIcon } from '@lucide/vue'
import { onMounted, ref, shallowRef } from 'vue'

import {
  belgeIndir,
  belgeleriGetir,
  belgeSil,
  belgeYukle,
  type IzinBelgesi,
} from '@/api/belgelerApi'
import ConfirmDialog from '@/common/components/ConfirmDialog.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useToast } from '@/common/composables/useToast'

const props = defineProps<{ talepId: number; degistirilebilir: boolean }>()
const emit = defineEmits<{ degisti: [] }>()

const toast = useToast()
const { gonder, gonderiliyor, alanHatasi } = useFormSubmit()
const belgeler = shallowRef<IzinBelgesi[]>([])
const secilenDosya = ref<File | null>(null)
const silinecek = ref<IzinBelgesi | null>(null)
const silmeAcik = ref(false)
const silme = useFormSubmit()

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

function silmeyiSor(belge: IzinBelgesi) {
  silinecek.value = belge
  silmeAcik.value = true
}

async function sil() {
  const belge = silinecek.value
  if (!belge) return
  const basarili = await silme.gonder(async () => {
    await belgeSil(props.talepId, belge.id)
    return true
  })
  if (!basarili) return
  silmeAcik.value = false
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
          :prepend-icon="FileTextIcon"
          :title="belge.dosyaAdi"
          :subtitle="boyutMetni(belge.boyut)"
        >
          <template #append>
            <v-btn
              :icon="DownloadIcon"
              variant="text"
              size="small"
              :aria-label="`${belge.dosyaAdi} dosyasını indir`"
              @click="belgeIndir(talepId, belge)"
            />
            <v-btn
              v-if="degistirilebilir"
              :icon="Trash2Icon"
              variant="text"
              size="small"
              color="error"
              :aria-label="`${belge.dosyaAdi} dosyasını sil`"
              @click="silmeyiSor(belge)"
            />
          </template>
        </v-list-item>
      </v-list>
      <p v-else class="text-medium-emphasis">Bu talebe belge eklenmemiş.</p>

      <v-form v-if="degistirilebilir" class="d-flex flex-wrap ga-2" @submit.prevent="dosyayiYukle">
        <v-file-input
          v-model="secilenDosya"
          label="Belge (PDF, JPG, PNG; En Çok 5 MB)"
          accept=".pdf,.jpg,.jpeg,.png"
          prepend-icon=""
          class="flex-1-1"
          :error-messages="alanHatasi('dosya')"
        />
        <v-btn
          type="submit"
          :prepend-icon="UploadIcon"
          :disabled="!secilenDosya"
          :loading="gonderiliyor"
          class="align-self-start mt-1"
        >
          Yükle
        </v-btn>
      </v-form>
    </v-card-text>

    <ConfirmDialog
      v-model="silmeAcik"
      baslik="Belgeyi Sil"
      :metin="`${silinecek?.dosyaAdi ?? ''} kalıcı olarak silinecek.`"
      onay-metni="Sil"
      onay-rengi="error"
      :calisiyor="silme.gonderiliyor.value"
      @onay="sil"
    />
  </v-card>
</template>
