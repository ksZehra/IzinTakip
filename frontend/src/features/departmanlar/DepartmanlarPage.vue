<script setup lang="ts">
import { mdiPencil, mdiPlus } from '@mdi/js'
import { onMounted, shallowRef } from 'vue'

import {
  departmanAktifligiDegistir,
  departmanEkle,
  departmanGuncelle,
  departmanlariGetir,
  type Departman,
  type DepartmanKaydetIstegi,
} from '@/api/departmanlarApi'
import CalisanSecici from '@/common/components/CalisanSecici.vue'
import AktiflikAnahtari from '@/common/components/AktiflikAnahtari.vue'
import KayitDialog from '@/common/components/KayitDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useKayitFormu } from '@/common/composables/useKayitFormu'
import { useToast } from '@/common/composables/useToast'

const toast = useToast()
const departmanlar = shallowRef<Departman[]>([])
const seciliYoneticiAdi = shallowRef<string | null>(null)
const yukleniyor = shallowRef(false)

async function yukle() {
  yukleniyor.value = true
  try {
    departmanlar.value = await departmanlariGetir()
  } finally {
    yukleniyor.value = false
  }
}

const { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi } = useKayitFormu<
  Departman,
  DepartmanKaydetIstegi
>({
  bosForm: () => ({ ad: '', yoneticiId: null }),
  formaCevir: (departman) => {
    seciliYoneticiAdi.value = departman.yoneticiAdSoyad
    return { ad: departman.ad, yoneticiId: departman.yoneticiId }
  },
  ekle: departmanEkle,
  guncelle: departmanGuncelle,
  kaydedildi: yukle,
})

async function aktiflikDegistir(departman: Departman, aktif: boolean) {
  await departmanAktifligiDegistir(departman.id, aktif)
  toast.basariGoster(aktif ? 'Departman aktif yapıldı.' : 'Departman pasif yapıldı.')
  await yukle()
}

const basliklar = [
  { title: 'Ad', key: 'ad' },
  { title: 'Yönetici', key: 'yoneticiAdSoyad' },
  { title: 'Aktif çalışan', key: 'aktifCalisanSayisi' },
  { title: 'Aktif', key: 'aktif', sortable: false },
  { title: '', key: 'islemler', sortable: false, align: 'end' as const },
]

onMounted(yukle)
</script>

<template>
  <v-container>
    <PageHeader baslik="Departmanlar">
      <v-btn color="primary" :prepend-icon="mdiPlus" @click="yeni">Yeni departman</v-btn>
    </PageHeader>

    <v-card>
      <v-data-table
        :headers="basliklar"
        :items="departmanlar"
        :loading="yukleniyor"
        items-per-page="-1"
        hide-default-footer
        no-data-text="Departman yok."
      >
        <template #[`item.yoneticiAdSoyad`]="{ item }">{{ item.yoneticiAdSoyad ?? '—' }}</template>
        <template #[`item.aktif`]="{ item }">
          <AktiflikAnahtari
            :aktif="item.aktif"
            :etiket="`${item.ad} aktif`"
            :degistir="(aktif) => aktiflikDegistir(item, aktif)"
          />
        </template>
        <template #[`item.islemler`]="{ item }">
          <v-btn
            :icon="mdiPencil"
            variant="text"
            size="small"
            :aria-label="`${item.ad} düzenle`"
            @click="duzenle(item)"
          />
        </template>
      </v-data-table>
    </v-card>

    <KayitDialog
      v-model="acik"
      :baslik="yeniKayit ? 'Yeni departman' : 'Departmanı düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field v-model="form.ad" label="Ad" :error-messages="alanHatasi('ad')" />
      <CalisanSecici
        v-model="form.yoneticiId"
        etiket="Yönetici"
        :secili-ad="seciliYoneticiAdi"
        :hatalar="alanHatasi('yoneticiId')"
      />
    </KayitDialog>
  </v-container>
</template>
