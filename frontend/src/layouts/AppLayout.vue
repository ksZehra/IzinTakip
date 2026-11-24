<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useDisplay } from 'vuetify'

import HakkindaDialog from '@/common/components/HakkindaDialog.vue'
import ImzaLogo from '@/common/components/ImzaLogo.vue'
import UrunLogo from '@/common/components/UrunLogo.vue'
import { TELIF_YILI } from '@/common/utils/uygulamaBilgisi'
import { menuOgeleri } from '@/router/menu'
import { routes } from '@/router/routes'
import { useAuthStore } from '@/stores/authStore'
import { useBildirimStore } from '@/stores/bildirimStore'

import NotificationBell from './NotificationBell.vue'
import TemaMenu from '@/common/components/TemaMenu.vue'
import UserMenu from './UserMenu.vue'

const auth = useAuthStore()
const bildirimler = useBildirimStore()
const route = useRoute()
const { mobile } = useDisplay()

const menuAcik = ref(!mobile.value)
const hakkindaAcik = ref(false)
const ogeler = computed(() => menuOgeleri(routes, auth.yetkiVar))
const anaOgeler = computed(() => ogeler.value.filter((oge) => !oge.grup))
const yonetimOgeleri = computed(() => ogeler.value.filter((oge) => oge.grup === 'Yönetim'))

watch(mobile, (mobilMi) => (menuAcik.value = !mobilMi))

watch(
  () => route.path,
  async () => {
    if (mobile.value) {
      menuAcik.value = false
    }
    await nextTick()
    const baslik = document.querySelector<HTMLElement>('#icerik h1')
    baslik?.setAttribute('tabindex', '-1')
    baslik?.focus({ preventScroll: true })
  },
)

onMounted(() => bildirimler.izlemeyiBaslat())
onUnmounted(() => bildirimler.izlemeyiDurdur())
</script>

<template>
  <a class="atlama-baglantisi" href="#icerik">İçeriğe geç</a>

  <v-app-bar density="comfortable">
    <v-app-bar-nav-icon
      :aria-label="menuAcik ? 'Menüyü kapat' : 'Menüyü aç'"
      :aria-expanded="menuAcik"
      @click="menuAcik = !menuAcik"
    />
    <UrunLogo :boyut="32" class="ms-1" />
    <v-app-bar-title class="font-weight-semibold">Personel İzin Takip Sistemi</v-app-bar-title>
    <TemaMenu />
    <NotificationBell />
    <UserMenu @hakkinda="hakkindaAcik = true" />
  </v-app-bar>

  <v-navigation-drawer v-model="menuAcik" :permanent="!mobile" :temporary="mobile">
    <nav aria-label="Ana menü">
      <v-list nav density="comfortable" role="none">
        <v-list-item
          v-for="oge in anaOgeler"
          :key="oge.rotaAdi"
          :to="{ name: oge.rotaAdi }"
          :prepend-icon="oge.ikon"
          :title="oge.baslik"
          :exact="oge.rotaAdi === 'ana-sayfa'"
          color="primary"
        />
        <template v-if="yonetimOgeleri.length > 0">
          <v-list-subheader>Yönetim</v-list-subheader>
          <v-list-item
            v-for="oge in yonetimOgeleri"
            :key="oge.rotaAdi"
            :to="{ name: oge.rotaAdi }"
            :prepend-icon="oge.ikon"
            :title="oge.baslik"
            color="primary"
          />
        </template>
      </v-list>
    </nav>

    <template #append>
      <button type="button" class="menu-imza" aria-haspopup="dialog" @click="hakkindaAcik = true">
        <ImzaLogo :boyut="22" />
        <span class="text-body-small text-medium-emphasis">
          © {{ TELIF_YILI }}
          <span class="text-high-emphasis font-weight-medium">Zehra Nur Köse</span>
        </span>
      </button>
    </template>
  </v-navigation-drawer>

  <HakkindaDialog v-model="hakkindaAcik" />

  <v-main id="icerik">
    <RouterView />
  </v-main>
</template>

<style scoped>
.menu-imza {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  padding: 14px 16px;
  border: 0;
  border-top: thin solid rgba(var(--v-border-color), var(--v-border-opacity));
  background: transparent;
  text-align: left;
  cursor: pointer;
  transition: background-color 0.15s;
}

.menu-imza:hover,
.menu-imza:focus-visible {
  background: rgba(
    var(--v-theme-on-surface),
    calc(var(--v-hover-opacity) * var(--v-high-emphasis-opacity))
  );
}
</style>
