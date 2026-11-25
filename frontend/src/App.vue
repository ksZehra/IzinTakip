<script setup lang="ts">
import { useToast } from '@/common/composables/useToast'

const { mesajlar } = useToast()
</script>

<template>
  <v-app>
    <RouterView />
    <v-snackbar-queue v-model="mesajlar" location="top right">
      <template #actions="{ props }">
        <v-btn
          v-bind="props"
          icon="$close"
          text=""
          size="small"
          variant="text"
          aria-label="Kapat"
        />
      </template>
    </v-snackbar-queue>
  </v-app>
</template>
