<script setup lang="ts">
import { Trash2Icon, PencilIcon, PlusIcon } from '@lucide/vue'
import { ref, shallowRef, watch } from 'vue'

import {
  resmiTatilEkle,
  resmiTatilGuncelle,
  resmiTatilleriGetir,
  resmiTatilSil,
  type ResmiTatil,
  type ResmiTatilKaydetRequest,
} from '@/api/tanimlarApi'
import ConfirmDialog from '@/common/components/ConfirmDialog.vue'
import DataGrid from '@/common/components/DataGrid.vue'
import type { GridKolon } from '@/common/components/dataGridTipleri'
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useCrudForm } from '@/common/composables/useCrudForm'
import { useToast } from '@/common/composables/useToast'
import { useYerelFiltre } from '@/common/composables/useYerelFiltre'
import { gunBasligi } from '@/common/utils/tarih'

const toast = useToast()
const silme = useFormSubmit()
const buYil = new Date().getFullYear()
const yil = ref(buYil)
const yillar = [buYil - 1, buYil, buYil + 1, buYil + 2]
const tatiller = shallowRef<ResmiTatil[]>([])
const silinecek = ref<ResmiTatil | null>(null)
const silmeAcik = ref(false)

async function yukle() {
  tatiller.value = await resmiTatilleriGetir(yil.value)
}

const { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi } = useCrudForm<
  ResmiTatil,
  ResmiTatilKaydetRequest
>({
  bosForm: () => ({ tarih: '', ad: '', yarimGun: false }),
  formaCevir: ({ tarih, ad, yarimGun }) => ({ tarih, ad, yarimGun }),
  ekle: resmiTatilEkle,
  guncelle: resmiTatilGuncelle,
  kaydedildi: yukle,
})

function silmeyiSor(tatil: ResmiTatil) {
  silinecek.value = tatil
  silmeAcik.value = true
}

async function sil() {
  const tatil = silinecek.value
  if (!tatil) return
  const basarili = await silme.gonder(async () => {
    await resmiTatilSil(tatil.id)
    return true
  })
  if (!basarili) return
  silmeAcik.value = false
  toast.basariGoster('Resmi tatil silindi.')
  await yukle()
}

const { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla } = useYerelFiltre(
  {
    ad: '',
    baslangic: '',
    bitis: '',
    yarimGun: null as boolean | null,
    sort: '',
  },
  { gizliAlanlar: ['ad'] },
)

const kolonlar: GridKolon<ResmiTatil>[] = [
  {
    key: 'tarih',
    title: 'Tarih',
    deger: (t) => gunBasligi(t.tarih),
    sirala: true,
    filtre: { tur: 'tarih', baslangicAnahtari: 'baslangic', bitisAnahtari: 'bitis' },
  },
  { key: 'ad', title: 'Ad', sirala: true, filtre: { tur: 'metin', anahtar: 'ad' } },
  {
    key: 'yarimGun',
    title: 'Süre',
    deger: (t) => (t.yarimGun ? 'Yarım gün (arife)' : 'Tam gün'),
    sirala: true,
    filtre: {
      tur: 'tekliSecim',
      anahtar: 'yarimGun',
      secenekler: [
        { title: 'Tam gün', value: false },
        { title: 'Yarım gün (arife)', value: true },
      ],
    },
  },
  { key: 'islemler', title: '', hizala: 'end' },
]

watch(yil, yukle, { immediate: true })
</script>

<template>
  <v-container>
    <PageHeader baslik="Resmi Tatiller">
      <v-select
        v-model="yil"
        :items="yillar"
        label="Yıl"
        hide-details
        density="compact"
        style="max-width: 140px"
      />
      <v-btn color="primary" :prepend-icon="PlusIcon" @click="yeni">Yeni tatil</v-btn>
    </PageHeader>

    <DataGrid
      v-model:sirala="filtre.sort"
      yerel
      :kolonlar="kolonlar"
      :satirlar="tatiller"
      :filtre="filtre"
      :sifirlanabilir="varsayilandanFarkli"
      bos-metin="Bu yıl için resmi tatil girilmemiş."
      @filtre-degisti="filtreyiAyarla"
      @sifirla="filtreyiSifirla"
    >
      <template #[`item.islemler`]="{ item }">
        <v-btn
          :icon="PencilIcon"
          variant="text"
          size="small"
          :aria-label="`${item.ad} düzenle`"
          @click="duzenle(item)"
        />
        <v-btn
          :icon="Trash2Icon"
          variant="text"
          size="small"
          color="error"
          :aria-label="`${item.ad} sil`"
          @click="silmeyiSor(item)"
        />
      </template>
    </DataGrid>

    <FormDialog
      v-model="acik"
      :baslik="yeniKayit ? 'Yeni Resmi Tatil' : 'Resmi Tatili Düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field
        v-model="form.tarih"
        label="Tarih"
        required
        type="date"
        :error-messages="alanHatasi('tarih')"
      />
      <v-text-field v-model="form.ad" label="Ad" required :error-messages="alanHatasi('ad')" />
      <v-checkbox
        v-model="form.yarimGun"
        label="Yarım Gün (Arife: Öğleden Sonrası Tatil)"
        hide-details
      />
    </FormDialog>

    <ConfirmDialog
      v-model="silmeAcik"
      baslik="Resmi Tatili Sil"
      :metin="`${silinecek?.ad ?? ''} silinecek.`"
      onay-metni="Sil"
      onay-rengi="error"
      :calisiyor="silme.gonderiliyor.value"
      @onay="sil"
    />
  </v-container>
</template>
