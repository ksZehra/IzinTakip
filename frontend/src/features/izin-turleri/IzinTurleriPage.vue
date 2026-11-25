<script setup lang="ts">
import { PencilIcon, PlusIcon } from '@lucide/vue'
import { onMounted, shallowRef } from 'vue'

import {
  izinTuruAktifligiDegistir,
  izinTuruEkle,
  izinTuruGuncelle,
  izinTurleriniGetir,
  type IzinTuru,
  type IzinTuruKaydetRequest,
} from '@/api/izinTurleriApi'
import AktiflikSwitch from '@/common/components/AktiflikSwitch.vue'
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import SureInput from '@/common/components/SureInput.vue'
import { useCrudForm } from '@/common/composables/useCrudForm'
import { useToast } from '@/common/composables/useToast'
import { gunHesabiEtiketleri, secenekler } from '@/common/utils/etiketler'

const toast = useToast()
const turler = shallowRef<IzinTuru[]>([])

async function yukle() {
  turler.value = await izinTurleriniGetir()
}

const { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi } = useCrudForm<
  IzinTuru,
  IzinTuruKaydetRequest
>({
  bosForm: () => ({
    ad: '',
    bakiyedenDuser: false,
    belgeZorunlu: false,
    gunHesabi: 'IsGunu',
    tamGunAlinir: true,
    yarimGunAlinir: false,
    saatlikAlinir: false,
    talepBasinaEnFazlaDakika: null,
    yillikEnFazlaDakika: null,
  }),
  formaCevir: ({ id, aktif, talepBasinaEnFazlaMetni, yillikEnFazlaMetni, ...istek }) => istek,
  ekle: izinTuruEkle,
  guncelle: izinTuruGuncelle,
  kaydedildi: yukle,
})

function bicimler(tur: IzinTuru): string {
  return [
    tur.tamGunAlinir && 'Tam gün',
    tur.yarimGunAlinir && 'Yarım gün',
    tur.saatlikAlinir && 'Saatlik',
  ]
    .filter(Boolean)
    .join(', ')
}

async function aktiflikDegistir(tur: IzinTuru, aktif: boolean) {
  await izinTuruAktifligiDegistir(tur.id, aktif)
  toast.basariGoster(aktif ? 'İzin türü aktif yapıldı.' : 'İzin türü pasif yapıldı.')
  await yukle()
}

const basliklar = [
  { title: 'Ad', key: 'ad' },
  { title: 'Biçimler', key: 'bicimler', value: bicimler },
  {
    title: 'Bakiyeden Düşer',
    key: 'bakiyedenDuser',
    value: (t: IzinTuru) => (t.bakiyedenDuser ? 'Evet' : 'Hayır'),
  },
  {
    title: 'Belge',
    key: 'belgeZorunlu',
    value: (t: IzinTuru) => (t.belgeZorunlu ? 'Zorunlu' : '—'),
  },
  {
    title: 'Talep Başına En Çok',
    key: 'talepBasinaEnFazlaMetni',
    value: (t: IzinTuru) => t.talepBasinaEnFazlaMetni ?? 'Sınırsız',
  },
  {
    title: 'Yılda En Çok',
    key: 'yillikEnFazlaMetni',
    value: (t: IzinTuru) => t.yillikEnFazlaMetni ?? 'Sınırsız',
  },
  { title: 'Aktif', key: 'aktif', sortable: false },
  { title: '', key: 'islemler', sortable: false, align: 'end' as const },
]

onMounted(yukle)
</script>

<template>
  <v-container fluid>
    <PageHeader baslik="İzin Türleri">
      <v-btn color="primary" :prepend-icon="PlusIcon" @click="yeni">Yeni izin türü</v-btn>
    </PageHeader>

    <v-card>
      <v-data-table :headers="basliklar" :items="turler" items-per-page="-1" hide-default-footer>
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
      </v-data-table>
    </v-card>

    <FormDialog
      v-model="acik"
      :baslik="yeniKayit ? 'Yeni İzin Türü' : 'İzin Türünü Düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field v-model="form.ad" label="Ad" required :error-messages="alanHatasi('ad')" />
      <fieldset class="border-0 pa-0">
        <legend class="text-body-medium">Alınabilir Biçimler</legend>
        <div class="d-flex flex-wrap">
          <v-checkbox v-model="form.tamGunAlinir" label="Tam Gün" hide-details />
          <v-checkbox v-model="form.yarimGunAlinir" label="Yarım Gün" hide-details />
          <v-checkbox v-model="form.saatlikAlinir" label="Saatlik" hide-details />
        </div>
        <v-messages :messages="alanHatasi('tamGunAlinir')" active color="error" />
      </fieldset>
      <v-select
        v-model="form.gunHesabi"
        :items="secenekler(gunHesabiEtiketleri)"
        label="Gün Hesabı"
        required
        :error-messages="alanHatasi('gunHesabi')"
      />
      <div class="d-flex flex-wrap">
        <v-checkbox v-model="form.bakiyedenDuser" label="Bakiyeden Düşer" hide-details />
        <v-checkbox v-model="form.belgeZorunlu" label="Belge Zorunlu" hide-details />
      </div>
      <SureInput
        v-model="form.talepBasinaEnFazlaDakika"
        etiket="Talep Başına En Çok"
        bos-olabilir
        :hatalar="alanHatasi('talepBasinaEnFazlaDakika')"
      />
      <SureInput
        v-model="form.yillikEnFazlaDakika"
        etiket="Takvim Yılında En Çok"
        bos-olabilir
        :hatalar="alanHatasi('yillikEnFazlaDakika')"
      />
    </FormDialog>
  </v-container>
</template>
