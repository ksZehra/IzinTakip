import { mdiCalendarCheck } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler } from '@/common/auth/yetkiler'

const yetki = [Yetkiler.IzinTalebiOlustur, Yetkiler.IzinTalebiYonet]

export const izinTalebiRoutes: RouteRecordRaw[] = [
  {
    path: 'izin-talepleri',
    name: 'izin-talepleri',
    component: () => import('./IzinTalepleriPage.vue'),
    meta: {
      baslik: 'İzin Talepleri',
      yetki,
      menu: { baslik: 'İzin Talepleri', ikon: mdiCalendarCheck },
    },
  },
  {
    path: 'izin-talepleri/yeni',
    name: 'yeni-izin-talebi',
    component: () => import('./YeniTalepPage.vue'),
    meta: { baslik: 'Yeni İzin Talebi', yetki },
  },
  {
    path: 'izin-talepleri/:id(\\d+)',
    name: 'izin-talebi',
    component: () => import('./IzinTalebiDetayPage.vue'),
    props: (rota) => ({ id: Number(rota.params.id) }),
    meta: { baslik: 'İzin Talebi', yetki },
  },
]
