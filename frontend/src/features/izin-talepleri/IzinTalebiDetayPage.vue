<script setup lang="ts">
import { mdiArrowLeft } from '@mdi/js'
import { computed, ref, shallowRef, watch } from 'vue'

import {
  talebiGetir,
  talebiIptalEt,
  talebiOnayla,
  talebiReddet,
  type IzinTalebi,
} from '@/api/izinTalepleriApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import ConfirmDialog from '@/common/components/ConfirmDialog.vue'
import DurumChip from '@/common/components/DurumChip.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { ApiError } from '@/common/http/ApiError'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useToast } from '@/common/composables/useToast'
import { sureTipiEtiketleri } from '@/common/utils/etiketler'
import { tarihSaatGoster } from '@/common/utils/tarih'
import { useAuthStore } from '@/stores/authStore'

import BelgelerCard from './BelgelerCard.vue'
import { talepAraligiMetni } from '@/common/utils/talepAraligi'

type Islem = 'onayla' | 'reddet' | 'iptal'

const props = defineProps<{ id: number }>()

const auth = useAuthStore()
const toast = useToast()
const { gonder, gonderiliyor } = useFormSubmit()
const talep = shallowRef<IzinTalebi | null>(null)
const acikIslem = ref<Islem | null>(null)

const beklemede = computed(() => talep.value?.durum === 'Beklemede')
const kendiTalebi = computed(() => talep.value?.calisanId === auth.oturum?.calisanId)
const superAdmin = computed(() => auth.oturum?.roller.includes('SuperAdmin') ?? false)
const kararVerebilir = computed(
  () =>
    beklemede.value &&
    auth.yetkiVar(Yetkiler.IzinTalebiYonet) &&
    (!kendiTalebi.value || superAdmin.value),
)
const iptalEdilebilir = computed(
  () => talep.value?.durum === 'Beklemede' || talep.value?.durum === 'Onaylandi',
)

const islemler: Record<
  Islem,
  {
    baslik: string
    metin: string
    onayMetni: string
    renk: string
    notEtiketi: string
    notZorunlu: boolean
    calistir: (id: number, not: string | null) => Promise<IzinTalebi>
    basari: string
  }
> = {
  onayla: {
    baslik: 'Talebi onayla',
    metin: 'Talep onaylanacak ve talep sahibine bildirim gidecek.',
    onayMetni: 'Onayla',
    renk: 'success',
    notEtiketi: 'Not',
    notZorunlu: false,
    calistir: talebiOnayla,
    basari: 'Talep onaylandı.',
  },
  reddet: {
    baslik: 'Talebi reddet',
    metin: 'Red gerekçesi talep sahibine iletilecek.',
    onayMetni: 'Reddet',
    renk: 'error',
    notEtiketi: 'Red gerekçesi',
    notZorunlu: true,
    calistir: (id, not) => talebiReddet(id, not ?? ''),
    basari: 'Talep reddedildi.',
  },
  iptal: {
    baslik: 'Talebi iptal et',
    metin: 'İptal edilen talebin süresi bakiyeye geri döner.',
    onayMetni: 'İptal et',
    renk: 'error',
    notEtiketi: 'Not',
    notZorunlu: false,
    calistir: talebiIptalEt,
    basari: 'Talep iptal edildi.',
  },
}

const dialogAcik = computed({
  get: () => acikIslem.value !== null,
  set: (acik) => {
    if (!acik) acikIslem.value = null
  },
})
const secilenIslem = computed(() => (acikIslem.value ? islemler[acikIslem.value] : null))

const yuklenemedi = ref(false)

async function yukle() {
  yuklenemedi.value = false
  try {
    talep.value = await talebiGetir(props.id)
  } catch (hata) {
    if (!(hata instanceof ApiError)) throw hata
    yuklenemedi.value = true
  }
}

async function islemiCalistir(not: string | null) {
  const islem = secilenIslem.value
  if (!islem) return
  const guncel = await gonder(() => islem.calistir(props.id, not))
  if (!guncel) return
  talep.value = guncel
  acikIslem.value = null
  toast.basariGoster(islem.basari)
}

watch(() => props.id, yukle, { immediate: true })
</script>

<template>
  <v-container style="max-width: 960px">
    <PageHeader baslik="İzin Talebi" :aciklama="talep ? `#${talep.id}` : undefined">
      <v-btn variant="text" :prepend-icon="mdiArrowLeft" :to="{ name: 'izin-talepleri' }">
        Talepler
      </v-btn>
    </PageHeader>

    <v-alert v-if="yuklenemedi" type="warning" variant="tonal">
      Kayıt açılamadı. Silinmiş ya da görme yetkiniz olmayabilir. Talep listesinden tekrar seçin.
    </v-alert>
    <v-skeleton-loader v-else-if="!talep" type="article" />

    <div v-else class="d-flex flex-column ga-4">
      <v-card>
        <v-card-text>
          <dl class="ozellikler">
            <dt>Durum</dt>
            <dd><DurumChip :durum="talep.durum" /></dd>
            <dt>Çalışan</dt>
            <dd>{{ talep.calisanAdSoyad }} · {{ talep.departmanAdi }}</dd>
            <dt>İzin türü</dt>
            <dd>{{ talep.izinTuruAdi }} ({{ sureTipiEtiketleri[talep.sureTipi] }})</dd>
            <dt>Tarih</dt>
            <dd>{{ talepAraligiMetni(talep) }}</dd>
            <dt>Süre</dt>
            <dd>{{ talep.sureMetni }}</dd>
            <dt v-if="talep.aciklama">Açıklama</dt>
            <dd v-if="talep.aciklama">{{ talep.aciklama }}</dd>
            <dt>Talebi açan</dt>
            <dd>{{ talep.olusturanAdi ?? '—' }} · {{ tarihSaatGoster(talep.olusturmaTarihi) }}</dd>
            <template v-if="talep.kararTarihi">
              <dt>Son işlem</dt>
              <dd>{{ talep.kararVerenAdi ?? '—' }} · {{ tarihSaatGoster(talep.kararTarihi) }}</dd>
            </template>
            <template v-if="talep.kararNotu">
              <dt>Not</dt>
              <dd>{{ talep.kararNotu }}</dd>
            </template>
          </dl>
        </v-card-text>
        <v-card-actions v-if="kararVerebilir || iptalEdilebilir" class="flex-wrap ga-2 px-4 pb-4">
          <v-btn v-if="kararVerebilir" color="success" variant="flat" @click="acikIslem = 'onayla'">
            Onayla
          </v-btn>
          <v-btn v-if="kararVerebilir" color="error" variant="tonal" @click="acikIslem = 'reddet'">
            Reddet
          </v-btn>
          <v-btn v-if="iptalEdilebilir" variant="text" @click="acikIslem = 'iptal'">
            İptal et
          </v-btn>
        </v-card-actions>
      </v-card>

      <BelgelerCard :talep-id="talep.id" :degistirilebilir="beklemede" />
    </div>

    <ConfirmDialog
      v-if="secilenIslem"
      v-model="dialogAcik"
      :baslik="secilenIslem.baslik"
      :metin="secilenIslem.metin"
      :onay-metni="secilenIslem.onayMetni"
      :onay-rengi="secilenIslem.renk"
      :not-etiketi="secilenIslem.notEtiketi"
      :not-zorunlu="secilenIslem.notZorunlu"
      :calisiyor="gonderiliyor"
      @onay="islemiCalistir"
    />
  </v-container>
</template>

<style scoped>
.ozellikler {
  display: grid;
  grid-template-columns: minmax(120px, max-content) 1fr;
  gap: 12px 24px;
  align-items: center;
}

.ozellikler dt {
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.ozellikler dd {
  margin: 0;
}

@media (max-width: 599px) {
  .ozellikler {
    grid-template-columns: 1fr;
    gap: 4px;
  }

  .ozellikler dd {
    margin-bottom: 8px;
  }
}
</style>
