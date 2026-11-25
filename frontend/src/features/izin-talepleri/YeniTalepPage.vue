<script setup lang="ts">
import { computed, onMounted, reactive, shallowRef, watch } from 'vue'
import { useRouter } from 'vue-router'

import type { Ayarlar } from '@/api/ayarlarApi'
import {
  talepOlustur,
  type GunYarisi,
  type SureHesaplaRequest,
  type SureTipi,
} from '@/api/izinTalepleriApi'
import { izinTurleriniGetir, type IzinTuru } from '@/api/izinTurleriApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import CalisanSelect from '@/common/components/CalisanSelect.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
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
const { gonder, gonderiliyor, alanHatasi } = useFormSubmit()

const baskasiAdinaAcabilir = auth.yetkiVar(Yetkiler.IzinTalebiYonet)
const izinTurleri = shallowRef<IzinTuru[]>([])
const ayarlar = shallowRef<Ayarlar | null>(null)

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

const onizlemeIstegi = computed<SureHesaplaRequest | null>(() => {
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

onMounted(async () => {
  ;[izinTurleri.value, ayarlar.value] = await Promise.all([
    izinTurleriniGetir(true),
    ayarlarStore.yukle(),
  ])
})

function talepIstegi(): SureHesaplaRequest | null {
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
          <CalisanSelect
            v-if="baskasiAdinaAcabilir"
            v-model="form.calisanId"
            etiket="Çalışan"
            yer-tutucu="Boş bırakırsanız kendi adınıza açılır"
            :hatalar="alanHatasi('calisanId')"
          />

          <v-select
            v-model="form.izinTuruId"
            :items="izinTurleri"
            item-title="ad"
            item-value="id"
            label="İzin Türü"
            required
            :error-messages="alanHatasi('izinTuruId')"
          />

          <v-radio-group
            v-if="izinliBicimler.length > 1"
            v-model="form.sureTipi"
            label="Süre Biçimi"
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
                :label="form.sureTipi === 'TamGun' ? 'Başlangıç Tarihi' : 'Tarih'"
                type="date"
                required
                class="flex-1-1"
                :error-messages="alanHatasi('tarih')"
              />
              <v-text-field
                v-if="form.sureTipi === 'TamGun'"
                v-model="form.bitisTarihi"
                label="Bitiş Tarihi"
                type="date"
                required
                :min="form.tarih || undefined"
                class="flex-1-1"
                :error-messages="alanHatasi('bitisTarihi')"
              />
            </div>

            <v-radio-group
              v-if="form.sureTipi === 'YarimGun'"
              v-model="form.gunYarisi"
              label="Günün Hangi Yarısı"
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
                label="Başlangıç Saati"
                type="time"
                required
                :step="saatAdimiSaniye"
                :min="ayarlar ? saatGoster(ayarlar.mesaiBaslangic) : undefined"
                :max="ayarlar ? saatGoster(ayarlar.mesaiBitis) : undefined"
                class="flex-1-1"
                :error-messages="alanHatasi('baslangicSaati')"
              />
              <v-text-field
                v-model="form.bitisSaati"
                label="Bitiş Saati"
                type="time"
                required
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
            label="Açıklama (İsteğe Bağlı)"
            rows="2"
            auto-grow
            counter="500"
            maxlength="500"
            :error-messages="alanHatasi('aciklama')"
          >
            <template #counter="{ counter }">
              <span aria-hidden="true">{{ counter }}</span>
            </template>
          </v-textarea>

          <div class="d-flex justify-end ga-2">
            <v-btn variant="tonal" :to="{ name: 'izin-talepleri' }">Vazgeç</v-btn>
            <v-btn type="submit" color="primary" :loading="gonderiliyor">Talebi gönder</v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>
