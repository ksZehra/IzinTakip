<script setup lang="ts">
import { CalendarIcon, ChevronLeftIcon, ChevronRightIcon } from '@lucide/vue'
import { computed, onMounted, ref, shallowRef, watch } from 'vue'

import { departmanlariGetir, type Departman } from '@/api/departmanlarApi'
import { takvimiGetir, type TakvimKaydi } from '@/api/raporlarApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import DurumChip from '@/common/components/DurumChip.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { sureTipiEtiketleri } from '@/common/utils/etiketler'
import { ayBasligi, gunBasligi } from '@/common/utils/tarih'
import { useAuthStore } from '@/stores/authStore'
import { useAyarlarStore } from '@/stores/ayarlarStore'

import { ayinSinirlari, gunlereDagit } from './takvimGunleri'

const auth = useAuthStore()
const ayarlarStore = useAyarlarStore()
const ekibiGorur = auth.yetkiVar(Yetkiler.IzinTalebiYonet)

const bugun = new Date()
const yil = ref(bugun.getFullYear())
const ay = ref(bugun.getMonth())
const departmanId = ref<number | null>(null)
const departmanlar = shallowRef<Departman[]>([])
const kayitlar = shallowRef<TakvimKaydi[]>([])
const calismaGunleri = shallowRef<string[]>([])
const yukleniyor = ref(false)

const sinirlar = computed(() => ayinSinirlari(yil.value, ay.value))
const gunler = computed(() =>
  gunlereDagit(
    kayitlar.value,
    sinirlar.value.baslangic,
    sinirlar.value.bitis,
    calismaGunleri.value,
  ),
)

function ayDegistir(fark: number) {
  const yeni = new Date(yil.value, ay.value + fark, 1)
  yil.value = yeni.getFullYear()
  ay.value = yeni.getMonth()
}

async function yukle() {
  yukleniyor.value = true
  try {
    kayitlar.value = await takvimiGetir({ ...sinirlar.value, departmanId: departmanId.value })
  } finally {
    yukleniyor.value = false
  }
}

watch([sinirlar, departmanId], yukle)

onMounted(async () => {
  const [ayarlar, liste] = await Promise.all([
    ayarlarStore.yukle(),
    ekibiGorur ? departmanlariGetir() : Promise.resolve([]),
    yukle(),
  ])
  calismaGunleri.value = ayarlar.calismaGunleri
  departmanlar.value = liste.filter((departman) => departman.aktif)
})
</script>

<template>
  <v-container style="max-width: 960px">
    <PageHeader
      baslik="İzin Takvimi"
      aciklama="Beklemedeki ve onaylı izinler, çalışma günlerine göre."
    >
      <v-select
        v-if="ekibiGorur"
        v-model="departmanId"
        :items="departmanlar"
        item-title="ad"
        item-value="id"
        label="Departman"
        clearable
        hide-details
        density="compact"
        style="min-width: 220px"
      />
    </PageHeader>

    <div class="d-flex align-center justify-space-between mb-4">
      <v-btn
        :icon="ChevronLeftIcon"
        variant="text"
        aria-label="Önceki ay"
        @click="ayDegistir(-1)"
      />
      <h2 class="text-title-large" aria-live="polite">{{ ayBasligi(yil, ay) }}</h2>
      <v-btn
        :icon="ChevronRightIcon"
        variant="text"
        aria-label="Sonraki ay"
        @click="ayDegistir(1)"
      />
    </div>

    <v-progress-linear v-if="yukleniyor" indeterminate color="primary" class="mb-2" />

    <div v-if="gunler.length > 0" class="d-flex flex-column ga-3">
      <v-card v-for="gun in gunler" :key="gun.tarih">
        <v-card-title class="text-title-medium">
          <h3 class="text-title-medium">{{ gunBasligi(gun.tarih) }}</h3>
        </v-card-title>
        <v-list density="compact" role="none">
          <v-list-item
            v-for="kayit in gun.kayitlar"
            :key="kayit.izinTalebiId"
            :to="{ name: 'izin-talebi', params: { id: kayit.izinTalebiId } }"
            :title="kayit.calisanAdSoyad"
            :subtitle="`${kayit.departmanAdi} · ${kayit.izinTuruAdi} · ${sureTipiEtiketleri[kayit.sureTipi]}`"
          >
            <template #append><DurumChip :durum="kayit.durum" /></template>
          </v-list-item>
        </v-list>
      </v-card>
    </div>
    <v-empty-state
      v-else-if="!yukleniyor"
      :icon="CalendarIcon"
      title="Bu ay izinli yok"
      text="Seçilen ayda beklemede ya da onaylı izin bulunmuyor."
    />
  </v-container>
</template>
