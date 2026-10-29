import type { RouteRecordRaw } from 'vue-router'

export const authRoutes: RouteRecordRaw[] = [
  {
    path: '/giris',
    name: 'giris',
    component: () => import('./GirisPage.vue'),
    meta: { baslik: 'Giriş', herkeseAcik: true },
  },
  {
    path: '/parola-degistir',
    name: 'parola-degistir',
    component: () => import('./ParolaDegistirPage.vue'),
    meta: { baslik: 'Parola Değiştir' },
  },
]
