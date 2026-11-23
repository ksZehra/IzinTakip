import type { RouteRecordRaw } from 'vue-router'

export const authRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('./LoginPage.vue'),
    meta: { baslik: 'Giriş', herkeseAcik: true },
  },
  {
    path: '/parola-degistir',
    name: 'parola-degistir',
    component: () => import('./ParolaDegistirPage.vue'),
    meta: { baslik: 'Parola Değiştir' },
  },
]
