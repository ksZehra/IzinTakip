<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

import { parolaDegistir } from '@/api/authApi'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import CenteredLayout from '@/layouts/CenteredLayout.vue'
import { useToast } from '@/common/composables/useToast'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const toast = useToast()
const { gonder, gonderiliyor, alanHatasi } = useFormSubmit()

const mevcutParola = ref('')
const yeniParola = ref('')
const yeniParolaTekrar = ref('')

const zorunlu = computed(() => auth.oturum?.parolaDegistirmeli ?? false)
const tekrarHatasi = computed(() =>
  yeniParolaTekrar.value && yeniParolaTekrar.value !== yeniParola.value
    ? ['Parolalar aynı değil.']
    : [],
)

async function kaydet() {
  if (tekrarHatasi.value.length > 0) {
    return
  }
  const basarili = await gonder(async () => {
    await parolaDegistir({ mevcutParola: mevcutParola.value, yeniParola: yeniParola.value })
    return true
  })
  if (basarili) {
    await auth.cikisYap()
    toast.basariGoster('Parolanız değişti. Yeni parolanızla giriş yapın.')
    await router.replace({ name: 'login' })
  }
}

async function cikisYap() {
  await auth.cikisYap()
  await router.replace({ name: 'login' })
}
</script>

<template>
  <CenteredLayout>
    <v-card>
      <v-card-item>
        <h1 class="text-headline-small">Parola Değiştir</h1>
      </v-card-item>
      <v-card-text class="d-flex flex-column ga-4">
        <v-alert v-if="zorunlu" type="info" variant="tonal">
          Devam etmek için önce geçici parolanızı değiştirin.
        </v-alert>
        <v-form class="d-flex flex-column ga-4" @submit.prevent="kaydet">
          <v-text-field
            v-model="mevcutParola"
            label="Mevcut Parola"
            required
            type="password"
            autocomplete="current-password"
            :error-messages="alanHatasi('mevcutParola')"
          />
          <v-text-field
            v-model="yeniParola"
            label="Yeni Parola"
            required
            type="password"
            autocomplete="new-password"
            hint="En az 8 karakter; büyük harf, küçük harf ve rakam."
            persistent-hint
            :error-messages="alanHatasi('yeniParola')"
          />
          <v-text-field
            v-model="yeniParolaTekrar"
            label="Yeni Parola (Tekrar)"
            required
            type="password"
            autocomplete="new-password"
            :error-messages="tekrarHatasi"
          />
          <v-btn type="submit" color="primary" size="large" :loading="gonderiliyor" block>
            Parolayı değiştir
          </v-btn>
        </v-form>
        <v-btn v-if="zorunlu" variant="text" @click="cikisYap">Çıkış yap</v-btn>
        <v-btn v-else variant="tonal" :to="{ name: 'ana-sayfa' }">Vazgeç</v-btn>
      </v-card-text>
    </v-card>
  </CenteredLayout>
</template>
