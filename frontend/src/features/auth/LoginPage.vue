<script setup lang="ts">
import {
  mdiCheck,
  mdiCheckCircleOutline,
  mdiEmailOutline,
  mdiEye,
  mdiEyeOff,
  mdiLockOutline,
} from '@mdi/js'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import ImzaLogo from '@/common/components/ImzaLogo.vue'
import UrunLogo from '@/common/components/UrunLogo.vue'
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
      <section class="login-marka d-none d-md-flex">
        <div class="d-flex align-center ga-3 text-title-medium font-weight-semibold">
          <UrunLogo :boyut="40" />
          İzinTakip
        </div>

        <div class="login-kartlar" aria-hidden="true">
          <div class="login-kart login-kart-bakiye">
            <span class="text-body-small text-medium-emphasis">Kalan yıllık izin</span>
            <span class="d-block text-title-large font-weight-bold text-primary"
              >12 gün 4 saat</span
            >
            <span class="login-kart-cubuk"><span /></span>
          </div>
          <div class="login-kart login-kart-onay">
            <span class="login-kart-onay-ikon"><v-icon :icon="mdiCheck" size="18" /></span>
            <span>
              <span class="d-block text-body-medium font-weight-semibold">Talebiniz onaylandı</span>
              <span class="text-body-small text-medium-emphasis">3–5 Mart · Yıllık izin</span>
            </span>
          </div>
        </div>

        <div>
          <p class="text-display-small font-weight-bold login-baslik">
            Personel İzin<br />Takip Sistemi
          </p>
          <p class="text-title-medium login-alt-baslik mt-3 mb-8">İzin süreçleriniz tek ekranda.</p>
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
            <div
              class="d-flex d-md-none align-center ga-3 text-title-large font-weight-semibold text-primary mb-10"
            >
              <UrunLogo :boyut="44" />
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
  color: #fff;
  background:
    radial-gradient(circle at 85% 15%, rgb(66 165 245 / 0.6), transparent 45%),
    radial-gradient(circle at 10% 95%, rgb(38 198 218 / 0.3), transparent 40%),
    linear-gradient(150deg, #0b2e6b 0%, #1565c0 65%, #1e88e5 100%);
}

.login-baslik {
  line-height: 1.15;
  letter-spacing: -0.5px;
}

.login-alt-baslik {
  opacity: 0.9;
}

.login-kartlar {
  position: relative;
  flex: 1;
  min-height: 200px;
}

.login-kart {
  position: absolute;
  padding: 14px 16px;
  border-radius: 14px;
  background: #fff;
  color: rgb(var(--v-theme-on-surface));
  box-shadow: 0 16px 40px rgb(8 30 70 / 0.35);
}

.login-kart-bakiye {
  top: 18%;
  right: 0;
  width: 220px;
}

.login-kart-cubuk {
  display: block;
  height: 6px;
  margin-top: 10px;
  border-radius: 3px;
  background: #e3eefb;
}

.login-kart-cubuk span {
  display: block;
  width: 62%;
  height: 100%;
  border-radius: 3px;
  background: rgb(var(--v-theme-primary));
}

.login-kart-onay {
  top: 52%;
  right: 18%;
  display: flex;
  align-items: center;
  gap: 12px;
}

.login-kart-onay-ikon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: #e8f5e9;
  color: rgb(var(--v-theme-success));
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
