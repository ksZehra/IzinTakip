<script setup lang="ts">
import {
  islemKayitlariniGetir,
  type IslemKaydi,
  type IslemKaydiFilter,
} from '@/api/kullanicilarApi'
import DataGrid from '@/common/components/DataGrid.vue'
import type { GridKolon } from '@/common/components/dataGridTipleri'
import PageHeader from '@/common/components/PageHeader.vue'
import { usePagedList } from '@/common/composables/usePagedList'
import { tarihSaatGoster } from '@/common/utils/tarih'

import { alanEtiketi, degerGoster } from './degisiklikGoster'

const varsayilanFiltre: IslemKaydiFilter = {
  ilgiliArama: '',
  tablo: [],
  islem: [],
  baslangic: '',
  bitis: '',
  sadeceKendiKaydi: null,
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
} = usePagedList(islemKayitlariniGetir, varsayilanFiltre, {
  gizliAlanlar: ['ilgiliArama'],
})

const islemEtiketleri: Record<string, string> = {
  Eklendi: 'Eklendi',
  Guncellendi: 'Güncellendi',
  Silindi: 'Silindi',
}
const tabloEtiketleri: Record<string, string> = {
  Calisan: 'Çalışan',
  IzinHakki: 'Hak satırı',
  Departman: 'Departman',
  IzinTuru: 'İzin türü',
  IzinHakkiKurali: 'Hak kuralı',
  ResmiTatil: 'Resmi tatil',
  Ayarlar: 'Ayarlar',
  Kullanici: 'Kullanıcı',
  Rol: 'Rol',
  KullaniciRolu: 'Rol ataması',
  RolYetkisi: 'Rol yetkisi',
}
const secenekListesi = (etiketler: Record<string, string>) =>
  Object.entries(etiketler).map(([value, title]) => ({ value, title }))

const kolonlar: GridKolon<IslemKaydi>[] = [
  {
    key: 'tarih',
    title: 'Tarih',
    deger: (k) => tarihSaatGoster(k.tarih),
    sirala: true,
    filtre: { tur: 'tarih', baslangicAnahtari: 'baslangic', bitisAnahtari: 'bitis' },
  },
  { key: 'kullaniciAdi', title: 'Yapan' },
  {
    key: 'islem',
    title: 'İşlem',
    deger: (k) => islemEtiketleri[k.islem] ?? k.islem,
    sirala: true,
    filtre: { tur: 'cokluSecim', anahtar: 'islem', secenekler: secenekListesi(islemEtiketleri) },
  },
  {
    key: 'tablo',
    title: 'Kayıt',
    deger: (k) => tabloEtiketleri[k.tablo] ?? k.tablo,
    sirala: true,
    filtre: { tur: 'cokluSecim', anahtar: 'tablo', secenekler: secenekListesi(tabloEtiketleri) },
  },
  {
    key: 'ilgiliCalisanAdi',
    title: 'İlgili',
    deger: (k) => k.ilgiliCalisanAdi ?? '—',
    sirala: true,
    filtre: { tur: 'metin', anahtar: 'ilgiliArama' },
  },
  {
    key: 'degisiklikler',
    title: 'Değişiklikler',
    filtre: {
      tur: 'tekliSecim',
      anahtar: 'sadeceKendiKaydi',
      secenekler: [{ title: 'Yalnız kişinin kendi kaydı', value: true }],
    },
  },
]
</script>

<template>
  <v-container fluid>
    <PageHeader
      baslik="İşlem Kayıtları"
      aciklama="Yönetim tablolarındaki ekleme, güncelleme ve silmeler: kim, ne zaman, neyi değiştirdi."
    />

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
    >
      <template #[`item.degisiklikler`]="{ item }">
        <ul class="degisiklikler">
          <li v-for="degisiklik in item.degisiklikler" :key="degisiklik.alan">
            <strong>{{ alanEtiketi(degisiklik.alan) }}:</strong>
            {{ degerGoster(degisiklik.alan, degisiklik.eski) }} →
            {{ degerGoster(degisiklik.alan, degisiklik.yeni) }}
          </li>
        </ul>
        <v-chip v-if="item.kendiKaydi" size="x-small" color="warning" class="mt-1">
          Kendi kaydı
        </v-chip>
      </template>
    </DataGrid>
  </v-container>
</template>

<style scoped>
.degisiklikler {
  list-style: none;
  padding: 4px 0;
  margin: 0;
  font-size: 0.875rem;
}
</style>
