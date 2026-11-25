<script setup lang="ts">
import { AwardIcon, HourglassIcon, PalmtreeIcon, PlaneTakeoffIcon, PlusIcon } from '@lucide/vue'
import { computed, onMounted, shallowRef } from 'vue'

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

onMounted(async () => {
  const [bakiyeSonucu, taleplerim, bekleyenler] = await Promise.all([
    calisanId && talepAcabilir ? bakiyeGetir(calisanId) : null,
    calisanId && talepAcabilir ? talepleriGetir({ calisanId, page: 1, pageSize: 5 }) : null,
    onaylayabilir ? talepleriGetir({ durum: ['Beklemede'], page: 1, pageSize: 5 }) : null,
  ])
  bakiye.value = bakiyeSonucu
  sonTaleplerim.value = taleplerim?.items ?? []
  onayBekleyenler.value = (bekleyenler?.items ?? []).filter((t) => t.calisanId !== calisanId)
})

const ozetKartlari = computed(() =>
  bakiye.value
    ? [
        {
          etiket: 'Kullanılabilir izin',
          deger: bakiye.value.kullanilabilirMetni,
          ikon: PalmtreeIcon,
          renk: 'primary',
        },
        { etiket: 'Toplam hak', deger: bakiye.value.hakMetni, ikon: AwardIcon, renk: 'violet' },
        {
          etiket: 'Kullanılan',
          deger: bakiye.value.kullanilanMetni,
          ikon: PlaneTakeoffIcon,
          renk: 'pink',
        },
        {
          etiket: 'Onay bekleyen',
          deger: bakiye.value.bekleyenMetni,
          ikon: HourglassIcon,
          renk: 'amber',
        },
      ]
    : [],
)
</script>

<template>
  <v-container>
    <PageHeader baslik="Ana Sayfa" :aciklama="`Hoş geldiniz, ${auth.oturum?.gorunenAd ?? ''}.`">
      <v-btn
        v-if="talepAcabilir"
        color="primary"
        :prepend-icon="PlusIcon"
        :to="{ name: 'yeni-izin-talebi' }"
      >
        Yeni talep
      </v-btn>
    </PageHeader>

    <v-row>
      <v-col v-for="kart in ozetKartlari" :key="kart.etiket" cols="12" sm="6" md="3">
        <v-card class="h-100 pa-5">
          <v-avatar :color="kart.renk" variant="tonal" rounded="lg" size="40">
            <v-icon :icon="kart.ikon" size="22" />
          </v-avatar>
          <p class="text-body-medium text-medium-emphasis mt-4">{{ kart.etiket }}</p>
          <p class="text-headline-small mt-1">{{ kart.deger }}</p>
        </v-card>
      </v-col>

      <v-col v-if="onaylayabilir" cols="12">
        <v-card title="Onay bekleyen talepler" class="h-100">
          <v-list v-if="onayBekleyenler.length > 0" role="none">
            <v-list-item
              v-for="talep in onayBekleyenler"
              :key="talep.id"
              :to="{ name: 'izin-talebi', params: { id: talep.id } }"
              :title="`${talep.calisanAdSoyad} · ${talep.izinTuruAdi}`"
              :subtitle="`${talepAraligiMetni(talep)} · ${talep.sureMetni}`"
            >
              <template #prepend>
                <v-avatar color="primary" variant="tonal" size="36" class="me-3">
                  {{ talep.calisanAdSoyad.charAt(0) }}
                </v-avatar>
              </template>
            </v-list-item>
          </v-list>
          <v-card-text v-else class="text-medium-emphasis">Onay bekleyen talep yok.</v-card-text>
        </v-card>
      </v-col>

      <v-col v-if="calisanId && talepAcabilir" cols="12">
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
