<script setup lang="ts">
import { mdiCheckCircleOutline, mdiEmailOutline, mdiEye, mdiEyeOff, mdiLockOutline } from '@mdi/js'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import HakkindaDialog from '@/common/components/HakkindaDialog.vue'
import ImzaLogo from '@/common/components/ImzaLogo.vue'
import UrunLogo from '@/common/components/UrunLogo.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { TELIF_YILI } from '@/common/utils/uygulamaBilgisi'
import { guvenliDonusAdresi } from '@/router/guards'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const { gonder, gonderiliyor, hataMesaji, alanHatasi } = useFormSubmit()

const eposta = ref('')
const parola = ref('')
const parolaGorunur = ref(false)
const hakkindaAcik = ref(false)

const ozellikler = [
  'Talep açın, onaya gönderin',
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
      <section class="login-marka d-none d-md-flex">
        <svg class="login-desen" viewBox="0 0 280 200" aria-hidden="true" fill="none">
          <g stroke="#fff" stroke-opacity="0.08" stroke-width="1">
            <path d="M40 0v200M80 0v200M120 0v200M160 0v200M200 0v200M240 0v200" />
          </g>
          <rect x="30" y="30" width="110" height="22" rx="11" fill="#4c8bd0" fill-opacity="0.32" />
          <rect x="110" y="68" width="90" height="22" rx="11" fill="#5ccbc5" fill-opacity="0.3" />
          <rect x="60" y="106" width="70" height="22" rx="11" fill="#e57c9f" fill-opacity="0.3" />
          <rect x="150" y="144" width="110" height="22" rx="11" fill="#f9c8d6" fill-opacity="0.2" />
        </svg>

        <div class="login-marka-ust d-flex align-center ga-3 text-title-large font-weight-semibold">
          <UrunLogo :boyut="44" />
          Personel İzin Takip Sistemi
        </div>

        <div class="login-marka-orta">
          <p class="text-headline-medium font-weight-semibold login-baslik mb-6">
            İzin Süreçleriniz Tek Ekranda
          </p>
          <ul class="login-ozellikler">
            <li
              v-for="ozellik in ozellikler"
              :key="ozellik"
              class="d-flex align-center ga-3 text-body-large"
            >
              <v-icon :icon="mdiCheckCircleOutline" size="22" />
              {{ ozellik }}
            </li>
          </ul>
        </div>
      </section>

      <section class="login-form">
        <div class="login-form-govde">
          <div class="w-100" style="max-width: 380px">
            <div
              class="d-flex d-md-none align-center ga-3 text-title-large font-weight-semibold text-primary mb-10"
            >
              <UrunLogo :boyut="40" />
              Personel İzin Takip Sistemi
            </div>

            <h1 class="text-headline-small font-weight-semibold">Hoş geldiniz</h1>
            <p class="text-body-medium text-medium-emphasis mt-1 mb-8">
              Kurumsal e-posta adresinizle giriş yapın.
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
              Parolanızı unuttuysanız İK birimiyle görüşün.
            </p>
          </div>
        </div>

        <button
          type="button"
          class="login-imza"
          aria-haspopup="dialog"
          @click="hakkindaAcik = true"
        >
          <ImzaLogo :boyut="32" />
          <span class="text-body-small text-medium-emphasis">
            © {{ TELIF_YILI }}
            <span class="text-high-emphasis font-weight-semibold">Zehra Nur Köse</span>. Tüm hakları
            saklıdır.
          </span>
        </button>
      </section>
    </div>

    <HakkindaDialog v-model="hakkindaAcik" />
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
  justify-content: center;
  padding: 40px 48px;
  position: relative;
  overflow: hidden;
  color: #fff;
  background:
    radial-gradient(circle at 100% 100%, rgb(42 167 161 / 0.5), transparent 55%),
    linear-gradient(160deg, #0f2a4f 0%, #1f5fa8 100%);
}

.login-desen {
  position: absolute;
  right: -10%;
  bottom: -6%;
  width: 88%;
  height: auto;
}

.login-marka-orta {
  position: relative;
}

.login-marka-ust {
  position: absolute;
  top: 40px;
  left: 48px;
}

.login-baslik {
  line-height: 1.25;
}

.login-ozellikler {
  display: flex;
  flex-direction: column;
  gap: 14px;
  padding: 0;
  list-style: none;
}

.login-ozellikler {
  color: #b5d4f4;
}

.login-form {
  position: relative;
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
  border: 0;
  background: none;
  color: inherit;
  font: inherit;
  text-align: center;
  cursor: pointer;
}

.login-imza:hover {
  opacity: 0.75;
}

@media (min-width: 960px) {
  .login-imza {
    position: absolute;
    right: 0;
    bottom: 0;
    left: 0;
  }
}
</style>
