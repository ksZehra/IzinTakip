<script setup lang="ts">
defineProps<{ baslik: string; kaydediliyor: boolean }>()
const emit = defineEmits<{ kaydet: [] }>()
const acik = defineModel<boolean>({ required: true })
</script>

<template>
  <v-dialog v-model="acik" max-width="600" scrollable>
    <v-card :title="baslik">
      <v-form @submit.prevent="emit('kaydet')">
        <v-card-text class="d-flex flex-column ga-4">
          <slot />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="acik = false">Vazgeç</v-btn>
          <v-btn type="submit" color="primary" variant="flat" :loading="kaydediliyor">Kaydet</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>
</template>
