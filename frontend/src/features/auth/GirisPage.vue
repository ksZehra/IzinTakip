<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { useFormGonder } from '@/common/composables/useFormGonder'
import CenteredLayout from '@/layouts/CenteredLayout.vue'
import { guvenliDonusAdresi } from '@/router/guards'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const { gonder, gonderiliyor, hataMesaji, alanHatasi } = useFormGonder()

const eposta = ref('')
const parola = ref('')

async function girisYap() {
  const basarili = await gonder(async () => {
    await auth.girisYap(eposta.value.trim(), parola.value)
    return true
  })
  if (basarili) {
    await router.replace(guvenliDonusAdresi(route.query.donus, router))
  }
}
</script>

<template>
  <CenteredLayout>
    <v-card>
      <v-card-item>
        <h1 class="text-headline-small">Giriş</h1>
      </v-card-item>
      <v-card-text>
        <v-form class="d-flex flex-column ga-4" @submit.prevent="girisYap">
          <v-alert v-if="hataMesaji" type="error" variant="tonal" :text="hataMesaji" />
          <v-text-field
            v-model="eposta"
            label="E-posta"
            type="email"
            autocomplete="username"
            :error-messages="alanHatasi('eposta')"
          />
          <v-text-field
            v-model="parola"
            label="Parola"
            type="password"
            autocomplete="current-password"
            :error-messages="alanHatasi('parola')"
          />
          <v-btn type="submit" color="primary" size="large" :loading="gonderiliyor" block>
            Giriş yap
          </v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </CenteredLayout>
</template>
