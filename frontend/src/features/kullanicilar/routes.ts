import { UserRoundCogIcon } from '@lucide/vue'
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
      menu: { baslik: 'Kullanıcılar', ikon: UserRoundCogIcon, grup: 'Yönetim' },
    },
  },
]
