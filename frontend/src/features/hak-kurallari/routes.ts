import { mdiScaleBalance } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const hakKuraliRoutes: RouteRecordRaw[] = [
  {
    path: 'hak-kurallari',
    name: 'hak-kurallari',
    component: () => import('./HakKurallariPage.vue'),
    meta: {
      baslik: 'Hak Kuralları',
      yetki: [Yetkiler.TanimYonet],
      menu: { baslik: 'Hak Kuralları', ikon: mdiScaleBalance, grup: 'Yönetim' },
    },
  },
]
