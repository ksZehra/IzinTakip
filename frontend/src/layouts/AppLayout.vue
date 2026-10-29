<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useDisplay } from 'vuetify'

import { menuOgeleri } from '@/router/menu'
import { routes } from '@/router/routes'
import { useAuthStore } from '@/stores/authStore'
import { useBildirimStore } from '@/stores/bildirimStore'

import NotificationBell from './NotificationBell.vue'
import UserMenu from './UserMenu.vue'

const auth = useAuthStore()
const bildirimler = useBildirimStore()
const route = useRoute()
const { mobile } = useDisplay()

const menuAcik = ref(!mobile.value)
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

  <v-app-bar color="primary" density="comfortable">
    <v-app-bar-nav-icon
      :aria-label="menuAcik ? 'Menüyü kapat' : 'Menüyü aç'"
      :aria-expanded="menuAcik"
      @click="menuAcik = !menuAcik"
    />
    <v-app-bar-title>IzinTakip</v-app-bar-title>
    <NotificationBell />
    <UserMenu />
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
        />
        <template v-if="yonetimOgeleri.length > 0">
          <v-list-subheader>Yönetim</v-list-subheader>
          <v-list-item
            v-for="oge in yonetimOgeleri"
            :key="oge.rotaAdi"
            :to="{ name: oge.rotaAdi }"
            :prepend-icon="oge.ikon"
            :title="oge.baslik"
          />
        </template>
      </v-list>
    </nav>
  </v-navigation-drawer>

  <v-main id="icerik">
    <RouterView />
  </v-main>
</template>
