<script setup lang="ts">
import { mdiDelete, mdiPencil, mdiPlus } from '@mdi/js'
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
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useCrudForm } from '@/common/composables/useCrudForm'
import { useToast } from '@/common/composables/useToast'
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

const basliklar = [
  { title: 'Ad', key: 'ad' },
  { title: 'Korumalı', key: 'korumali', value: (r: Rol) => (r.korumali ? 'Evet' : 'Hayır') },
  { title: 'Yetki', key: 'yetkiler', value: (r: Rol) => `${r.yetkiler.length} yetki` },
  { title: 'Kullanıcı', key: 'kullaniciSayisi' },
  { title: '', key: 'islemler', sortable: false, align: 'end' as const },
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
      <v-btn color="primary" :prepend-icon="mdiPlus" @click="yeni">Yeni rol</v-btn>
    </PageHeader>

    <v-card>
      <v-data-table :headers="basliklar" :items="roller" items-per-page="-1" hide-default-footer>
        <template #[`item.ad`]="{ item }">
          {{ rolEtiketi(item.ad) }}
          <v-chip v-if="item.sistemRolu" size="x-small" class="ml-1">Sistem</v-chip>
        </template>
        <template #[`item.islemler`]="{ item }">
          <v-btn
            v-if="item.ad !== 'SuperAdmin'"
            :icon="mdiPencil"
            variant="text"
            size="small"
            :aria-label="`${rolEtiketi(item.ad)} düzenle`"
            @click="duzenle(item)"
          />
          <v-btn
            v-if="!item.sistemRolu"
            :icon="mdiDelete"
            variant="text"
            size="small"
            color="error"
            :aria-label="`${rolEtiketi(item.ad)} sil`"
            @click="silmeyiSor(item)"
          />
        </template>
      </v-data-table>
    </v-card>

    <FormDialog
      v-model="acik"
      :baslik="yeniKayit ? 'Yeni rol' : 'Rolü düzenle'"
      :kaydediliyor="gonderiliyor"
      @kaydet="kaydet"
    >
      <v-text-field v-model="form.ad" label="Ad" :error-messages="alanHatasi('ad')" />
      <v-checkbox v-model="form.korumali" label="Korumalı (yalnız SuperAdmin atar)" hide-details />
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
      baslik="Rolü sil"
      :metin="`${rolEtiketi(silinecek?.ad ?? '')} rolü silinecek.`"
      onay-metni="Sil"
      onay-rengi="error"
      :calisiyor="silme.gonderiliyor.value"
      @onay="sil"
    />
  </v-container>
</template>
