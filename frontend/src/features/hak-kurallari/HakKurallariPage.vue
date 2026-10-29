<script setup lang="ts">
import { mdiPencil, mdiPlus } from '@mdi/js'
import { onMounted, shallowRef } from 'vue'

import {
  hakKuraliAktifligiDegistir,
  hakKuraliEkle,
  hakKuraliGuncelle,
  hakKurallariniGetir,
  type HakKurali,
  type HakKuraliKaydetIstegi,
} from '@/api/tanimlarApi'
import KayitDialog from '@/common/components/KayitDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useKayitFormu } from '@/common/composables/useKayitFormu'
import { useToast } from '@/common/composables/useToast'

const toast = useToast()
const kurallar = shallowRef<HakKurali[]>([])

async function yukle() {
  kurallar.value = await hakKurallariniGetir()
}

const { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi } = useKayitFormu<
  HakKurali,
  HakKuraliKaydetIstegi
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

async function aktiflikDegistir(kural: HakKurali, aktif: boolean | null) {
  await hakKuraliAktifligiDegistir(kural.id, aktif ?? false)
  toast.basariGoster(aktif ? 'Kural aktif yapıldı.' : 'Kural pasif yapıldı.')
  await yukle()
}

const basliklar = [
  { title: 'Ad', key: 'ad' },
  {
    title: 'Kıdem',
    key: 'kidem',
    value: (k: HakKurali) => aralik(k.minKidemYil, k.maxKidemYil, 'yıl'),
  },
  { title: 'Yaş', key: 'yas', value: (k: HakKurali) => aralik(k.minYas, k.maxYas, 'yaş') },
  { title: 'Gün', key: 'gun' },
  { title: 'Aktif', key: 'aktif', sortable: false },
  { title: '', key: 'islemler', sortable: false, align: 'end' as const },
]

onMounted(yukle)
</script>

<template>
  <v-container>
    <PageHeader
      baslik="Hak Kuralları"
      aciklama="Yıllık izin hakkı, koşulu tutan aktif kuralların en yükseğidir."
    >
      <v-btn color="primary" :prepend-icon="mdiPlus" @click="yeni">Yeni kural</v-btn>
    </PageHeader>

    <v-card>
      <v-data-table :headers="basliklar" :items="kurallar" items-per-page="-1" hide-default-footer>
        <template #[`item.aktif`]="{ item }">
          <v-switch
            :model-value="item.aktif"
            color="primary"
            hide-details
            density="compact"
            :aria-label="`${item.ad} aktif`"
            @update:model-value="aktiflikDegistir(item, $event)"
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
      :baslik="yeniKayit ? 'Yeni hak kuralı' : 'Hak kuralını düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field v-model="form.ad" label="Ad" :error-messages="alanHatasi('ad')" />
      <div class="d-flex flex-wrap ga-4">
        <v-number-input
          v-model="form.minKidemYil"
          label="En az kıdem (yıl)"
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('minKidemYil')"
        />
        <v-number-input
          v-model="form.maxKidemYil"
          label="En çok kıdem (boşsa sınırsız)"
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('maxKidemYil')"
        />
      </div>
      <div class="d-flex flex-wrap ga-4">
        <v-number-input
          v-model="form.minYas"
          label="En az yaş (isteğe bağlı)"
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('minYas')"
        />
        <v-number-input
          v-model="form.maxYas"
          label="En çok yaş (isteğe bağlı)"
          :min="0"
          class="flex-1-1"
          :error-messages="alanHatasi('maxYas')"
        />
      </div>
      <v-number-input
        v-model="form.gun"
        label="Yıllık izin (gün)"
        :min="0"
        :error-messages="alanHatasi('gun')"
      />
    </KayitDialog>
  </v-container>
</template>
