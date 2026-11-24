import { SettingsIcon } from '@lucide/vue'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const ayarlarRoutes: RouteRecordRaw[] = [
  {
    path: 'ayarlar',
    name: 'ayarlar',
    component: () => import('./AyarlarPage.vue'),
    meta: {
      baslik: 'İş Takvimi Ayarları',
      yetki: [Yetkiler.TanimYonet],
      menu: { baslik: 'Ayarlar', ikon: SettingsIcon, grup: 'Yönetim' },
    },
  },
]
