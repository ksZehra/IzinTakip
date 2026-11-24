<script setup lang="ts">
import { mdiClose, mdiGithub } from '@mdi/js'

import ImzaLogo from '@/common/components/ImzaLogo.vue'
import { TELIF_YILI } from '@/common/utils/uygulamaBilgisi'

const acik = defineModel<boolean>({ required: true })
const surum = __UYGULAMA_SURUMU__
</script>

<template>
  <v-dialog v-model="acik" max-width="340">
    <v-card class="text-center px-6 pt-8 pb-6" rounded="lg" aria-labelledby="hakkinda-baslik">
      <v-btn
        :icon="mdiClose"
        variant="text"
        size="small"
        density="comfortable"
        aria-label="Kapat"
        class="hakkinda-kapat"
        @click="acik = false"
      />
      <ImzaLogo :boyut="84" class="mx-auto" />
      <h2 id="hakkinda-baslik" class="text-title-medium font-weight-semibold mt-4">
        Personel İzin Takip Sistemi
      </h2>
      <p class="hakkinda-surum text-body-small mt-1">Sürüm {{ surum }} · {{ TELIF_YILI }}</p>
      <v-divider class="my-5" />
      <p class="text-body-large font-weight-semibold">Zehra Nur Köse</p>
      <div class="d-flex justify-center mt-1">
        <a
          href="https://github.com/ksZehra"
          target="_blank"
          rel="noopener noreferrer"
          class="hakkinda-baglanti d-flex align-center ga-1"
        >
          <v-icon :icon="mdiGithub" size="18" />
          github.com/ksZehra
        </a>
      </div>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.hakkinda-kapat {
  position: absolute;
  top: 10px;
  right: 10px;
}

.hakkinda-surum,
.hakkinda-baglanti {
  color: #757575;
}

.hakkinda-baglanti .v-icon {
  color: inherit;
}

.hakkinda-baglanti {
  font-size: 13px;
  text-decoration: none;
}

.hakkinda-baglanti:hover,
.hakkinda-baglanti:focus-visible {
  color: rgb(var(--v-theme-primary));
  text-decoration: underline;
}
</style>
