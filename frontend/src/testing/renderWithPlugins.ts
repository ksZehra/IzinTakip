import { createPinia } from 'pinia'
import { render } from 'vitest-browser-vue'
import { defineComponent, h, type Component } from 'vue'
import { createMemoryHistory, createRouter } from 'vue-router'
import { VApp } from 'vuetify/components'

import { createAppVuetify } from '@/plugins/vuetify'
import { routes } from '@/router/routes'

interface RenderOptions {
  props?: Record<string, unknown>
  yol?: string
}

export async function renderWithPlugins(component: Component, options: RenderOptions = {}) {
  const router = createRouter({ history: createMemoryHistory(), routes })
  await router.push(options.yol ?? '/')
  const pinia = createPinia()

  const sarmalayici = defineComponent(() => () => h(VApp, null, () => h(component, options.props)))
  const screen = await render(sarmalayici, {
    global: { plugins: [createAppVuetify(), router, pinia] },
  })

  return { screen, router, pinia }
}
