<script setup lang="ts">
import ImzaLogo from '@/common/components/ImzaLogo.vue'

const acik = defineModel<boolean>({ required: true })
</script>

<template>
  <v-dialog v-model="acik" max-width="360">
    <v-card class="text-center pa-6" aria-labelledby="hakkinda-baslik">
      <ImzaLogo :boyut="96" class="mx-auto" />
      <h2 id="hakkinda-baslik" class="text-title-large font-weight-semibold mt-4">
        Personel İzin Takip Sistemi
      </h2>
      <v-divider class="my-4" />
      <p class="text-body-medium text-medium-emphasis">Geliştiren</p>
      <p class="text-body-large font-weight-semibold">Zehra Nur Köse</p>
      <a
        href="https://github.com/ksZehra"
        target="_blank"
        rel="noopener noreferrer"
        class="text-body-medium text-primary"
        >github.com/ksZehra</a
      >
      <v-card-actions class="justify-center pb-0 mt-4">
        <v-btn color="primary" @click="acik = false">Kapat</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
