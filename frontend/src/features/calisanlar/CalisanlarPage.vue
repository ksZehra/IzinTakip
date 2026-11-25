<script setup lang="ts">
import { PlusIcon } from '@lucide/vue'
import { computed, onMounted, ref, shallowRef } from 'vue'
import { useRouter } from 'vue-router'

import {
  calisanEkle,
  calisanlariGetir,
  type CalisanFilter,
  type CalisanSatiri,
} from '@/api/calisanlarApi'
import { departmanlariGetir, type Departman } from '@/api/departmanlarApi'
import DataGrid from '@/common/components/DataGrid.vue'
import type { GridKolon } from '@/common/components/dataGridTipleri'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { usePagedList } from '@/common/composables/usePagedList'
import { useToast } from '@/common/composables/useToast'
import { aktiflikSecenekleri } from '@/common/utils/etiketler'
import { tarihGoster } from '@/common/utils/tarih'

import CalisanForm from './CalisanForm.vue'
import { bosCalisanFormu } from './calisanFormu'

const router = useRouter()
const toast = useToast()
const { gonder, gonderiliyor, alanHatasi } = useFormSubmit()

const varsayilanFiltre: CalisanFilter = {
  arama: '',
  departmanId: [],
  aktif: true,
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
  yukle,
  filtreyiAyarla,
  filtreyiSifirla,
} = usePagedList(calisanlariGetir, varsayilanFiltre, {
  sayiListeleri: ['departmanId'],
  gizliAlanlar: ['arama'],
})
const departmanlar = shallowRef<Departman[]>([])
const dialogAcik = shallowRef(false)
const form = ref(bosCalisanFormu())

const kolonlar = computed<GridKolon<CalisanSatiri>[]>(() => [
  {
    key: 'adSoyad',
    title: 'Ad Soyad',
    deger: (c) => `${c.ad} ${c.soyad}`,
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
    deger: (c) => tarihGoster(c.iseGirisTarihi),
    sirala: true,
    filtre: {
      tur: 'tarih',
      baslangicAnahtari: 'iseGirisBaslangic',
      bitisAnahtari: 'iseGirisBitis',
    },
  },
  {
    key: 'aktif',
    title: 'Durum',
    deger: (c) => (c.aktif ? 'Aktif' : 'Pasif'),
    sirala: true,
    filtre: { tur: 'tekliSecim', anahtar: 'aktif', secenekler: aktiflikSecenekleri },
  },
])

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

function detayaGit(satir: CalisanSatiri) {
  void router.push({ name: 'calisan', params: { id: satir.id } })
}

onMounted(async () => {
  departmanlar.value = await departmanlariGetir()
})
</script>

<template>
  <v-container fluid>
    <PageHeader baslik="Çalışanlar">
      <v-btn color="primary" :prepend-icon="PlusIcon" @click="yeni">Yeni çalışan</v-btn>
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
      bos-metin="Çalışan bulunamadı."
      satir-tiklanir
      @filtre-degisti="filtreyiAyarla"
      @sifirla="filtreyiSifirla"
      @satir-tikla="detayaGit"
    >
      <template #[`item.adSoyad`]="{ item }">
        <router-link :to="{ name: 'calisan', params: { id: item.id } }" @click.stop>
          {{ item.ad }} {{ item.soyad }}
        </router-link>
      </template>
    </DataGrid>

    <CalisanForm
      v-model="dialogAcik"
      v-model:form="form"
      baslik="Yeni Çalışan"
      :departmanlar="departmanlar"
      :kaydediliyor="gonderiliyor"
      :alan-hatasi="alanHatasi"
      @kaydet="ekle"
    />
  </v-container>
</template>
