import type { RouteRecordRaw } from 'vue-router'
import './meta'

import { anaSayfaRoutes } from '@/features/ana-sayfa/routes'
import { authRoutes } from '@/features/auth/routes'
import { bildirimRoutes } from '@/features/bildirimler/routes'
import { hataRoutes } from '@/features/hatalar/routes'
import { izinTalebiRoutes } from '@/features/izin-talepleri/routes'
import { raporRoutes } from '@/features/raporlar/routes'

export const routes: RouteRecordRaw[] = [
  ...authRoutes,
  {
    path: '/',
    component: () => import('@/layouts/AppLayout.vue'),
    children: [
      ...anaSayfaRoutes,
      ...izinTalebiRoutes,
      ...raporRoutes,
      ...bildirimRoutes,
      ...hataRoutes,
    ],
  },
]
