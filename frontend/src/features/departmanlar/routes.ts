import { Building2Icon } from '@lucide/vue'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const departmanRoutes: RouteRecordRaw[] = [
  {
    path: 'departmanlar',
    name: 'departmanlar',
    component: () => import('./DepartmanlarPage.vue'),
    meta: {
      baslik: 'Departmanlar',
      yetki: [Yetkiler.TanimYonet],
      menu: { baslik: 'Departmanlar', ikon: Building2Icon, grup: 'Yönetim' },
    },
  },
]
