import { mdiCalendarMonth, mdiChartBox } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const raporRoutes: RouteRecordRaw[] = [
  {
    path: 'takvim',
    name: 'takvim',
    component: () => import('./TakvimPage.vue'),
    meta: {
      baslik: 'İzin Takvimi',
      yetki: [Yetkiler.IzinTalebiOlustur, Yetkiler.IzinTalebiYonet],
      menu: { baslik: 'Takvim', ikon: mdiCalendarMonth },
    },
  },
  {
    path: 'raporlar/bakiyeler',
    name: 'bakiye-raporu',
    component: () => import('./BakiyeRaporuPage.vue'),
    meta: {
      baslik: 'Bakiye Raporu',
      yetki: [Yetkiler.RaporGor],
      menu: { baslik: 'Bakiye Raporu', ikon: mdiChartBox },
    },
  },
]
