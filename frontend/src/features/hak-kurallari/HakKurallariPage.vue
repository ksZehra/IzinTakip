<script setup lang="ts">
import { PencilIcon, PlusIcon } from '@lucide/vue'
import { onMounted, shallowRef } from 'vue'

import {
  hakKuraliAktifligiDegistir,
  hakKuraliEkle,
  hakKuraliGuncelle,
  hakKurallariniGetir,
  type HakKurali,
  type HakKuraliKaydetRequest,
} from '@/api/tanimlarApi'
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
const kurallar = shallowRef<HakKurali[]>([])

async function yukle() {
  kurallar.value = await hakKurallariniGetir()
}

const { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi } = useCrudForm<
  HakKurali,
  HakKuraliKaydetRequest
>({
  bosForm: () => ({
    ad: '',
    minKidemYil: 1,
    maxKidemYil: null,
    minYas: null,
    maxYas: null,
    gun: 14,
  }),
  formaCevir: ({ id, aktif, ...istek }) => istek,
  ekle: hakKuraliEkle,
  guncelle: hakKuraliGuncelle,
  kaydedildi: yukle,
})

const aralik = (en: number | null, cok: number | null, birim: string) =>
  en === null && cok === null
    ? '—'
    : cok === null
      ? `${en} ${birim} ve üstü`
      : en === null
        ? `en çok ${cok} ${birim}`
        : `${en}–${cok} ${birim}`

async function aktiflikDegistir(kural: HakKurali, aktif: boolean) {
  await hakKuraliAktifligiDegistir(kural.id, aktif)
  toast.basariGoster(aktif ? 'Kural aktif yapıldı.' : 'Kural pasif yapıldı.')
  await yukle()
}

const { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla } = useYerelFiltre(
  {
    ad: '',
    aktif: null as boolean | null,
    sort: '',
  },
  { gizliAlanlar: ['ad'] },
)

const kolonlar: GridKolon<HakKurali>[] = [
  { key: 'ad', title: 'Ad', sirala: true, filtre: { tur: 'metin', anahtar: 'ad' } },
  {
    key: 'kidem',
    title: 'Kıdem',
    deger: (k) => aralik(k.minKidemYil, k.maxKidemYil, 'yıl'),
    ham: (k) => k.minKidemYil,
    sirala: true,
  },
  {
    key: 'yas',
    title: 'Yaş',
    deger: (k) => aralik(k.minYas, k.maxYas, 'yaş'),
    ham: (k) => k.minYas ?? k.maxYas,
    sirala: true,
  },
  { key: 'gun', title: 'Gün', sirala: true },
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
    <PageHeader
      baslik="Hak Kuralları"
      aciklama="Yıllık izin hakkı, koşulu tutan aktif kuralların en yükseğidir."
    >
      <v-btn color="primary" :prepend-icon="PlusIcon" @click="yeni">Yeni kural</v-btn>
    </PageHeader>

    <DataGrid
      v-model:sirala="filtre.sort"
      yerel
      :kolonlar="kolonlar"
      :satirlar="kurallar"
      :filtre="filtre"
      :sifirlanabilir="varsayilandanFarkli"
      bos-metin="Hak kuralı yok."
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
      :baslik="yeniKayit ? 'Yeni Hak Kuralı' : 'Hak Kuralını Düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field v-model="form.ad" label="Ad" required :error-messages="alanHatasi('ad')" />
      <div class="d-flex flex-wrap ga-4">
        <v-number-input
          v-model="form.minKidemYil"
          label="En Az Kıdem (Yıl)"
          required
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('minKidemYil')"
        />
        <v-number-input
          v-model="form.maxKidemYil"
          label="En Çok Kıdem (Boşsa Sınırsız)"
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('maxKidemYil')"
        />
      </div>
      <div class="d-flex flex-wrap ga-4">
        <v-number-input
          v-model="form.minYas"
          label="En Az Yaş (İsteğe Bağlı)"
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('minYas')"
        />
        <v-number-input
          v-model="form.maxYas"
          label="En Çok Yaş (İsteğe Bağlı)"
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('maxYas')"
        />
      </div>
      <v-number-input
        v-model="form.gun"
        label="Yıllık İzin (Gün)"
        required
        :min="0"
        :error-messages="alanHatasi('gun')"
      />
    </FormDialog>
  </v-container>
</template>
