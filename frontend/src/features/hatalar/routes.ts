import type { RouteRecordRaw } from 'vue-router'

export const hataRoutes: RouteRecordRaw[] = [
  {
    path: '/:yol(.*)*',
    name: 'bulunamadi',
    component: () => import('./BulunamadiPage.vue'),
    meta: { baslik: 'Sayfa Bulunamadı' },
  },
]
