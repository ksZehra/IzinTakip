<script setup lang="ts">
import {
  mdiCalendarCheck,
  mdiCheckCircleOutline,
  mdiEmailOutline,
  mdiEye,
  mdiEyeOff,
  mdiLockOutline,
} from '@mdi/js'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import ImzaLogo from '@/common/components/ImzaLogo.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { guvenliDonusAdresi } from '@/router/guards'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const { gonder, gonderiliyor, hataMesaji, alanHatasi } = useFormSubmit()

const eposta = ref('')
const parola = ref('')
const parolaGorunur = ref(false)

const ozellikler = [
  'Talebinizi açın, onaya kendiliğinden gitsin',
  'Bakiyenizi her an görün',
  'Ekibinizin izin takvimini izleyin',
]

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
  <v-main>
    <div class="login">
      <section class="login-marka bg-primary d-none d-md-flex">
        <div class="d-flex align-center ga-3 text-title-medium">
          <span class="login-logo"><v-icon :icon="mdiCalendarCheck" size="22" /></span>
          Personel İzin Takip Sistemi
        </div>

        <div class="my-auto">
          <p class="text-headline-small font-weight-semibold mb-6">İzin Süreçleriniz Tek Ekranda</p>
          <ul class="login-ozellikler">
            <li
              v-for="ozellik in ozellikler"
              :key="ozellik"
              class="d-flex align-center ga-3 text-body-large"
            >
              <v-icon :icon="mdiCheckCircleOutline" size="20" />
              {{ ozellik }}
            </li>
          </ul>
        </div>
      </section>

      <section class="login-form">
        <div class="login-form-govde">
          <div class="w-100" style="max-width: 380px">
            <div class="d-flex d-md-none align-center ga-3 text-title-medium text-primary mb-10">
              <span class="login-logo bg-primary"
                ><v-icon :icon="mdiCalendarCheck" size="22"
              /></span>
              Personel İzin Takip Sistemi
            </div>

            <h1 class="text-headline-small font-weight-semibold">Giriş yap</h1>
            <p class="text-body-medium text-medium-emphasis mt-1 mb-8">
              Kurumsal e-posta adresinizle devam edin.
            </p>

            <v-form class="d-flex flex-column ga-4" @submit.prevent="girisYap">
              <v-alert v-if="hataMesaji" type="error" variant="tonal" :text="hataMesaji" />

              <v-text-field
                v-model="eposta"
                label="E-posta"
                type="email"
                autocomplete="username"
                :prepend-inner-icon="mdiEmailOutline"
                :error-messages="alanHatasi('eposta')"
              />
              <v-text-field
                v-model="parola"
                label="Parola"
                :type="parolaGorunur ? 'text' : 'password'"
                autocomplete="current-password"
                :prepend-inner-icon="mdiLockOutline"
                :error-messages="alanHatasi('parola')"
              >
                <template #append-inner>
                  <v-btn
                    :icon="parolaGorunur ? mdiEyeOff : mdiEye"
                    :aria-label="parolaGorunur ? 'Parolayı gizle' : 'Parolayı göster'"
                    :aria-pressed="parolaGorunur"
                    variant="text"
                    size="small"
                    density="comfortable"
                    @click="parolaGorunur = !parolaGorunur"
                  />
                </template>
              </v-text-field>

              <v-btn
                type="submit"
                color="primary"
                size="large"
                :loading="gonderiliyor"
                block
                class="mt-2"
                >Giriş yap</v-btn
              >
            </v-form>

            <p class="text-body-small text-medium-emphasis text-center mt-6">
              Parolanızı unuttuysanız İK birimiyle görüşünüz.
            </p>
          </div>
        </div>

        <footer class="login-imza">
          <ImzaLogo :boyut="32" />
          <p class="text-body-small text-medium-emphasis">
            © 2026 <span class="text-high-emphasis font-weight-semibold">Zehra Nur Köse</span>. Tüm
            hakları saklıdır.
          </p>
        </footer>
      </section>
    </div>
  </v-main>
</template>

<style scoped>
.login {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  min-height: 100vh;
}

@media (min-width: 960px) {
  .login {
    grid-template-columns: minmax(0, 5fr) minmax(0, 6fr);
  }
}

.login-marka {
  flex-direction: column;
  padding: 48px 56px;
}

.login-logo {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 10px;
  background: rgb(255 255 255 / 0.16);
}

.login-logo.bg-primary {
  background: rgb(var(--v-theme-primary));
}

.login-ozellikler {
  display: flex;
  flex-direction: column;
  gap: 16px;
  list-style: none;
  padding: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
}

.login-form-govde {
  display: flex;
  flex: 1;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
}

.login-imza {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0 24px 24px;
  text-align: center;
}
</style>
