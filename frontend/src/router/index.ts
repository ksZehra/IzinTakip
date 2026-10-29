import { createRouter, createWebHistory } from 'vue-router'

import { useAuthStore } from '@/stores/authStore'

import { rotaKarari } from './guards'
import { routes } from './routes'

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})

router.beforeEach(async (hedef) => {
  const auth = useAuthStore()
  if (!auth.yuklendi) {
    await auth.oturumuYukle()
  }
  return rotaKarari(hedef, auth.oturum)
})

router.afterEach((hedef) => {
  document.title = `${hedef.meta.baslik} · IzinTakip`
})

export default router
