import type { RouteRecordRaw } from 'vue-router'
import './meta'

import { anaSayfaRoutes } from '@/features/ana-sayfa/routes'
import { authRoutes } from '@/features/auth/routes'
import { bulunamadiRoute, yetkisizRoute } from '@/features/hatalar/routes'

export const routes: RouteRecordRaw[] = [
  ...authRoutes,
  ...anaSayfaRoutes,
  yetkisizRoute,
  bulunamadiRoute,
]
