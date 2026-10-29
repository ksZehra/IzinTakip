import { createPinia } from 'pinia'
import { createApp } from 'vue'

import App from '@/App.vue'
import { useToast } from '@/common/composables/useToast'
import { oturumDustugundeCalistir } from '@/common/http/httpClient'
import { createAppVuetify } from '@/plugins/vuetify'
import router from '@/router'
import { useAuthStore } from '@/stores/authStore'

const app = createApp(App).use(createPinia()).use(router).use(createAppVuetify())

oturumDustugundeCalistir(() => {
  const auth = useAuthStore()
  if (!auth.girisYapildi) {
    return
  }
  auth.temizle()
  useToast().bilgiGoster('Oturumunuz sona erdi. Devam etmek için tekrar giriş yapın.')
  void router.push({ name: 'giris', query: { donus: router.currentRoute.value.fullPath } })
})

app.mount('#app')
