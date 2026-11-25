<script setup lang="ts">
import type { CalisanKaydetRequest } from '@/api/calisanlarApi'
import type { Departman } from '@/api/departmanlarApi'
import FormDialog from '@/common/components/FormDialog.vue'

defineProps<{
  baslik: string
  departmanlar: Departman[]
  kaydediliyor: boolean
  alanHatasi: (alan: string) => string[]
}>()
const emit = defineEmits<{ kaydet: [] }>()
const acik = defineModel<boolean>({ required: true })
const form = defineModel<CalisanKaydetRequest>('form', { required: true })
</script>

<template>
  <FormDialog v-model="acik" :baslik="baslik" :kaydediliyor="kaydediliyor" @kaydet="emit('kaydet')">
    <div class="d-flex flex-wrap ga-4">
      <v-text-field
        v-model="form.ad"
        label="Ad"
        required
        class="flex-1-1"
        :error-messages="alanHatasi('ad')"
      />
      <v-text-field
        v-model="form.soyad"
        label="Soyad"
        required
        class="flex-1-1"
        :error-messages="alanHatasi('soyad')"
      />
    </div>
    <v-select
      :model-value="form.departmanId || null"
      :items="departmanlar.filter((departman) => departman.aktif)"
      item-title="ad"
      item-value="id"
      label="Departman"
      required
      :error-messages="alanHatasi('departmanId')"
      @update:model-value="form.departmanId = $event ?? 0"
    />
    <div class="d-flex flex-wrap ga-4">
      <v-text-field
        :model-value="form.iseGirisTarihi"
        label="İşe Giriş Tarihi"
        type="date"
        required
        class="flex-1-1"
        :error-messages="alanHatasi('iseGirisTarihi')"
        @update:model-value="form.iseGirisTarihi = $event || null"
      />
      <v-text-field
        :model-value="form.dogumTarihi"
        label="Doğum Tarihi"
        type="date"
        required
        class="flex-1-1"
        :error-messages="alanHatasi('dogumTarihi')"
        @update:model-value="form.dogumTarihi = $event || null"
      />
    </div>
    <v-number-input
      v-model="form.hakBaslangicKidemYili"
      label="Hak Başlangıç Kıdem Yılı"
      hint="Boş bırakılırsa kayıt günündeki kıdemin bir fazlası alınır (yeni işe girende 1)."
      persistent-hint
      :min="1"
      :error-messages="alanHatasi('hakBaslangicKidemYili')"
    />
  </FormDialog>
</template>
