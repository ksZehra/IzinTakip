import { ShieldCheckIcon } from '@lucide/vue'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const rolRoutes: RouteRecordRaw[] = [
  {
    path: 'roller',
    name: 'roller',
    component: () => import('./RollerPage.vue'),
    meta: {
      baslik: 'Roller',
      yetki: [Yetkiler.RolYonetimi],
      menu: { baslik: 'Roller', ikon: ShieldCheckIcon, grup: 'Yönetim' },
    },
  },
]
