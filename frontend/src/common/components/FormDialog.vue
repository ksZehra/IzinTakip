<script setup lang="ts">
import { XIcon } from '@lucide/vue'
import { useFocusReturn } from '@/common/composables/useFocusReturn'

defineProps<{ baslik: string; kaydediliyor: boolean }>()
const emit = defineEmits<{ kaydet: [] }>()
const acik = defineModel<boolean>({ required: true })
const { odagiGeriVer } = useFocusReturn(acik)
</script>

<template>
  <v-dialog v-model="acik" max-width="600" scrollable @after-leave="odagiGeriVer">
    <v-card :title="baslik">
      <template #append>
        <v-btn
          :icon="XIcon"
          variant="text"
          size="small"
          density="comfortable"
          aria-label="Kapat"
          @click="acik = false"
        />
      </template>
      <v-form @submit.prevent="emit('kaydet')">
        <v-card-text class="d-flex flex-column ga-4">
          <slot />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="tonal" @click="acik = false">Vazgeç</v-btn>
          <v-btn type="submit" color="primary" variant="flat" :loading="kaydediliyor">Kaydet</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>
</template>
