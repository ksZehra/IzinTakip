import { mdiShapeOutline } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const izinTuruRoutes: RouteRecordRaw[] = [
  {
    path: 'izin-turleri',
    name: 'izin-turleri',
    component: () => import('./IzinTurleriPage.vue'),
    meta: {
      baslik: 'İzin Türleri',
      yetki: [Yetkiler.TanimYonet],
      menu: { baslik: 'İzin Türleri', ikon: mdiShapeOutline, grup: 'Yönetim' },
    },
  },
]
