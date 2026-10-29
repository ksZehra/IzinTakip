import { mdiHome } from '@mdi/js'
import type { RouteRecordRaw } from 'vue-router'

export const anaSayfaRoutes: RouteRecordRaw[] = [
  {
    path: '',
    name: 'ana-sayfa',
    component: () => import('./AnaSayfaPage.vue'),
    meta: { baslik: 'Ana Sayfa', menu: { baslik: 'Ana Sayfa', ikon: mdiHome } },
  },
]
