<script setup lang="ts">
defineProps<{ baslik: string; aciklama?: string }>()
</script>

<template>
  <header class="d-flex flex-wrap align-center ga-3 mb-6">
    <div class="flex-grow-1">
      <h1 class="text-headline-small">{{ baslik }}</h1>
      <p v-if="aciklama" class="text-body-medium text-medium-emphasis mt-1">{{ aciklama }}</p>
    </div>
    <div class="d-flex flex-wrap ga-2">
      <slot />
    </div>
  </header>
</template>
