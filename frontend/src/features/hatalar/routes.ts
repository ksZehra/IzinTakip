import type { RouteRecordRaw } from 'vue-router'

export const hataRoutes: RouteRecordRaw[] = [
  {
    path: 'yetkisiz',
    name: 'yetkisiz',
    component: () => import('./YetkisizPage.vue'),
    meta: { baslik: 'Yetkiniz Yok' },
  },
  {
    path: ':yol(.*)*',
    name: 'bulunamadi',
    component: () => import('./BulunamadiPage.vue'),
    meta: { baslik: 'Sayfa Bulunamadı' },
  },
]
