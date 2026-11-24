<script setup lang="ts">
import { useId } from 'vue'

defineProps<{ boyut: number }>()

const id = useId()
</script>

<template>
  <svg class="urun-logo" :width="boyut" :height="boyut" viewBox="0 0 48 48" aria-hidden="true">
    <defs>
      <linearGradient :id="id" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0" stop-color="#42a5f5" />
        <stop offset="1" stop-color="#1565c0" />
      </linearGradient>
    </defs>
    <rect width="48" height="48" rx="13" :fill="`url(#${id})`" />
    <rect x="11" y="13" width="26" height="23" rx="5" fill="#fff" />
    <rect x="11" y="13" width="26" height="7" rx="3" fill="#bbdefb" />
    <rect x="16" y="9.5" width="3" height="7" rx="1.5" fill="#fff" />
    <rect x="29" y="9.5" width="3" height="7" rx="1.5" fill="#fff" />
    <path
      d="M18 28 L22.5 32 L30.5 24"
      fill="none"
      stroke="#1565c0"
      stroke-width="3"
      stroke-linecap="round"
      stroke-linejoin="round"
    />
  </svg>
</template>

<style scoped>
.urun-logo {
  flex-shrink: 0;
}
</style>
