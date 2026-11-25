<script setup lang="ts">
import { Trash2Icon, PencilIcon, PlusIcon } from '@lucide/vue'
import { onMounted, ref, shallowRef } from 'vue'

import {
  atanabilirYetkileriGetir,
  rolEkle,
  rolGuncelle,
  rolleriGetir,
  rolSil,
  type Rol,
  type RolKaydetRequest,
  type YetkiTanimi,
} from '@/api/kullanicilarApi'
import ConfirmDialog from '@/common/components/ConfirmDialog.vue'
import DataGrid from '@/common/components/DataGrid.vue'
import type { GridKolon } from '@/common/components/dataGridTipleri'
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useCrudForm } from '@/common/composables/useCrudForm'
import { useToast } from '@/common/composables/useToast'
import { useYerelFiltre } from '@/common/composables/useYerelFiltre'
import { rolEtiketi } from '@/common/utils/etiketler'

const toast = useToast()
const silme = useFormSubmit()
const roller = shallowRef<Rol[]>([])
const yetkiler = shallowRef<YetkiTanimi[]>([])
const silinecek = ref<Rol | null>(null)
const silmeAcik = ref(false)

async function yukle() {
  roller.value = await rolleriGetir()
}

const { acik, form, yeniKayit, yeni, duzenle, kaydet, gonderiliyor, alanHatasi } = useCrudForm<
  Rol,
  RolKaydetRequest
>({
  bosForm: () => ({ ad: '', korumali: false, yetkiler: [] }),
  formaCevir: (rol) => ({ ad: rol.ad, korumali: rol.korumali, yetkiler: [...rol.yetkiler] }),
  ekle: rolEkle,
  guncelle: rolGuncelle,
  kaydedildi: yukle,
})

function silmeyiSor(rol: Rol) {
  silinecek.value = rol
  silmeAcik.value = true
}

async function sil() {
  const rol = silinecek.value
  if (!rol) return
  const basarili = await silme.gonder(async () => {
    await rolSil(rol.id)
    return true
  })
  if (!basarili) return
  silmeAcik.value = false
  toast.basariGoster('Rol silindi.')
  await yukle()
}

const { filtre, varsayilandanFarkli, filtreyiAyarla, filtreyiSifirla } = useYerelFiltre(
  {
    ad: '',
    korumali: null as boolean | null,
    sort: '',
  },
  { gizliAlanlar: ['ad'] },
)

const kolonlar: GridKolon<Rol>[] = [
  {
    key: 'ad',
    title: 'Ad',
    deger: (r) => rolEtiketi(r.ad),
    ham: (r) => rolEtiketi(r.ad),
    sirala: true,
    filtre: { tur: 'metin', anahtar: 'ad' },
  },
  {
    key: 'korumali',
    title: 'Korumalı',
    deger: (r) => (r.korumali ? 'Evet' : 'Hayır'),
    sirala: true,
    filtre: {
      tur: 'tekliSecim',
      anahtar: 'korumali',
      secenekler: [
        { title: 'Evet', value: true },
        { title: 'Hayır', value: false },
      ],
    },
  },
  {
    key: 'yetkiler',
    title: 'Yetki',
    deger: (r) => `${r.yetkiler.length} yetki`,
    ham: (r) => r.yetkiler.length,
    sirala: true,
  },
  { key: 'kullaniciSayisi', title: 'Kullanıcı', sirala: true },
  { key: 'islemler', title: '', hizala: 'end' },
]

onMounted(async () => {
  ;[roller.value, yetkiler.value] = await Promise.all([rolleriGetir(), atanabilirYetkileriGetir()])
})
</script>

<template>
  <v-container>
    <PageHeader
      baslik="Roller"
      aciklama="Korumalı rolleri yalnız SuperAdmin atar; İK korumalı rolü olan kullanıcıya dokunamaz."
    >
      <v-btn color="primary" :prepend-icon="PlusIcon" @click="yeni">Yeni rol</v-btn>
    </PageHeader>

    <DataGrid
      v-model:sirala="filtre.sort"
      yerel
      :kolonlar="kolonlar"
      :satirlar="roller"
      :filtre="filtre"
      :sifirlanabilir="varsayilandanFarkli"
      bos-metin="Rol yok."
      @filtre-degisti="filtreyiAyarla"
      @sifirla="filtreyiSifirla"
    >
      <template #[`item.ad`]="{ item }">
        {{ rolEtiketi(item.ad) }}
        <v-chip v-if="item.sistemRolu" size="x-small" class="ml-1">Sistem</v-chip>
      </template>
      <template #[`item.islemler`]="{ item }">
        <v-btn
          v-if="item.ad !== 'SuperAdmin'"
          :icon="PencilIcon"
          variant="text"
          size="small"
          :aria-label="`${rolEtiketi(item.ad)} düzenle`"
          @click="duzenle(item)"
        />
        <v-btn
          v-if="!item.sistemRolu"
          :icon="Trash2Icon"
          variant="text"
          size="small"
          color="error"
          :aria-label="`${rolEtiketi(item.ad)} sil`"
          @click="silmeyiSor(item)"
        />
      </template>
    </DataGrid>

    <FormDialog
      v-model="acik"
      :baslik="yeniKayit ? 'Yeni Rol' : 'Rolü Düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field v-model="form.ad" label="Ad" required :error-messages="alanHatasi('ad')" />
      <v-checkbox v-model="form.korumali" label="Korumalı (Yalnız SuperAdmin Atar)" hide-details />
      <fieldset class="border-0 pa-0">
        <legend class="text-body-medium mb-1">Yetkiler</legend>
        <v-checkbox
          v-for="yetki in yetkiler"
          :key="yetki.kod"
          v-model="form.yetkiler"
          :value="yetki.kod"
          :label="yetki.aciklama"
          density="compact"
          hide-details
        />
        <v-messages :messages="alanHatasi('yetkiler')" active color="error" />
      </fieldset>
    </FormDialog>

    <ConfirmDialog
      v-model="silmeAcik"
      baslik="Rolü Sil"
      :metin="`${rolEtiketi(silinecek?.ad ?? '')} rolü silinecek.`"
      onay-metni="Sil"
      onay-rengi="error"
      :calisiyor="silme.gonderiliyor.value"
      @onay="sil"
    />
  </v-container>
</template>
