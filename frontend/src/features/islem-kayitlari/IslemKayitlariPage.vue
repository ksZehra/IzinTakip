<script setup lang="ts">
import {
  islemKayitlariniGetir,
  type IslemKaydi,
  type IslemKaydiFilter,
} from '@/api/kullanicilarApi'
import PageHeader from '@/common/components/PageHeader.vue'
import { usePagedList } from '@/common/composables/usePagedList'
import { tarihSaatGoster } from '@/common/utils/tarih'

const varsayilanFiltre: IslemKaydiFilter = {
  tablo: null,
  baslangic: null,
  bitis: null,
  sadeceKendiKaydi: false,
}
const { filtre, sayfa, sayfaBoyutu, satirlar, toplam, yukleniyor } = usePagedList(
  islemKayitlariniGetir,
  varsayilanFiltre,
)

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
const tablolar = Object.entries(tabloEtiketleri).map(([value, title]) => ({ value, title }))

const basliklar = [
  { title: 'Tarih', key: 'tarih', value: (k: IslemKaydi) => tarihSaatGoster(k.tarih) },
  { title: 'Yapan', key: 'kullaniciAdi' },
  { title: 'İşlem', key: 'islem', value: (k: IslemKaydi) => islemEtiketleri[k.islem] ?? k.islem },
  { title: 'Kayıt', key: 'tablo', value: (k: IslemKaydi) => tabloEtiketleri[k.tablo] ?? k.tablo },
  { title: 'İlgili', key: 'ilgiliCalisanAdi', value: (k: IslemKaydi) => k.ilgiliCalisanAdi ?? '—' },
  { title: 'Değişiklikler', key: 'degisiklikler', sortable: false },
]
</script>

<template>
  <v-container fluid>
    <PageHeader
      baslik="İşlem Kayıtları"
      aciklama="Yönetim tablolarındaki ekleme, güncelleme ve silmeler: kim, ne zaman, neyi değiştirdi."
    />

    <v-row density="compact" class="mb-2 align-center">
      <v-col cols="12" sm="6" md="3">
        <v-select
          v-model="filtre.tablo"
          :items="tablolar"
          label="Kayıt türü"
          clearable
          hide-details
        />
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-text-field
          v-model="filtre.baslangic"
          label="Başlangıç"
          type="date"
          clearable
          hide-details
        />
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-text-field v-model="filtre.bitis" label="Bitiş" type="date" clearable hide-details />
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-switch
          v-model="filtre.sadeceKendiKaydi"
          label="Yalnız kişinin kendi kaydı"
          color="primary"
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
      >
        <template #[`item.degisiklikler`]="{ item }">
          <ul class="degisiklikler">
            <li v-for="degisiklik in item.degisiklikler" :key="degisiklik.alan">
              <strong>{{ degisiklik.alan }}:</strong>
              {{ degisiklik.eski ?? '—' }} → {{ degisiklik.yeni ?? '—' }}
            </li>
          </ul>
          <v-chip v-if="item.kendiKaydi" size="x-small" color="warning" class="mt-1">
            Kendi kaydı
          </v-chip>
        </template>
      </v-data-table-server>
    </v-card>
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
