<script setup lang="ts">
import { CheckIcon, MonitorIcon, MoonIcon, SunIcon } from '@lucide/vue'
import { ref } from 'vue'
import type { Ikon } from '@/common/utils/ikon'
import { useTheme } from 'vuetify'

import { kayitliTema, temaKaydet, type TemaSecimi } from '@/common/utils/tema'

const tema = useTheme()
const secim = ref<TemaSecimi>(kayitliTema())

const secenekler: { deger: TemaSecimi; baslik: string; ikon: Ikon }[] = [
  { deger: 'light', baslik: 'Açık', ikon: SunIcon },
  { deger: 'dark', baslik: 'Koyu', ikon: MoonIcon },
  { deger: 'system', baslik: 'Sistem', ikon: MonitorIcon },
]

function sec(deger: TemaSecimi) {
  secim.value = deger
  void tema.change(deger)
  temaKaydet(deger)
}
</script>

<template>
  <v-menu location="bottom end">
    <template #activator="{ props }">
      <v-btn
        v-bind="props"
        icon
        variant="text"
        aria-label="Tema seçimi"
        :aria-description="`Geçerli tema: ${secenekler.find((s) => s.deger === secim)?.baslik}`"
      >
        <v-icon :icon="secim === 'system' ? MonitorIcon : secim === 'dark' ? MoonIcon : SunIcon" />
      </v-btn>
    </template>
    <v-list density="compact" role="none">
      <v-list-item
        v-for="secenek in secenekler"
        :key="secenek.deger"
        :prepend-icon="secenek.ikon"
        :title="secenek.baslik"
        :active="secim === secenek.deger"
        color="primary"
        @click="sec(secenek.deger)"
      >
        <template v-if="secim === secenek.deger" #append>
          <v-icon :icon="CheckIcon" size="small" />
        </template>
      </v-list-item>
    </v-list>
  </v-menu>
</template>
