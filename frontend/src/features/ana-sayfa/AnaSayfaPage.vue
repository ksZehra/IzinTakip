<script setup lang="ts">
import { mdiPlus } from '@mdi/js'
import { onMounted, shallowRef } from 'vue'

import { bakiyeGetir, type Bakiye } from '@/api/calisanlarApi'
import { talepleriGetir, type IzinTalebiSatiri } from '@/api/izinTalepleriApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import DurumChip from '@/common/components/DurumChip.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useAuthStore } from '@/stores/authStore'

import { talepAraligiMetni } from '@/common/utils/talepAraligi'

const auth = useAuthStore()
const calisanId = auth.oturum?.calisanId ?? null
const talepAcabilir = auth.yetkiVar(Yetkiler.IzinTalebiOlustur, Yetkiler.IzinTalebiYonet)
const onaylayabilir = auth.yetkiVar(Yetkiler.IzinTalebiYonet)

const bakiye = shallowRef<Bakiye | null>(null)
const sonTaleplerim = shallowRef<IzinTalebiSatiri[]>([])
const onayBekleyenler = shallowRef<IzinTalebiSatiri[]>([])

const bos = { durum: null, izinTuruId: null, baslangic: null, bitis: null }

onMounted(async () => {
  const [bakiyeSonucu, taleplerim, bekleyenler] = await Promise.all([
    calisanId && talepAcabilir ? bakiyeGetir(calisanId) : null,
    calisanId && talepAcabilir ? talepleriGetir({ ...bos, calisanId, page: 1, pageSize: 5 }) : null,
    onaylayabilir
      ? talepleriGetir({ ...bos, calisanId: null, durum: 'Beklemede', page: 1, pageSize: 5 })
      : null,
  ])
  bakiye.value = bakiyeSonucu
  sonTaleplerim.value = taleplerim?.items ?? []
  onayBekleyenler.value = (bekleyenler?.items ?? []).filter((t) => t.calisanId !== calisanId)
})
</script>

<template>
  <v-container>
    <PageHeader baslik="Ana Sayfa" :aciklama="`Hoş geldiniz, ${auth.oturum?.gorunenAd ?? ''}.`">
      <v-btn
        v-if="talepAcabilir"
        color="primary"
        :prepend-icon="mdiPlus"
        :to="{ name: 'yeni-izin-talebi' }"
      >
        Yeni talep
      </v-btn>
    </PageHeader>

    <v-row>
      <v-col v-if="bakiye" cols="12" md="4">
        <v-card title="İzin bakiyem" class="h-100">
          <v-card-text>
            <p class="text-display-small text-primary mb-2">{{ bakiye.kullanilabilirMetni }}</p>
            <p class="text-body-medium text-medium-emphasis mb-4">kullanılabilir</p>
            <dl class="d-flex flex-column ga-1 text-body-medium">
              <div class="d-flex justify-space-between">
                <dt>Toplam hak</dt>
                <dd>{{ bakiye.hakMetni }}</dd>
              </div>
              <div class="d-flex justify-space-between">
                <dt>Kullanılan</dt>
                <dd>{{ bakiye.kullanilanMetni }}</dd>
              </div>
              <div class="d-flex justify-space-between">
                <dt>Onay bekleyen</dt>
                <dd>{{ bakiye.bekleyenMetni }}</dd>
              </div>
            </dl>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col v-if="onaylayabilir" cols="12" :md="bakiye ? 8 : 12">
        <v-card title="Onay bekleyen talepler" class="h-100">
          <v-list v-if="onayBekleyenler.length > 0" role="none">
            <v-list-item
              v-for="talep in onayBekleyenler"
              :key="talep.id"
              :to="{ name: 'izin-talebi', params: { id: talep.id } }"
              :title="`${talep.calisanAdSoyad} · ${talep.izinTuruAdi}`"
              :subtitle="`${talepAraligiMetni(talep)} · ${talep.sureMetni}`"
            />
          </v-list>
          <v-card-text v-else class="text-medium-emphasis">Onay bekleyen talep yok.</v-card-text>
        </v-card>
      </v-col>

      <v-col v-if="calisanId && talepAcabilir" cols="12" :md="onaylayabilir ? 12 : 8">
        <v-card title="Son taleplerim" class="h-100">
          <v-list v-if="sonTaleplerim.length > 0" role="none">
            <v-list-item
              v-for="talep in sonTaleplerim"
              :key="talep.id"
              :to="{ name: 'izin-talebi', params: { id: talep.id } }"
              :title="talep.izinTuruAdi"
              :subtitle="`${talepAraligiMetni(talep)} · ${talep.sureMetni}`"
            >
              <template #append><DurumChip :durum="talep.durum" /></template>
            </v-list-item>
          </v-list>
          <v-card-text v-else class="text-medium-emphasis">Henüz talebiniz yok.</v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
