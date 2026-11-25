<script setup lang="ts">
import { onMounted, ref } from 'vue'

import { ayarlariGuncelle, type AyarlarGuncelleRequest } from '@/api/ayarlarApi'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useToast } from '@/common/composables/useToast'
import { haftaGunuEtiketleri, secenekler } from '@/common/utils/etiketler'
import { saatGoster } from '@/common/utils/tarih'
import { useAyarlarStore } from '@/stores/ayarlarStore'

const ayarlarStore = useAyarlarStore()
const toast = useToast()
const { gonder, gonderiliyor, alanHatasi } = useFormSubmit()
const form = ref<AyarlarGuncelleRequest | null>(null)
const gunlukSure = ref('')
const adimlar = [15, 30, 60].map((dakika) => ({ title: `${dakika} dakika`, value: dakika }))

function formuDoldur() {
  const ayarlar = ayarlarStore.ayarlar
  if (!ayarlar) return
  form.value = {
    calismaGunleri: [...ayarlar.calismaGunleri],
    mesaiBaslangic: saatGoster(ayarlar.mesaiBaslangic),
    mesaiBitis: saatGoster(ayarlar.mesaiBitis),
    ogleArasiBaslangic: saatGoster(ayarlar.ogleArasiBaslangic),
    ogleArasiBitis: saatGoster(ayarlar.ogleArasiBitis),
    saatlikIzinAdimDakika: ayarlar.saatlikIzinAdimDakika,
    hatirlatmaGunSayisi: ayarlar.hatirlatmaGunSayisi,
  }
  const dakika = ayarlar.gunlukCalismaDakikasi
  gunlukSure.value = `${Math.floor(dakika / 60)} saat${dakika % 60 ? ` ${dakika % 60} dk` : ''}`
}

async function kaydet() {
  const istek = form.value
  if (!istek) return
  const guncel = await gonder(() => ayarlariGuncelle(istek))
  if (!guncel) return
  ayarlarStore.guncellendi(guncel)
  formuDoldur()
  toast.basariGoster('Ayarlar kaydedildi.')
}

onMounted(async () => {
  await ayarlarStore.yukle()
  formuDoldur()
})
</script>

<template>
  <v-container style="max-width: 760px">
    <PageHeader
      baslik="İş Takvimi Ayarları"
      :aciklama="gunlukSure ? `1 iş günü = ${gunlukSure}` : undefined"
    />

    <v-skeleton-loader v-if="!form" type="article" />

    <v-card v-else>
      <v-card-text>
        <v-form class="d-flex flex-column ga-4" @submit.prevent="kaydet">
          <fieldset class="border-0 pa-0">
            <legend class="text-body-medium">Çalışma Günleri</legend>
            <div class="d-flex flex-wrap">
              <v-checkbox
                v-for="gun in secenekler(haftaGunuEtiketleri)"
                :key="gun.value"
                v-model="form.calismaGunleri"
                :label="gun.title"
                :value="gun.value"
                hide-details
              />
            </div>
            <v-messages :messages="alanHatasi('calismaGunleri')" active color="error" />
          </fieldset>
          <div class="d-flex flex-wrap ga-4">
            <v-text-field
              v-model="form.mesaiBaslangic"
              label="Mesai Başlangıcı"
              required
              type="time"
              class="flex-1-1"
              :error-messages="alanHatasi('mesaiBaslangic')"
            />
            <v-text-field
              v-model="form.mesaiBitis"
              label="Mesai Bitişi"
              required
              type="time"
              class="flex-1-1"
              :error-messages="alanHatasi('mesaiBitis')"
            />
          </div>
          <div class="d-flex flex-wrap ga-4">
            <v-text-field
              v-model="form.ogleArasiBaslangic"
              label="Öğle Arası Başlangıcı"
              required
              type="time"
              class="flex-1-1"
              :error-messages="alanHatasi('ogleArasiBaslangic')"
            />
            <v-text-field
              v-model="form.ogleArasiBitis"
              label="Öğle Arası Bitişi"
              required
              type="time"
              class="flex-1-1"
              :error-messages="alanHatasi('ogleArasiBitis')"
            />
          </div>
          <div class="d-flex flex-wrap ga-4">
            <v-select
              v-model="form.saatlikIzinAdimDakika"
              :items="adimlar"
              label="Saatlik İzin Adımı"
              required
              class="flex-1-1"
              :error-messages="alanHatasi('saatlikIzinAdimDakika')"
            />
            <v-number-input
              v-model="form.hatirlatmaGunSayisi"
              label="Hatırlatma (Gün)"
              required
              hint="Bu kadar gün bekleyen talep için onaycıya hatırlatma gider."
              persistent-hint
              :min="1"
              class="flex-1-1"
              :error-messages="alanHatasi('hatirlatmaGunSayisi')"
            />
          </div>
          <div class="d-flex justify-end">
            <v-btn type="submit" color="primary" :loading="gonderiliyor">Kaydet</v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>
