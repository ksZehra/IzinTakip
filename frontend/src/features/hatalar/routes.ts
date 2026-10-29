import type { RouteRecordRaw } from 'vue-router'

export const yetkisizRoute: RouteRecordRaw = {
  path: '/yetkisiz',
  name: 'yetkisiz',
  component: () => import('./YetkisizPage.vue'),
  meta: { baslik: 'Yetkiniz Yok' },
}

export const bulunamadiRoute: RouteRecordRaw = {
  path: '/:yol(.*)*',
  name: 'bulunamadi',
  component: () => import('./BulunamadiPage.vue'),
  meta: { baslik: 'Sayfa Bulunamadı' },
}
