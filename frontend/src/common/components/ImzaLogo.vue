<script setup lang="ts">
import { useId } from 'vue'

defineProps<{ boyut: number }>()

const id = useId()
const cubuklar = [
  { x: 27, y: 3, en: 17, boy: 94, renkler: ['#4c8bd0', '#1f5fa8'] },
  { x: 56, y: 3, en: 17, boy: 94, renkler: ['#5ccbc5', '#2aa7a1'] },
  { x: 3, y: 26, en: 94, boy: 17, renkler: ['#e57c9f', '#d4537e'] },
  { x: 3, y: 57, en: 94, boy: 17, renkler: ['#f9c8d6', '#f2a7bf'] },
]
</script>

<template>
  <svg class="imza-logo" :width="boyut" :height="boyut" viewBox="0 0 100 100" aria-hidden="true">
    <defs>
      <linearGradient
        v-for="(cubuk, i) in cubuklar"
        :id="`${id}-${i}`"
        :key="i"
        x1="0"
        y1="0"
        x2="1"
        y2="1"
      >
        <stop offset="0" :stop-color="cubuk.renkler[0]" />
        <stop offset="1" :stop-color="cubuk.renkler[1]" />
      </linearGradient>
    </defs>
    <g transform="rotate(10 50 50)" opacity="0.95">
      <rect
        v-for="(cubuk, i) in cubuklar"
        :key="i"
        :x="cubuk.x"
        :y="cubuk.y"
        :width="cubuk.en"
        :height="cubuk.boy"
        rx="8.5"
        :fill="`url(#${id}-${i})`"
      />
    </g>
    <rect x="19" y="19" width="62" height="62" rx="9" fill="#0f2a4f" />
    <text x="50" y="63" class="imza-logo-harf">ZK</text>
  </svg>
</template>

<style scoped>
.imza-logo {
  flex-shrink: 0;
}

.imza-logo-harf {
  font-family: 'Segoe UI', Arial, sans-serif;
  font-size: 36px;
  font-weight: 700;
  letter-spacing: -1px;
  fill: #fff;
  text-anchor: middle;
}
</style>
