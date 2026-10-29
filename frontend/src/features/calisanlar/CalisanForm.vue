<script setup lang="ts">
import type { CalisanKaydetIstegi } from '@/api/calisanlarApi'
import type { Departman } from '@/api/departmanlarApi'
import KayitDialog from '@/common/components/KayitDialog.vue'

defineProps<{
  baslik: string
  departmanlar: Departman[]
  kaydediliyor: boolean
  alanHatasi: (alan: string) => string[]
}>()
const emit = defineEmits<{ kaydet: [] }>()
const acik = defineModel<boolean>({ required: true })
const form = defineModel<CalisanKaydetIstegi>('form', { required: true })
</script>

<template>
  <KayitDialog
    v-model="acik"
    :baslik="baslik"
    :kaydediliyor="kaydediliyor"
    @kaydet="emit('kaydet')"
  >
    <div class="d-flex flex-wrap ga-4">
      <v-text-field
        v-model="form.ad"
        label="Ad"
        class="flex-1-1"
        :error-messages="alanHatasi('ad')"
      />
      <v-text-field
        v-model="form.soyad"
        label="Soyad"
        class="flex-1-1"
        :error-messages="alanHatasi('soyad')"
      />
    </div>
    <v-select
      v-model="form.departmanId"
      :items="departmanlar.filter((departman) => departman.aktif)"
      item-title="ad"
      item-value="id"
      label="Departman"
      :error-messages="alanHatasi('departmanId')"
    />
    <div class="d-flex flex-wrap ga-4">
      <v-text-field
        v-model="form.iseGirisTarihi"
        label="İşe giriş tarihi"
        type="date"
        class="flex-1-1"
        :error-messages="alanHatasi('iseGirisTarihi')"
      />
      <v-text-field
        v-model="form.dogumTarihi"
        label="Doğum tarihi"
        type="date"
        class="flex-1-1"
        :error-messages="alanHatasi('dogumTarihi')"
      />
    </div>
    <v-number-input
      v-model="form.hakBaslangicKidemYili"
      label="Hak başlangıç kıdem yılı"
      hint="Boş bırakılırsa kayıt günündeki kıdemin bir fazlası alınır (yeni işe girende 1)."
      persistent-hint
      :min="1"
      :error-messages="alanHatasi('hakBaslangicKidemYili')"
    />
  </KayitDialog>
</template>
