<template>
  <v-container class="d-flex flex-column align-center ga-4 py-16 text-center">
    <h1 class="text-headline-small">Yetkiniz yok</h1>
    <p class="text-body-large">Bu sayfayı açmak için gereken yetkiye sahip değilsiniz.</p>
    <v-btn color="primary" :to="{ name: 'ana-sayfa' }">Ana sayfaya dön</v-btn>
  </v-container>
</template>
