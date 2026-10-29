import { createPinia } from 'pinia'
import { createApp } from 'vue'

import App from '@/App.vue'
import { createAppVuetify } from '@/plugins/vuetify'
import router from '@/router'

createApp(App).use(createPinia()).use(router).use(createAppVuetify()).mount('#app')
