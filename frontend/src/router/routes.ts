import type { RouteRecordRaw } from 'vue-router'
import './meta'

import { anaSayfaRoutes } from '@/features/ana-sayfa/routes'
import { ayarlarRoutes } from '@/features/ayarlar/routes'
import { authRoutes } from '@/features/auth/routes'
import { bildirimRoutes } from '@/features/bildirimler/routes'
import { calisanRoutes } from '@/features/calisanlar/routes'
import { departmanRoutes } from '@/features/departmanlar/routes'
import { hakKuraliRoutes } from '@/features/hak-kurallari/routes'
import { hataRoutes } from '@/features/hatalar/routes'
import { izinTalebiRoutes } from '@/features/izin-talepleri/routes'
import { izinTuruRoutes } from '@/features/izin-turleri/routes'
import { raporRoutes } from '@/features/raporlar/routes'
import { resmiTatilRoutes } from '@/features/resmi-tatiller/routes'

export const routes: RouteRecordRaw[] = [
  ...authRoutes,
  {
    path: '/',
    component: () => import('@/layouts/AppLayout.vue'),
    children: [
      ...anaSayfaRoutes,
      ...izinTalebiRoutes,
      ...raporRoutes,
      ...calisanRoutes,
      ...departmanRoutes,
      ...izinTuruRoutes,
      ...resmiTatilRoutes,
      ...hakKuraliRoutes,
      ...ayarlarRoutes,
      ...bildirimRoutes,
      ...hataRoutes,
    ],
  },
]
