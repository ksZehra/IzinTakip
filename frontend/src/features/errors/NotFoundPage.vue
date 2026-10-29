<template>
  <v-container class="d-flex flex-column align-center ga-4 py-16 text-center">
    <h1 class="text-headline-small">Sayfa bulunamadı</h1>
    <p class="text-body-large">Aradığınız adres yok ya da taşınmış olabilir.</p>
    <v-btn color="primary" :to="{ name: 'ana-sayfa' }">Ana sayfaya dön</v-btn>
  </v-container>
</template>
