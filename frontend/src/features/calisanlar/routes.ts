import { mdiAccountGroup } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const calisanRoutes: RouteRecordRaw[] = [
  {
    path: 'calisanlar',
    name: 'calisanlar',
    component: () => import('./CalisanlarPage.vue'),
    meta: {
      baslik: 'Çalışanlar',
      yetki: [Yetkiler.CalisanYonet],
      menu: { baslik: 'Çalışanlar', ikon: mdiAccountGroup, grup: 'Yönetim' },
    },
  },
  {
    path: 'calisanlar/:id(\\d+)',
    name: 'calisan',
    component: () => import('./CalisanDetayPage.vue'),
    props: (rota) => ({ id: Number(rota.params.id) }),
    meta: {
      baslik: 'Çalışan',
      yetki: [Yetkiler.CalisanYonet],
    },
  },
]
