<script setup lang="ts">
import { mdiAccountCircle, mdiKeyVariant, mdiLogout } from '@mdi/js'
import { useRouter } from 'vue-router'

import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()

async function cikisYap() {
  await auth.cikisYap()
  await router.replace({ name: 'giris' })
}
</script>

<template>
  <v-menu location="bottom end">
    <template #activator="{ props }">
      <v-btn
        v-bind="props"
        :prepend-icon="mdiAccountCircle"
        :aria-label="`Kullanıcı menüsü: ${auth.oturum?.gorunenAd ?? ''}`"
      >
        <span class="d-none d-sm-inline">{{ auth.oturum?.gorunenAd }}</span>
      </v-btn>
    </template>
    <v-list density="compact" role="none">
      <v-list-item :subtitle="auth.oturum?.eposta" :title="auth.oturum?.gorunenAd" />
      <v-divider />
      <v-list-item
        :prepend-icon="mdiKeyVariant"
        title="Parola değiştir"
        :to="{ name: 'parola-degistir' }"
      />
      <v-list-item :prepend-icon="mdiLogout" title="Çıkış yap" @click="cikisYap" />
    </v-list>
  </v-menu>
</template>
