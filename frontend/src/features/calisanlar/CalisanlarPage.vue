<script setup lang="ts">
import { mdiPlus } from '@mdi/js'
import { onMounted, ref, shallowRef } from 'vue'
import { useRouter } from 'vue-router'

import {
  calisanEkle,
  calisanlariGetir,
  type CalisanFiltresi,
  type CalisanSatiri,
} from '@/api/calisanlarApi'
import { departmanlariGetir, type Departman } from '@/api/departmanlarApi'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormGonder } from '@/common/composables/useFormGonder'
import { useSayfaliListe } from '@/common/composables/useSayfaliListe'
import { useToast } from '@/common/composables/useToast'
import { tarihGoster } from '@/common/utils/tarih'

import CalisanForm from './CalisanForm.vue'
import { bosCalisanFormu } from './calisanFormu'

const router = useRouter()
const toast = useToast()
const { gonder, gonderiliyor, alanHatasi } = useFormGonder()

const varsayilanFiltre: CalisanFiltresi = { arama: '', departmanId: null, aktif: true }
const { filtre, sayfa, sayfaBoyutu, satirlar, toplam, yukleniyor, yukle } = useSayfaliListe(
  calisanlariGetir,
  varsayilanFiltre,
)
const departmanlar = shallowRef<Departman[]>([])
const dialogAcik = shallowRef(false)
const form = ref(bosCalisanFormu())

const basliklar = [
  { title: 'Ad soyad', key: 'adSoyad', value: (c: CalisanSatiri) => `${c.ad} ${c.soyad}` },
  { title: 'Departman', key: 'departmanAdi' },
  {
    title: 'İşe giriş',
    key: 'iseGirisTarihi',
    value: (c: CalisanSatiri) => tarihGoster(c.iseGirisTarihi),
  },
  { title: 'Durum', key: 'aktif', value: (c: CalisanSatiri) => (c.aktif ? 'Aktif' : 'Pasif') },
]
const aktiflikSecenekleri = [
  { title: 'Aktif', value: true },
  { title: 'Pasif', value: false },
]

function yeni() {
  form.value = bosCalisanFormu()
  dialogAcik.value = true
}

async function ekle() {
  const calisan = await gonder(() => calisanEkle(form.value))
  if (!calisan) return
  dialogAcik.value = false
  toast.basariGoster('Çalışan eklendi.')
  await yukle()
}

function detayaGit(_olay: unknown, { item }: { item: CalisanSatiri }) {
  void router.push({ name: 'calisan', params: { id: item.id } })
}

onMounted(async () => {
  departmanlar.value = await departmanlariGetir()
})
</script>

<template>
  <v-container fluid>
    <PageHeader baslik="Çalışanlar">
      <v-btn color="primary" :prepend-icon="mdiPlus" @click="yeni">Yeni çalışan</v-btn>
    </PageHeader>

    <v-row density="compact" class="mb-2">
      <v-col cols="12" md="4">
        <v-text-field v-model="filtre.arama" label="Ad ya da soyad ara" clearable hide-details />
      </v-col>
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
      <v-col cols="12" sm="6" md="4">
        <v-select
          v-model="filtre.aktif"
          :items="aktiflikSecenekleri"
          label="Durum"
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
        no-data-text="Çalışan bulunamadı."
        @click:row="detayaGit"
      >
        <template #[`item.adSoyad`]="{ item }">
          <router-link :to="{ name: 'calisan', params: { id: item.id } }" @click.stop>
            {{ item.ad }} {{ item.soyad }}
          </router-link>
        </template>
      </v-data-table-server>
    </v-card>

    <CalisanForm
      v-model="dialogAcik"
      v-model:form="form"
      baslik="Yeni çalışan"
      :departmanlar="departmanlar"
      :kaydediliyor="gonderiliyor"
      :alan-hatasi="alanHatasi"
      @kaydet="ekle"
    />
  </v-container>
</template>
