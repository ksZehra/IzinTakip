import { describe, expect, it } from 'vitest'

import { useToast } from '@/common/composables/useToast'
import { renderWithPlugins } from '@/testing/renderWithPlugins'

import App from './App.vue'

describe('App', () => {
  it('toast mesajı kapatma düğmesini yazı yerine ikonla gösterir', async () => {
    const { screen } = await renderWithPlugins(App)
    useToast().basariGoster('Kaydedildi.')

    const dugme = screen.getByRole('button', { name: 'Kapat' })
    await expect.element(dugme).toBeVisible()
    const element = dugme.element()
    expect(element.querySelector('svg')).not.toBeNull()
    expect(element.textContent?.trim()).toBe('')
  })
})
