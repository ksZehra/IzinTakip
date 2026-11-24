<script setup lang="ts">
import { BanIcon, CircleCheckIcon, ClockIcon, CircleXIcon } from '@lucide/vue'

import type { Ikon } from '@/common/utils/ikon'

import type { IzinDurumu } from '@/api/izinTalepleriApi'
import { durumEtiketleri } from '@/common/utils/etiketler'

defineProps<{ durum: IzinDurumu }>()

const gorunum: Record<IzinDurumu, { renk: string; ikon: Ikon }> = {
  Beklemede: { renk: 'warning', ikon: ClockIcon },
  Onaylandi: { renk: 'success', ikon: CircleCheckIcon },
  Reddedildi: { renk: 'error', ikon: CircleXIcon },
  IptalEdildi: { renk: 'secondary', ikon: BanIcon },
}
</script>

<template>
  <v-chip
    :color="gorunum[durum].renk"
    :prepend-icon="gorunum[durum].ikon"
    size="small"
    variant="tonal"
    :text="durumEtiketleri[durum]"
  />
</template>
