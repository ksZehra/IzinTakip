import { mdiAccountMultiple } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const kullaniciRoutes: RouteRecordRaw[] = [
  {
    path: 'kullanicilar',
    name: 'kullanicilar',
    component: () => import('./KullanicilarPage.vue'),
    meta: {
      baslik: 'Kullanıcılar',
      yetki: [Yetkiler.KullaniciYonet],
      menu: { baslik: 'Kullanıcılar', ikon: mdiAccountMultiple, grup: 'Yönetim' },
    },
  },
]
