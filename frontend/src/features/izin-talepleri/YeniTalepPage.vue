<script setup lang="ts">
import { computed, onMounted, reactive, ref, shallowRef, watch } from 'vue'
import { useRouter } from 'vue-router'

import type { Ayarlar } from '@/api/ayarlarApi'
import { calisanlariGetir, type CalisanSatiri } from '@/api/calisanlarApi'
import {
  talepOlustur,
  type GunYarisi,
  type SureHesaplaIstegi,
  type SureTipi,
} from '@/api/izinTalepleriApi'
import { izinTurleriniGetir, type IzinTuru } from '@/api/izinTurleriApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormGonder } from '@/common/composables/useFormGonder'
import { useToast } from '@/common/composables/useToast'
import { gunYarisiEtiketleri, secenekler, sureTipiEtiketleri } from '@/common/utils/etiketler'
import { saatGoster } from '@/common/utils/tarih'
import { useAuthStore } from '@/stores/authStore'
import { useAyarlarStore } from '@/stores/ayarlarStore'

import { useSureOnizleme } from './useSureOnizleme'

const auth = useAuthStore()
const ayarlarStore = useAyarlarStore()
const router = useRouter()
const toast = useToast()
const { gonder, gonderiliyor, alanHatasi } = useFormGonder()

const baskasiAdinaAcabilir = auth.yetkiVar(Yetkiler.IzinTalebiYonet)
const izinTurleri = shallowRef<IzinTuru[]>([])
const ayarlar = shallowRef<Ayarlar | null>(null)
const calisanlar = shallowRef<CalisanSatiri[]>([])
const calisanAramasi = ref('')

const form = reactive({
  calisanId: null as number | null,
  izinTuruId: null as number | null,
  sureTipi: null as SureTipi | null,
  tarih: '',
  bitisTarihi: '',
  gunYarisi: null as GunYarisi | null,
  baslangicSaati: '',
  bitisSaati: '',
  aciklama: '',
})

const secilenTur = computed(() => izinTurleri.value.find((tur) => tur.id === form.izinTuruId))
const izinliBicimler = computed(() =>
  secenekler(sureTipiEtiketleri).filter(({ value }) => {
    const tur = secilenTur.value
    if (!tur) return false
    return { TamGun: tur.tamGunAlinir, YarimGun: tur.yarimGunAlinir, Saatlik: tur.saatlikAlinir }[
      value
    ]
  }),
)
const saatAdimiSaniye = computed(() => (ayarlar.value?.saatlikIzinAdimDakika ?? 30) * 60)

const onizlemeIstegi = computed<SureHesaplaIstegi | null>(() => {
  const istek = talepIstegi()
  if (!istek) return null
  const eksik =
    (istek.sureTipi === 'TamGun' && !istek.bitisTarihi) ||
    (istek.sureTipi === 'YarimGun' && !istek.gunYarisi) ||
    (istek.sureTipi === 'Saatlik' && (!istek.baslangicSaati || !istek.bitisSaati))
  return eksik ? null : istek
})
const { sureMetni, hata: onizlemeHatasi } = useSureOnizleme(onizlemeIstegi)

watch(izinliBicimler, (bicimler) => {
  if (!bicimler.some(({ value }) => value === form.sureTipi)) {
    form.sureTipi = bicimler[0]?.value ?? null
  }
})

watch(calisanAramasi, async (arama) => {
  if (!baskasiAdinaAcabilir) return
  calisanlar.value = (
    await calisanlariGetir({ arama: arama ?? '', aktif: true, pageSize: 20 })
  ).items
})

onMounted(async () => {
  ;[izinTurleri.value, ayarlar.value] = await Promise.all([
    izinTurleriniGetir(true),
    ayarlarStore.yukle(),
  ])
  if (baskasiAdinaAcabilir) {
    calisanlar.value = (await calisanlariGetir({ aktif: true, pageSize: 20 })).items
  }
})

function talepIstegi(): SureHesaplaIstegi | null {
  if (!form.izinTuruId || !form.sureTipi || !form.tarih) return null
  return {
    izinTuruId: form.izinTuruId,
    sureTipi: form.sureTipi,
    tarih: form.tarih,
    bitisTarihi: form.sureTipi === 'TamGun' ? form.bitisTarihi || null : null,
    gunYarisi: form.sureTipi === 'YarimGun' ? form.gunYarisi : null,
    baslangicSaati: form.sureTipi === 'Saatlik' ? form.baslangicSaati || null : null,
    bitisSaati: form.sureTipi === 'Saatlik' ? form.bitisSaati || null : null,
  }
}

async function gonderTalep() {
  const talep = await gonder(() =>
    talepOlustur({
      izinTuruId: form.izinTuruId ?? 0,
      sureTipi: form.sureTipi ?? 'TamGun',
      tarih: form.tarih,
      bitisTarihi: null,
      gunYarisi: null,
      baslangicSaati: null,
      bitisSaati: null,
      ...talepIstegi(),
      calisanId: form.calisanId,
      aciklama: form.aciklama.trim() || null,
    }),
  )
  if (!talep) return
  toast.basariGoster('Talep gönderildi.')
  if (secilenTur.value?.belgeZorunlu) {
    toast.bilgiGoster('Bu izin türü belge istiyor. Belgeyi talep sayfasından yükleyin.')
  }
  await router.push({ name: 'izin-talebi', params: { id: talep.id } })
}
</script>

<template>
  <v-container style="max-width: 760px">
    <PageHeader baslik="Yeni İzin Talebi" />

    <v-card>
      <v-card-text>
        <v-form class="d-flex flex-column ga-4" @submit.prevent="gonderTalep">
          <v-autocomplete
            v-if="baskasiAdinaAcabilir"
            v-model="form.calisanId"
            v-model:search="calisanAramasi"
            :items="calisanlar"
            :item-title="(calisan: CalisanSatiri) => `${calisan.ad} ${calisan.soyad}`"
            item-value="id"
            label="Çalışan"
            placeholder="Boş bırakırsanız kendi adınıza açılır"
            persistent-placeholder
            clearable
            no-filter
            :error-messages="alanHatasi('calisanId')"
          />

          <v-select
            v-model="form.izinTuruId"
            :items="izinTurleri"
            item-title="ad"
            item-value="id"
            label="İzin türü"
            :error-messages="alanHatasi('izinTuruId')"
          />

          <v-radio-group
            v-if="izinliBicimler.length > 1"
            v-model="form.sureTipi"
            label="Süre biçimi"
            inline
            hide-details="auto"
            :error-messages="alanHatasi('sureTipi')"
          >
            <v-radio
              v-for="bicim in izinliBicimler"
              :key="bicim.value"
              :label="bicim.title"
              :value="bicim.value"
            />
          </v-radio-group>

          <template v-if="form.sureTipi">
            <div class="d-flex flex-wrap ga-4">
              <v-text-field
                v-model="form.tarih"
                :label="form.sureTipi === 'TamGun' ? 'Başlangıç tarihi' : 'Tarih'"
                type="date"
                class="flex-1-1"
                :error-messages="alanHatasi('tarih')"
              />
              <v-text-field
                v-if="form.sureTipi === 'TamGun'"
                v-model="form.bitisTarihi"
                label="Bitiş tarihi"
                type="date"
                :min="form.tarih || undefined"
                class="flex-1-1"
                :error-messages="alanHatasi('bitisTarihi')"
              />
            </div>

            <v-radio-group
              v-if="form.sureTipi === 'YarimGun'"
              v-model="form.gunYarisi"
              label="Günün hangi yarısı"
              inline
              hide-details="auto"
              :error-messages="alanHatasi('gunYarisi')"
            >
              <v-radio
                v-for="yari in secenekler(gunYarisiEtiketleri)"
                :key="yari.value"
                :label="yari.title"
                :value="yari.value"
              />
            </v-radio-group>

            <div v-if="form.sureTipi === 'Saatlik'" class="d-flex flex-wrap ga-4">
              <v-text-field
                v-model="form.baslangicSaati"
                label="Başlangıç saati"
                type="time"
                :step="saatAdimiSaniye"
                :min="ayarlar ? saatGoster(ayarlar.mesaiBaslangic) : undefined"
                :max="ayarlar ? saatGoster(ayarlar.mesaiBitis) : undefined"
                class="flex-1-1"
                :error-messages="alanHatasi('baslangicSaati')"
              />
              <v-text-field
                v-model="form.bitisSaati"
                label="Bitiş saati"
                type="time"
                :step="saatAdimiSaniye"
                :min="ayarlar ? saatGoster(ayarlar.mesaiBaslangic) : undefined"
                :max="ayarlar ? saatGoster(ayarlar.mesaiBitis) : undefined"
                class="flex-1-1"
                :error-messages="alanHatasi('bitisSaati')"
              />
            </div>
          </template>

          <v-alert
            v-if="sureMetni || onizlemeHatasi"
            :type="onizlemeHatasi ? 'warning' : 'info'"
            variant="tonal"
            aria-live="polite"
          >
            {{ onizlemeHatasi || `Süre: ${sureMetni}` }}
          </v-alert>

          <v-textarea
            v-model="form.aciklama"
            label="Açıklama (isteğe bağlı)"
            rows="2"
            auto-grow
            counter="500"
            maxlength="500"
            :error-messages="alanHatasi('aciklama')"
          />

          <div class="d-flex justify-end ga-2">
            <v-btn variant="text" :to="{ name: 'izin-talepleri' }">Vazgeç</v-btn>
            <v-btn type="submit" color="primary" :loading="gonderiliyor">Talebi gönder</v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>
