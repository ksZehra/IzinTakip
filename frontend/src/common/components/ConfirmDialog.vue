<script setup lang="ts">
import { computed, ref, watch } from 'vue'

const props = defineProps<{
  baslik: string
  metin: string
  onayMetni: string
  onayRengi?: string
  notEtiketi?: string
  notZorunlu?: boolean
  calisiyor?: boolean
}>()

const emit = defineEmits<{ onay: [not: string | null] }>()
const acik = defineModel<boolean>({ required: true })
const not = ref('')

const onaylanabilir = computed(() => !props.notZorunlu || not.value.trim().length > 0)

watch(acik, (acildi) => {
  if (acildi) not.value = ''
})

function onayla() {
  if (onaylanabilir.value) emit('onay', not.value.trim() || null)
}
</script>

<template>
  <v-dialog v-model="acik" max-width="480">
    <v-card :title="baslik">
      <v-card-text class="d-flex flex-column ga-4">
        <p>{{ metin }}</p>
        <v-textarea
          v-if="notEtiketi"
          v-model="not"
          :label="notZorunlu ? `${notEtiketi} (zorunlu)` : notEtiketi"
          rows="3"
          auto-grow
          counter="500"
          maxlength="500"
        />
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="acik = false">Vazgeç</v-btn>
        <v-btn
          :color="onayRengi ?? 'primary'"
          variant="flat"
          :disabled="!onaylanabilir"
          :loading="calisiyor"
          @click="onayla"
        >
          {{ onayMetni }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
