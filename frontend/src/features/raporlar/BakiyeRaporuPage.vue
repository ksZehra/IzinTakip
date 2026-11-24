<script setup lang="ts">
import { FileSpreadsheetIcon } from '@lucide/vue'
import { onMounted, shallowRef } from 'vue'

import { departmanlariGetir, type Departman } from '@/api/departmanlarApi'
import { bakiyeleriExceleAktar, bakiyeleriGetir, type BakiyeFilter } from '@/api/raporlarApi'
import PageHeader from '@/common/components/PageHeader.vue'
import { usePagedList } from '@/common/composables/usePagedList'
import { tarihGoster } from '@/common/utils/tarih'

const varsayilanFiltre: BakiyeFilter = { departmanId: null }
const { filtre, sayfa, sayfaBoyutu, satirlar, toplam, yukleniyor } = usePagedList(
  bakiyeleriGetir,
  varsayilanFiltre,
)
const departmanlar = shallowRef<Departman[]>([])

const basliklar = [
  { title: 'Çalışan', key: 'calisanAdSoyad' },
  { title: 'Departman', key: 'departmanAdi' },
  {
    title: 'İşe giriş',
    key: 'iseGirisTarihi',
    value: (s: { iseGirisTarihi: string }) => tarihGoster(s.iseGirisTarihi),
  },
  { title: 'Toplam hak', key: 'hakMetni' },
  { title: 'Kullanılan', key: 'kullanilanMetni' },
  { title: 'Bekleyen', key: 'bekleyenMetni' },
  { title: 'Kullanılabilir', key: 'kullanilabilirMetni' },
]

onMounted(async () => {
  departmanlar.value = (await departmanlariGetir()).filter((departman) => departman.aktif)
})
</script>

<template>
  <v-container fluid>
    <PageHeader
      baslik="Bakiye Raporu"
      aciklama="Kapsamınızdaki aktif çalışanların izin bakiyeleri."
    >
      <v-btn
        variant="tonal"
        :prepend-icon="FileSpreadsheetIcon"
        @click="bakiyeleriExceleAktar({ ...filtre })"
      >
        Excel
      </v-btn>
    </PageHeader>

    <v-row density="compact" class="mb-2">
      <v-col cols="12" sm="6" md="4">
        <v-select
          v-model="filtre.departmanId"
          :items="departmanlar"
          item-title="ad"
          item-value="id"
          label="Departman"
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
        no-data-text="Kayıt yok."
      />
    </v-card>
  </v-container>
</template>
