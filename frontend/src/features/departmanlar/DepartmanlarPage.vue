<script setup lang="ts">
import { PencilIcon, PlusIcon } from '@lucide/vue'
import { onMounted, shallowRef } from 'vue'

import {
  departmanAktifligiDegistir,
  departmanEkle,
  departmanGuncelle,
  departmanlariGetir,
  type Departman,
  type DepartmanKaydetRequest,
} from '@/api/departmanlarApi'
import CalisanSelect from '@/common/components/CalisanSelect.vue'
import AktiflikSwitch from '@/common/components/AktiflikSwitch.vue'
import DataGrid from '@/common/components/DataGrid.vue'
import type { GridKolon } from '@/common/components/dataGridTipleri'
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useCrudForm } from '@/common/composables/useCrudForm'
import { useToast } from '@/common/composables/useToast'
import { useYerelFiltre } from '@/common/composables/useYerelFiltre'
import { aktiflikSecenekleri } from '@/common/utils/etiketler'

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

const { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi } = useCrudForm<
  Departman,
  DepartmanKaydetRequest
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

const { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla } = useYerelFiltre(
  {
    ad: '',
    yonetici: '',
    aktif: null as boolean | null,
    sort: '',
  },
  { gizliAlanlar: ['ad', 'yonetici'] },
)

const kolonlar: GridKolon<Departman>[] = [
  { key: 'ad', title: 'Ad', sirala: true, filtre: { tur: 'metin', anahtar: 'ad' } },
  {
    key: 'yoneticiAdSoyad',
    title: 'Yönetici',
    deger: (d) => d.yoneticiAdSoyad ?? '—',
    sirala: true,
    filtre: { tur: 'metin', anahtar: 'yonetici' },
  },
  { key: 'aktifCalisanSayisi', title: 'Aktif Çalışan', sirala: true },
  {
    key: 'aktif',
    title: 'Aktif',
    sirala: true,
    filtre: { tur: 'tekliSecim', anahtar: 'aktif', secenekler: aktiflikSecenekleri },
  },
  { key: 'islemler', title: '', hizala: 'end' },
]

onMounted(yukle)
</script>

<template>
  <v-container>
    <PageHeader baslik="Departmanlar">
      <v-btn color="primary" :prepend-icon="PlusIcon" @click="yeni">Yeni departman</v-btn>
    </PageHeader>

    <DataGrid
      v-model:sirala="filtre.sort"
      yerel
      :kolonlar="kolonlar"
      :satirlar="departmanlar"
      :yukleniyor="yukleniyor"
      :filtre="filtre"
      :sifirlanabilir="varsayilandanFarkli"
      bos-metin="Departman yok."
      @filtre-degisti="filtreyiAyarla"
      @sifirla="filtreyiSifirla"
    >
      <template #[`item.aktif`]="{ item }">
        <AktiflikSwitch
          :aktif="item.aktif"
          :etiket="`${item.ad} aktif`"
          :degistir="(aktif) => aktiflikDegistir(item, aktif)"
        />
      </template>
      <template #[`item.islemler`]="{ item }">
        <v-btn
          :icon="PencilIcon"
          variant="text"
          size="small"
          :aria-label="`${item.ad} düzenle`"
          @click="duzenle(item)"
        />
      </template>
    </DataGrid>

    <FormDialog
      v-model="acik"
      :baslik="yeniKayit ? 'Yeni Departman' : 'Departmanı Düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field v-model="form.ad" label="Ad" required :error-messages="alanHatasi('ad')" />
      <CalisanSelect
        v-model="form.yoneticiId"
        etiket="Yönetici"
        :secili-ad="seciliYoneticiAdi"
        :hatalar="alanHatasi('yoneticiId')"
      />
    </FormDialog>
  </v-container>
</template>
