<script setup lang="ts">
import { CircleUserIcon, InfoIcon, KeyRoundIcon, LogOutIcon } from '@lucide/vue'
import { useRouter } from 'vue-router'

import { useAuthStore } from '@/stores/authStore'

const emit = defineEmits<{ hakkinda: [] }>()
const auth = useAuthStore()
const router = useRouter()

async function cikisYap() {
  await auth.cikisYap()
  await router.replace({ name: 'login' })
}
</script>

<template>
  <v-menu location="bottom end">
    <template #activator="{ props }">
      <v-btn
        v-bind="props"
        :prepend-icon="CircleUserIcon"
        :aria-label="`Kullanıcı menüsü: ${auth.oturum?.gorunenAd ?? ''}`"
      >
        <span class="d-none d-sm-inline">{{ auth.oturum?.gorunenAd }}</span>
      </v-btn>
    </template>
    <v-list density="compact" role="none">
      <v-list-item :subtitle="auth.oturum?.eposta" :title="auth.oturum?.gorunenAd" />
      <v-divider />
      <v-list-item
        :prepend-icon="KeyRoundIcon"
        title="Parola değiştir"
        :to="{ name: 'parola-degistir' }"
      />
      <v-list-item :prepend-icon="InfoIcon" title="Hakkında" @click="emit('hakkinda')" />
      <v-list-item :prepend-icon="LogOutIcon" title="Çıkış yap" @click="cikisYap" />
    </v-list>
  </v-menu>
</template>
