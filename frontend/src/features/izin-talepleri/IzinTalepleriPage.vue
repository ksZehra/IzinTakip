<script setup lang="ts">
import { mdiMicrosoftExcel, mdiPlus } from '@mdi/js'
import { onMounted, shallowRef } from 'vue'
import { useRouter } from 'vue-router'

import {
  talepleriExceleAktar,
  talepleriGetir,
  type IzinTalebiSatiri,
  type TalepFilter,
} from '@/api/izinTalepleriApi'
import { izinTurleriniGetir, type IzinTuru } from '@/api/izinTurleriApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import DurumChip from '@/common/components/DurumChip.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { usePagedList } from '@/common/composables/usePagedList'
import { durumEtiketleri, secenekler } from '@/common/utils/etiketler'
import { useAuthStore } from '@/stores/authStore'

import { talepAraligiMetni } from '@/common/utils/talepAraligi'

const auth = useAuthStore()
const router = useRouter()
const ekibiGorur = auth.yetkiVar(Yetkiler.IzinTalebiYonet)
const excelAlabilir = auth.yetkiVar(Yetkiler.RaporGor)

const varsayilanFiltre: TalepFilter = {
  calisanId: null,
  durum: null,
  izinTuruId: null,
  baslangic: null,
  bitis: null,
}
const { filtre, sayfa, sayfaBoyutu, satirlar, toplam, yukleniyor } = usePagedList(
  talepleriGetir,
  varsayilanFiltre,
)
const izinTurleri = shallowRef<IzinTuru[]>([])
const durumlar = secenekler(durumEtiketleri)

const basliklar = [
  ...(ekibiGorur ? [{ title: 'Çalışan', key: 'calisanAdSoyad' }] : []),
  { title: 'İzin türü', key: 'izinTuruAdi' },
  { title: 'Tarih', key: 'aralik' },
  { title: 'Süre', key: 'sureMetni' },
  { title: 'Durum', key: 'durum' },
]

onMounted(async () => {
  izinTurleri.value = await izinTurleriniGetir()
})

function detayaGit(_olay: unknown, { item }: { item: IzinTalebiSatiri }) {
  void router.push({ name: 'izin-talebi', params: { id: item.id } })
}
</script>

<template>
  <v-container fluid>
    <PageHeader baslik="İzin Talepleri">
      <v-btn
        v-if="excelAlabilir"
        variant="tonal"
        :prepend-icon="mdiMicrosoftExcel"
        @click="talepleriExceleAktar({ ...filtre })"
      >
        Excel
      </v-btn>
      <v-btn color="primary" :prepend-icon="mdiPlus" :to="{ name: 'yeni-izin-talebi' }">
        Yeni talep
      </v-btn>
    </PageHeader>

    <v-row density="compact" class="mb-2">
      <v-col cols="12" sm="6" md="3">
        <v-select v-model="filtre.durum" :items="durumlar" label="Durum" clearable hide-details />
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-select
          v-model="filtre.izinTuruId"
          :items="izinTurleri"
          item-title="ad"
          item-value="id"
          label="İzin türü"
          clearable
          hide-details
        />
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-text-field
          v-model="filtre.baslangic"
          label="Başlangıç (en erken)"
          type="date"
          clearable
          hide-details
        />
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-text-field
          v-model="filtre.bitis"
          label="Bitiş (en geç)"
          type="date"
          clearable
          hide-details
        />
      </v-col>
    </v-row>

    <v-card>
      <v-data-table-server
        v-model:page="sayfa"
        v-model:items-per-page="sayfaBoyutu"
        :headers="basliklar"
        :items="satirlar"
        :items-length="toplam"
        :loading="yukleniyor"
        :items-per-page-options="[10, 20, 50, 100]"
        :row-props="{ class: 'cursor-pointer' }"
        hover
        no-data-text="Kayıtlı talep yok."
        @click:row="detayaGit"
      >
        <template #[`item.aralik`]="{ item }">
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
      </v-data-table-server>
    </v-card>
  </v-container>
</template>
