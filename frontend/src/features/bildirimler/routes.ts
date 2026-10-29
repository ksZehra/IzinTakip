import type { RouteRecordRaw } from 'vue-router'

export const bildirimRoutes: RouteRecordRaw[] = [
  {
    path: 'bildirimler',
    name: 'bildirimler',
    component: () => import('./BildirimlerPage.vue'),
    meta: { baslik: 'Bildirimler' },
  },
]
