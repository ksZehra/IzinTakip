<script setup lang="ts">
import { BellIcon, CheckCheckIcon } from '@lucide/vue'
import { computed } from 'vue'
import { useRouter } from 'vue-router'

import {
  bildirimleriGetir,
  okunduIsaretle,
  tumunuOkunduIsaretle,
  type Bildirim,
} from '@/api/bildirimlerApi'
import PageHeader from '@/common/components/PageHeader.vue'
import { usePagedList } from '@/common/composables/usePagedList'
import { tarihSaatGoster } from '@/common/utils/tarih'
import { useBildirimStore } from '@/stores/bildirimStore'

const router = useRouter()
const bildirimSayisi = useBildirimStore()
const { filtre, sayfa, sayfaBoyutu, satirlar, toplam, yukleniyor, yukle } = usePagedList(
  bildirimleriGetir,
  { sadeceOkunmamis: false },
)

const sayfaSayisi = computed(() => Math.max(1, Math.ceil(toplam.value / sayfaBoyutu.value)))

async function ac(bildirim: Bildirim) {
  if (!bildirim.okunduMu) {
    await okunduIsaretle(bildirim.id)
    await bildirimSayisi.sayiyiYenile()
  }
  if (bildirim.baglanti?.startsWith('/')) {
    await router.push(bildirim.baglanti)
  } else {
    await yukle()
  }
}

async function tumunuOku() {
  await tumunuOkunduIsaretle()
  await Promise.all([yukle(), bildirimSayisi.sayiyiYenile()])
}
</script>

<template>
  <v-container>
    <PageHeader baslik="Bildirimler">
      <v-btn
        variant="tonal"
        :prepend-icon="CheckCheckIcon"
        :disabled="bildirimSayisi.okunmamisSayisi === 0"
        @click="tumunuOku"
      >
        Tümünü okundu işaretle
      </v-btn>
    </PageHeader>

    <v-switch
      v-model="filtre.sadeceOkunmamis"
      label="Yalnız Okunmamışlar"
      color="primary"
      hide-details
      class="mb-4"
    />

    <v-card :loading="yukleniyor">
      <v-list v-if="satirlar.length > 0" lines="three">
        <v-list-item
          v-for="bildirim in satirlar"
          :key="bildirim.id"
          :class="{ 'font-weight-bold': !bildirim.okunduMu }"
          :title="bildirim.baslik"
          @click="ac(bildirim)"
        >
          <v-list-item-subtitle class="text-wrap">{{ bildirim.metin }}</v-list-item-subtitle>
          <v-list-item-subtitle class="mt-1">
            {{ tarihSaatGoster(bildirim.olusturmaTarihi) }}
            <span v-if="!bildirim.okunduMu"> · Okunmadı</span>
          </v-list-item-subtitle>
        </v-list-item>
      </v-list>
      <v-empty-state
        v-else-if="!yukleniyor"
        :icon="BellIcon"
        title="Bildirim yok"
        text="Yeni bir gelişme olduğunda burada görünür."
      />
    </v-card>

    <v-pagination
      v-if="sayfaSayisi > 1"
      v-model="sayfa"
      :length="sayfaSayisi"
      class="mt-4"
      aria-label="Bildirim sayfaları"
    />
  </v-container>
</template>
