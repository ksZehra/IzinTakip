<script setup lang="ts">
import { BellIcon } from '@lucide/vue'
import { computed } from 'vue'

import { useBildirimStore } from '@/stores/bildirimStore'

const bildirimler = useBildirimStore()

const etiket = computed(() =>
  bildirimler.okunmamisSayisi > 0
    ? `Bildirimler, ${bildirimler.okunmamisSayisi} okunmamış`
    : 'Bildirimler',
)
</script>

<template>
  <v-btn icon :to="{ name: 'bildirimler' }" :aria-label="etiket">
    <v-badge
      :model-value="bildirimler.okunmamisSayisi > 0"
      :content="bildirimler.okunmamisSayisi"
      color="error"
      max="99"
    >
      <v-icon :icon="BellIcon" />
    </v-badge>
  </v-btn>
</template>
