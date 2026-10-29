import { mdiHistory } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

export const islemKaydiRoutes: RouteRecordRaw[] = [
  {
    path: 'islem-kayitlari',
    name: 'islem-kayitlari',
    component: () => import('./IslemKayitlariPage.vue'),
    meta: {
      baslik: 'İşlem Kayıtları',
      yetki: [Yetkiler.DenetimGor],
      menu: { baslik: 'İşlem Kayıtları', ikon: mdiHistory, grup: 'Yönetim' },
    },
  },
]
