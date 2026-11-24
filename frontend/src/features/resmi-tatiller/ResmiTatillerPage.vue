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
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useCrudForm } from '@/common/composables/useCrudForm'
import { useToast } from '@/common/composables/useToast'
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

const basliklar = [
  { title: 'Tarih', key: 'tarih', value: (t: ResmiTatil) => gunBasligi(t.tarih) },
  { title: 'Ad', key: 'ad' },
  {
    title: 'Süre',
    key: 'yarimGun',
    value: (t: ResmiTatil) => (t.yarimGun ? 'Yarım gün (arife)' : 'Tam gün'),
  },
  { title: '', key: 'islemler', sortable: false, align: 'end' as const },
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

    <v-card>
      <v-data-table
        :headers="basliklar"
        :items="tatiller"
        items-per-page="-1"
        hide-default-footer
        no-data-text="Bu yıl için resmi tatil girilmemiş."
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
      </v-data-table>
    </v-card>

    <FormDialog
      v-model="acik"
      :baslik="yeniKayit ? 'Yeni resmi tatil' : 'Resmi tatili düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field
        v-model="form.tarih"
        label="Tarih"
        type="date"
        :error-messages="alanHatasi('tarih')"
      />
      <v-text-field v-model="form.ad" label="Ad" :error-messages="alanHatasi('ad')" />
      <v-checkbox
        v-model="form.yarimGun"
        label="Yarım gün (arife: öğleden sonrası tatil)"
        hide-details
      />
    </FormDialog>

    <ConfirmDialog
      v-model="silmeAcik"
      baslik="Resmi tatili sil"
      :metin="`${silinecek?.ad ?? ''} silinecek.`"
      onay-metni="Sil"
      onay-rengi="error"
      :calisiyor="silme.gonderiliyor.value"
      @onay="sil"
    />
  </v-container>
</template>
