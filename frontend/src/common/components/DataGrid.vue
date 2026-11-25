<script setup lang="ts" generic="Satir extends Record<string, any>">
import { FunnelIcon, FunnelXIcon } from '@lucide/vue'
import { computed, reactive, ref } from 'vue'
import { useDisplay } from 'vuetify'

import type { FilterValue } from '@/common/composables/usePagedList'

import {
  bosDegerler,
  doluFiltreSayisi,
  etkinFiltreler,
  type GridKolon,
  type TarihOperatoru,
  yerelFiltrele,
  yerelSirala,
} from './dataGridTipleri'
import GridFiltreMenu from './GridFiltreMenu.vue'

const props = defineProps<{
  kolonlar: GridKolon<Satir>[]
  satirlar: Satir[]
  toplam?: number
  yukleniyor?: boolean
  filtre: Record<string, FilterValue>
  sifirlanabilir?: boolean
  bosMetin?: string
  satirTiklanir?: boolean
  // Yerel modda bütün satırlar elde olur; süzme ve sıralama tarayıcıda yapılır, sayfalama gösterilmez.
  yerel?: boolean
}>()

const emit = defineEmits<{
  filtreDegisti: [anahtar: string, deger: FilterValue]
  sifirla: []
  satirTikla: [satir: Satir]
}>()

const sayfa = defineModel<number>('sayfa', { default: 1 })
const sayfaBoyutu = defineModel<number>('sayfaBoyutu', { default: 20 })
const sirala = defineModel<string>('sirala', { default: '' })

const { xs } = useDisplay()
const filtrePaneliAcik = ref(false)
const operatorler = reactive<Record<string, TarihOperatoru | undefined>>({})

const filtreliKolonlar = computed(() => props.kolonlar.filter((kolon) => kolon.filtre))
const doluSayisi = computed(() => doluFiltreSayisi(props.kolonlar, props.filtre))
const etiketler = computed(() => etkinFiltreler(props.kolonlar, props.filtre, operatorler))

const gorunenSatirlar = computed(() =>
  props.yerel
    ? yerelSirala(
        props.kolonlar,
        yerelFiltrele(props.kolonlar, props.satirlar, props.filtre),
        sirala.value,
      )
    : props.satirlar,
)
const satirSayisi = computed(() =>
  props.yerel ? gorunenSatirlar.value.length : (props.toplam ?? 0),
)

const basliklar = computed(() =>
  props.kolonlar.map((kolon) => ({
    key: kolon.key,
    title: kolon.title,
    value: kolon.deger,
    sortable: kolon.sirala === true,
    align: kolon.hizala,
  })),
)

const siralama = computed<{ key: string; order: 'asc' | 'desc' }[]>(() => {
  const [key, yon] = sirala.value.split(':')
  return key && (yon === 'asc' || yon === 'desc') ? [{ key, order: yon }] : []
})

function siralamaDegisti(secim: { key: string; order?: boolean | 'asc' | 'desc' }[]) {
  const ilk = secim[0]
  sirala.value = ilk ? `${ilk.key}:${ilk.order === 'desc' ? 'desc' : 'asc'}` : ''
}

function sayfaBoyutuDegisti(boyut: number) {
  if (!props.yerel) sayfaBoyutu.value = boyut
}

function operatoruAyarla(kolonKey: string, operator: TarihOperatoru | null) {
  operatorler[kolonKey] = operator ?? undefined
}

function filtreyiKaldir(kolonKey: string) {
  const tanim = props.kolonlar.find((kolon) => kolon.key === kolonKey)?.filtre
  if (!tanim) return
  for (const [anahtar, deger] of bosDegerler(tanim)) emit('filtreDegisti', anahtar, deger)
  operatoruAyarla(kolonKey, null)
}

function sifirla() {
  for (const anahtar of Object.keys(operatorler)) delete operatorler[anahtar]
  emit('sifirla')
}

function satirTiklandi(_olay: unknown, { item }: { item: Satir }) {
  if (props.satirTiklanir) emit('satirTikla', item)
}
</script>

<template>
  <div class="data-grid">
    <div v-if="filtreliKolonlar.length" class="d-flex flex-wrap align-center ga-2 mb-2">
      <v-btn
        v-if="xs"
        variant="tonal"
        size="small"
        :prepend-icon="FunnelIcon"
        :aria-expanded="filtrePaneliAcik"
        @click="filtrePaneliAcik = !filtrePaneliAcik"
      >
        Filtreler{{ doluSayisi ? ` (${doluSayisi})` : '' }}
      </v-btn>
      <v-chip
        v-for="etiket in etiketler"
        :key="etiket.kolonKey"
        closable
        size="small"
        color="primary"
        variant="tonal"
        :close-label="`${etiket.metin} filtresini kaldır`"
        @click:close="filtreyiKaldir(etiket.kolonKey)"
      >
        {{ etiket.metin }}
      </v-chip>
      <v-btn
        v-if="sifirlanabilir"
        variant="text"
        size="small"
        class="grid-sifirla-dugmesi"
        @click="sifirla"
      >
        <FunnelXIcon :size="14" class="mr-1" />
        Filtreleri Sıfırla
      </v-btn>
    </div>

    <v-card v-if="xs && filtrePaneliAcik" class="mb-2">
      <v-card-text class="d-flex flex-wrap ga-2">
        <GridFiltreMenu
          v-for="kolon in filtreliKolonlar"
          :key="kolon.key"
          :tanim="kolon.filtre!"
          :baslik="kolon.title"
          :filtre="filtre"
          :tarih-operatoru="operatorler[kolon.key]"
          etiketli
          @degistir="(anahtar, deger) => emit('filtreDegisti', anahtar, deger)"
          @operator="(operator) => operatoruAyarla(kolon.key, operator)"
        />
      </v-card-text>
    </v-card>

    <v-card>
      <v-data-table-server
        v-model:page="sayfa"
        :items-per-page="yerel ? -1 : sayfaBoyutu"
        :sort-by="siralama"
        :headers="basliklar"
        :items="gorunenSatirlar"
        :items-length="satirSayisi"
        :loading="yukleniyor"
        :hide-default-footer="yerel"
        :items-per-page-options="[10, 20, 50, 100]"
        :row-props="satirTiklanir ? { class: 'cursor-pointer' } : undefined"
        :hover="satirTiklanir"
        :no-data-text="bosMetin ?? 'Kayıt bulunamadı.'"
        @update:items-per-page="sayfaBoyutuDegisti"
        @update:sort-by="siralamaDegisti"
        @click:row="satirTiklandi"
      >
        <template
          v-for="kolon in kolonlar"
          :key="kolon.key"
          #[`header.${kolon.key}`]="{ column, getSortIcon }"
        >
          <template v-if="xs">{{ kolon.title }}</template>
          <div v-else class="v-data-table-header__content">
            <span>{{ kolon.title }}</span>
            <v-icon
              v-if="column.sortable"
              class="v-data-table-header__sort-icon grid-sirala-ikonu"
              size="14"
              :icon="getSortIcon(column)"
            />
            <span v-if="kolon.filtre" class="ml-1" @click.stop @keydown.stop>
              <GridFiltreMenu
                :tanim="kolon.filtre"
                :baslik="kolon.title"
                :filtre="filtre"
                :tarih-operatoru="operatorler[kolon.key]"
                @degistir="(anahtar, deger) => emit('filtreDegisti', anahtar, deger)"
                @operator="(operator) => operatoruAyarla(kolon.key, operator)"
              />
            </span>
          </div>
        </template>
        <template v-for="(_, ad) in $slots" :key="ad" #[ad]="slotProps">
          <slot :name="ad" v-bind="slotProps ?? {}" />
        </template>
      </v-data-table-server>
    </v-card>
  </div>
</template>

<style scoped>
.grid-sirala-ikonu {
  width: 14px;
  height: 14px;
  margin-inline-start: 4px;
}

.grid-sirala-ikonu :deep(svg) {
  width: 14px;
  height: 14px;
  stroke-width: 2;
}

.grid-sifirla-dugmesi {
  font-size: 0.8125rem;
  font-weight: 500;
  letter-spacing: normal;
  text-transform: none;
  color: rgba(var(--v-theme-on-surface), var(--v-medium-emphasis-opacity));
}

.grid-sifirla-dugmesi:hover {
  color: rgb(var(--v-theme-primary));
}
</style>
