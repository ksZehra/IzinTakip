<script setup lang="ts">
import { FileSpreadsheetIcon, PlusIcon } from '@lucide/vue'
import { computed, onMounted, shallowRef } from 'vue'
import { useRouter } from 'vue-router'

import {
  talepleriExceleAktar,
  talepleriGetir,
  type IzinTalebiSatiri,
  type TalepFilter,
} from '@/api/izinTalepleriApi'
import { izinTurleriniGetir, type IzinTuru } from '@/api/izinTurleriApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import DataGrid from '@/common/components/DataGrid.vue'
import type { GridKolon } from '@/common/components/dataGridTipleri'
import DurumChip from '@/common/components/DurumChip.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { usePagedList } from '@/common/composables/usePagedList'
import { durumEtiketleri, secenekler } from '@/common/utils/etiketler'
import { talepAraligiMetni } from '@/common/utils/talepAraligi'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const ekibiGorur = auth.yetkiVar(Yetkiler.IzinTalebiYonet)
const excelAlabilir = auth.yetkiVar(Yetkiler.RaporGor)

const varsayilanFiltre: TalepFilter = {
  calisanArama: '',
  durum: [],
  izinTuruId: [],
  baslangic: '',
  bitis: '',
  sort: '',
}
const {
  filtre,
  sayfa,
  sayfaBoyutu,
  satirlar,
  toplam,
  yukleniyor,
  varsayilandanFarkli,
  filtreyiAyarla,
  filtreyiSifirla,
} = usePagedList(talepleriGetir, varsayilanFiltre, { sayiListeleri: ['izinTuruId'] })
const izinTurleri = shallowRef<IzinTuru[]>([])

const calisanKolonu: GridKolon<IzinTalebiSatiri> = {
  key: 'calisanAdSoyad',
  title: 'Çalışan',
  sirala: true,
  filtre: { tur: 'metin', anahtar: 'calisanArama' },
}

const kolonlar = computed<GridKolon<IzinTalebiSatiri>[]>(() => [
  ...(ekibiGorur ? [calisanKolonu] : []),
  {
    key: 'izinTuruAdi',
    title: 'İzin Türü',
    sirala: true,
    filtre: {
      tur: 'cokluSecim',
      anahtar: 'izinTuruId',
      secenekler: izinTurleri.value.map((t) => ({ title: t.ad, value: t.id })),
    },
  },
  {
    key: 'baslangic',
    title: 'Tarih',
    deger: talepAraligiMetni,
    sirala: true,
    filtre: { tur: 'tarih', baslangicAnahtari: 'baslangic', bitisAnahtari: 'bitis' },
  },
  { key: 'dakika', title: 'Süre', deger: (t) => t.sureMetni, sirala: true },
  {
    key: 'durum',
    title: 'Durum',
    sirala: true,
    filtre: { tur: 'cokluSecim', anahtar: 'durum', secenekler: secenekler(durumEtiketleri) },
  },
])

onMounted(async () => {
  izinTurleri.value = await izinTurleriniGetir()
})

function detayaGit(satir: IzinTalebiSatiri) {
  void router.push({ name: 'izin-talebi', params: { id: satir.id } })
}
</script>

<template>
  <v-container fluid>
    <PageHeader baslik="İzin Talepleri">
      <v-btn
        v-if="excelAlabilir"
        variant="tonal"
        :prepend-icon="FileSpreadsheetIcon"
        @click="talepleriExceleAktar({ ...filtre })"
      >
        Excel
      </v-btn>
      <v-btn color="primary" :prepend-icon="PlusIcon" :to="{ name: 'yeni-izin-talebi' }">
        Yeni talep
      </v-btn>
    </PageHeader>

    <DataGrid
      v-model:sayfa="sayfa"
      v-model:sayfa-boyutu="sayfaBoyutu"
      v-model:sirala="filtre.sort"
      :kolonlar="kolonlar"
      :satirlar="satirlar"
      :toplam="toplam"
      :yukleniyor="yukleniyor"
      :filtre="filtre"
      :sifirlanabilir="varsayilandanFarkli"
      bos-metin="Kayıtlı talep yok."
      satir-tiklanir
      @filtre-degisti="filtreyiAyarla"
      @sifirla="filtreyiSifirla"
      @satir-tikla="detayaGit"
    >
      <template #[`item.baslangic`]="{ item }">
        <router-link
          :to="{ name: 'izin-talebi', params: { id: item.id } }"
          class="text-high-emphasis"
          @click.stop
        >
          {{ talepAraligiMetni(item) }}
        </router-link>
      </template>
      <template #[`item.durum`]="{ item }">
        <DurumChip :durum="item.durum" />
      </template>
    </DataGrid>
  </v-container>
</template>
