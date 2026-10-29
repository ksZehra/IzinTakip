<script setup lang="ts">
import { mdiCancel, mdiCheckCircle, mdiClockOutline, mdiCloseCircle } from '@mdi/js'

import type { IzinDurumu } from '@/api/izinTalepleriApi'
import { durumEtiketleri } from '@/common/utils/etiketler'

defineProps<{ durum: IzinDurumu }>()

const gorunum: Record<IzinDurumu, { renk: string; ikon: string }> = {
  Beklemede: { renk: 'warning', ikon: mdiClockOutline },
  Onaylandi: { renk: 'success', ikon: mdiCheckCircle },
  Reddedildi: { renk: 'error', ikon: mdiCloseCircle },
  IptalEdildi: { renk: 'secondary', ikon: mdiCancel },
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
