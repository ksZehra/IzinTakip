import { createPinia } from 'pinia'
import { createApp } from 'vue'

import App from '@/App.vue'
import { useToast } from '@/common/composables/useToast'
import { ApiError } from '@/common/http/ApiError'
import { BEKLENMEYEN_HATA } from '@/common/http/apiHatasinaCevir'
import { oturumDustugundeCalistir } from '@/common/http/httpClient'
import { createAppVuetify } from '@/plugins/vuetify'
import router from '@/router'
import { useAuthStore } from '@/stores/authStore'

const app = createApp(App).use(createPinia()).use(router).use(createAppVuetify())

app.config.errorHandler = (hata) => {
  if (!(hata instanceof ApiError)) {
    useToast().hataGoster(BEKLENMEYEN_HATA)
  }
}

oturumDustugundeCalistir(() => {
  const auth = useAuthStore()
  if (!auth.girisYapildi) {
    return
  }
  auth.temizle()
  useToast().bilgiGoster('Oturumunuz sona erdi. Devam etmek için tekrar giriş yapın.')
  void router.push({ name: 'login', query: { donus: router.currentRoute.value.fullPath } })
})

app.mount('#app')
