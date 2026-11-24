import { PartyPopperIcon } from '@lucide/vue'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const resmiTatilRoutes: RouteRecordRaw[] = [
  {
    path: 'resmi-tatiller',
    name: 'resmi-tatiller',
    component: () => import('./ResmiTatillerPage.vue'),
    meta: {
      baslik: 'Resmi Tatiller',
      yetki: [Yetkiler.TanimYonet],
      menu: { baslik: 'Resmi Tatiller', ikon: PartyPopperIcon, grup: 'Yönetim' },
    },
  },
]
