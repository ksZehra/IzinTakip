import type { RouteRecordRaw } from 'vue-router'

export const hataRoutes: RouteRecordRaw[] = [
  {
    path: 'yetkisiz',
    name: 'yetkisiz',
    component: () => import('./ForbiddenPage.vue'),
    meta: { baslik: 'Yetkiniz Yok' },
  },
  {
    path: ':yol(.*)*',
    name: 'bulunamadi',
    component: () => import('./NotFoundPage.vue'),
    meta: { baslik: 'Sayfa Bulunamadı' },
  },
]
