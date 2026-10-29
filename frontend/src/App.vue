<script setup lang="ts">
import { useToast } from '@/common/composables/useToast'

const { mesajlar } = useToast()
</script>

<template>
  <v-app>
    <RouterView />
    <v-snackbar-queue v-model="mesajlar" closable close-text="Kapat" location="top right" />
  </v-app>
</template>
