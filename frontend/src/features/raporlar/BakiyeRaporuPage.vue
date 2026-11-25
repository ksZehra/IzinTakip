<script setup lang="ts">
import { FileSpreadsheetIcon } from '@lucide/vue'
import { computed, onMounted, shallowRef } from 'vue'

import { departmanlariGetir, type Departman } from '@/api/departmanlarApi'
import {
  bakiyeleriExceleAktar,
  bakiyeleriGetir,
  type BakiyeFilter,
  type BakiyeSatiri,
} from '@/api/raporlarApi'
import DataGrid from '@/common/components/DataGrid.vue'
import type { GridKolon } from '@/common/components/dataGridTipleri'
import PageHeader from '@/common/components/PageHeader.vue'
import { usePagedList } from '@/common/composables/usePagedList'
import { tarihGoster } from '@/common/utils/tarih'

const varsayilanFiltre: BakiyeFilter = {
  arama: '',
  departmanId: [],
  iseGirisBaslangic: '',
  iseGirisBitis: '',
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
} = usePagedList(bakiyeleriGetir, varsayilanFiltre, {
  sayiListeleri: ['departmanId'],
  gizliAlanlar: ['arama'],
})
const departmanlar = shallowRef<Departman[]>([])

const kolonlar = computed<GridKolon<BakiyeSatiri>[]>(() => [
  {
    key: 'calisanAdSoyad',
    title: 'Çalışan',
    sirala: true,
    filtre: { tur: 'metin', anahtar: 'arama' },
  },
  {
    key: 'departmanAdi',
    title: 'Departman',
    sirala: true,
    filtre: {
      tur: 'cokluSecim',
      anahtar: 'departmanId',
      secenekler: departmanlar.value.map((d) => ({ title: d.ad, value: d.id })),
    },
  },
  {
    key: 'iseGirisTarihi',
    title: 'İşe Giriş Tarihi',
    deger: (s) => tarihGoster(s.iseGirisTarihi),
    sirala: true,
    filtre: {
      tur: 'tarih',
      baslangicAnahtari: 'iseGirisBaslangic',
      bitisAnahtari: 'iseGirisBitis',
    },
  },
  { key: 'hakMetni', title: 'Toplam Hak' },
  { key: 'kullanilanMetni', title: 'Kullanılan' },
  { key: 'bekleyenMetni', title: 'Bekleyen' },
  { key: 'kullanilabilirMetni', title: 'Kullanılabilir' },
])

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
      bos-metin="Kayıt yok."
      @filtre-degisti="filtreyiAyarla"
      @sifirla="filtreyiSifirla"
    />
  </v-container>
</template>
