import type { RouteRecordRaw } from 'vue-router'

import { anaSayfaRoutes } from '@/features/ana-sayfa/routes'
import { hataRoutes } from '@/features/hatalar/routes'

export const routes: RouteRecordRaw[] = [...anaSayfaRoutes, ...hataRoutes]
