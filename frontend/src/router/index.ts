import { createRouter, createWebHistory } from 'vue-router'

import { routes } from './routes'

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})

router.afterEach((hedef) => {
  document.title = `${hedef.meta.baslik} · IzinTakip`
})

export default router
