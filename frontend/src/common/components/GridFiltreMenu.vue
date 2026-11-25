<script setup lang="ts">
import { FunnelIcon } from '@lucide/vue'
import { computed, ref, watch } from 'vue'

import type { FilterValue } from '@/common/composables/usePagedList'

import {
  bosDegerler,
  filtreAnahtarlari,
  filtreDoluMu,
  tarihAraligi,
  tarihDurumu,
  tarihHatasi,
  tarihOperatorSecenekleri,
  tarihOzeti,
  type GridFiltre,
  type TarihOperatoru,
} from './dataGridTipleri'

const props = defineProps<{
  baslik: string
  tanim: GridFiltre
  filtre: Record<string, FilterValue>
  tarihOperatoru?: TarihOperatoru
  etiketli?: boolean
}>()

const emit = defineEmits<{
  degistir: [anahtar: string, deger: FilterValue]
  operator: [operator: TarihOperatoru | null]
}>()

const acik = ref(false)
const metinAlani = ref<{ focus: () => void } | null>(null)
const metin = ref('')
const secili = ref<(string | number)[]>([])
const arama = ref('')
const operator = ref<TarihOperatoru>('eq')
const tarihA = ref('')
const tarihB = ref('')

const dolu = computed(() =>
  filtreAnahtarlari(props.tanim).some((anahtar) => filtreDoluMu(props.filtre[anahtar])),
)

const coklu = computed(() => (props.tanim.tur === 'cokluSecim' ? props.tanim.secenekler : []))
const gorunenSecenekler = computed(() =>
  coklu.value.filter((s) =>
    s.title.toLocaleLowerCase('tr').includes(arama.value.toLocaleLowerCase('tr')),
  ),
)
const hepsiSecili = computed(
  () => coklu.value.length > 0 && secili.value.length === coklu.value.length,
)
const hata = computed(() => tarihHatasi(operator.value, tarihA.value, tarihB.value))
const ozet = computed(() =>
  tarihOzeti({ operator: operator.value, a: tarihA.value, b: tarihB.value }),
)

watch(acik, (acildi) => {
  if (!acildi) return
  const tanim = props.tanim
  if (tanim.tur === 'metin') {
    metin.value = String(props.filtre[tanim.anahtar] ?? '')
  } else if (tanim.tur === 'cokluSecim') {
    const deger = props.filtre[tanim.anahtar]
    secili.value = Array.isArray(deger) ? [...deger] : []
    arama.value = ''
  } else if (tanim.tur === 'tarih') {
    const durum = tarihDurumu(
      props.tarihOperatoru,
      String(props.filtre[tanim.baslangicAnahtari] ?? ''),
      String(props.filtre[tanim.bitisAnahtari] ?? ''),
    )
    operator.value = durum.operator
    tarihA.value = durum.a
    tarihB.value = durum.b
  }
})

function secimiDegistir(deger: string | number) {
  secili.value = secili.value.includes(deger)
    ? secili.value.filter((d) => d !== deger)
    : [...secili.value, deger]
}

function tumunuDegistir(secildi: boolean | null) {
  secili.value = secildi ? coklu.value.map((s) => s.value) : []
}

function tekliSec(deger: string | number | boolean | null) {
  if (props.tanim.tur !== 'tekliSecim') return
  emit('degistir', props.tanim.anahtar, deger)
  acik.value = false
}

function uygula() {
  const tanim = props.tanim
  if (tanim.tur === 'metin') {
    emit('degistir', tanim.anahtar, metin.value.trim())
  } else if (tanim.tur === 'cokluSecim') {
    emit('degistir', tanim.anahtar, [...secili.value])
  } else if (tanim.tur === 'tarih') {
    if (hata.value) return
    const aralik = tarihAraligi(operator.value, tarihA.value, tarihB.value)
    emit('degistir', tanim.baslangicAnahtari, aralik.baslangic)
    emit('degistir', tanim.bitisAnahtari, aralik.bitis)
    emit('operator', aralik.baslangic || aralik.bitis ? operator.value : null)
  }
  acik.value = false
}

function temizle() {
  for (const [anahtar, deger] of bosDegerler(props.tanim)) emit('degistir', anahtar, deger)
  emit('operator', null)
  acik.value = false
}
</script>

<template>
  <v-menu
    v-model="acik"
    :close-on-content-click="false"
    location="bottom start"
    @after-enter="metinAlani?.focus()"
  >
    <template #activator="{ props: menuOzellikleri }">
      <v-btn
        v-if="etiketli"
        v-bind="menuOzellikleri"
        size="small"
        :variant="dolu ? 'tonal' : 'outlined'"
        :color="dolu ? 'primary' : undefined"
        :prepend-icon="FunnelIcon"
      >
        {{ baslik }}
      </v-btn>
      <v-btn
        v-else
        v-bind="menuOzellikleri"
        icon
        size="24"
        class="grid-filtre-dugmesi"
        :class="{ 'grid-filtre-dugmesi--dolu': dolu }"
        :variant="dolu ? 'tonal' : 'text'"
        :color="dolu ? 'primary' : undefined"
        :aria-label="`${baslik} filtresi`"
      >
        <FunnelIcon :size="14" :stroke-width="2" :fill="dolu ? 'currentColor' : 'none'" />
      </v-btn>
    </template>

    <v-card width="280">
      <v-list
        v-if="tanim.tur === 'tekliSecim'"
        density="compact"
        role="radiogroup"
        :aria-label="`${baslik} filtresi`"
      >
        <v-list-item
          role="radio"
          :aria-checked="!filtreDoluMu(filtre[tanim.anahtar])"
          :active="!filtreDoluMu(filtre[tanim.anahtar])"
          title="Tümü"
          @click="tekliSec(null)"
        />
        <v-list-item
          v-for="secenek in tanim.secenekler"
          :key="String(secenek.value)"
          role="radio"
          :aria-checked="filtre[tanim.anahtar] === secenek.value"
          :active="filtre[tanim.anahtar] === secenek.value"
          :title="secenek.title"
          @click="tekliSec(secenek.value)"
        />
      </v-list>

      <template v-else>
        <v-card-text class="d-flex flex-column ga-3 pb-0">
          <v-text-field
            v-if="tanim.tur === 'metin'"
            ref="metinAlani"
            v-model="metin"
            :aria-label="`${baslik} filtre metni`"
            placeholder="Filtrelemek için yazın…"
            density="compact"
            hide-details
            clearable
            @keydown.enter.prevent="uygula"
          />

          <template v-else-if="tanim.tur === 'cokluSecim'">
            <v-text-field
              v-model="arama"
              :aria-label="`${baslik} listesinde ara`"
              placeholder="Listede ara…"
              density="compact"
              hide-details
              clearable
            />
            <div class="grid-liste">
              <v-checkbox
                label="(Tümünü Seç)"
                density="compact"
                hide-details
                :model-value="hepsiSecili"
                :indeterminate="secili.length > 0 && !hepsiSecili"
                @update:model-value="tumunuDegistir"
              />
              <v-checkbox
                v-for="secenek in gorunenSecenekler"
                :key="secenek.value"
                :label="secenek.title"
                density="compact"
                hide-details
                :model-value="secili.includes(secenek.value)"
                @update:model-value="secimiDegistir(secenek.value)"
              />
            </div>
          </template>

          <template v-else>
            <v-select
              v-model="operator"
              :items="tarihOperatorSecenekleri"
              label="Koşul"
              density="compact"
              hide-details
            />
            <v-text-field
              v-model="tarihA"
              :label="operator === 'bw' ? 'Başlangıç' : 'Tarih'"
              type="date"
              density="compact"
              hide-details
            />
            <v-text-field
              v-if="operator === 'bw'"
              v-model="tarihB"
              label="Bitiş"
              type="date"
              density="compact"
              hide-details
            />
            <p class="text-body-small" :class="hata ? 'text-error' : 'text-medium-emphasis'">
              {{
                hata ||
                (ozet ? `Listelenecek: ${ozet}` : 'Tarih seçilmedi, tüm kayıtlar listelenir.')
              }}
            </p>
          </template>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="tonal" @click="temizle">Temizle</v-btn>
          <v-btn color="primary" variant="flat" :disabled="Boolean(hata)" @click="uygula"
            >Uygula</v-btn
          >
        </v-card-actions>
      </template>
    </v-card>
  </v-menu>
</template>

<style scoped>
.grid-filtre-dugmesi {
  border-radius: 6px;
  opacity: 0.45;
  transition: opacity 0.15s;
}

.grid-filtre-dugmesi:hover,
.grid-filtre-dugmesi:focus-visible,
.grid-filtre-dugmesi[aria-expanded='true'],
.grid-filtre-dugmesi--dolu {
  opacity: 1;
}

.grid-liste {
  max-height: 220px;
  overflow-y: auto;
}
</style>
