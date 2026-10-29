import { describe, expect, it } from 'vitest'

import { renderWithPlugins } from '@/testing/renderWithPlugins'

import NotFoundPage from './NotFoundPage.vue'

describe('NotFoundPage', () => {
  it('başlığı ve ana sayfa bağlantısını gösterir', async () => {
    const { screen } = await renderWithPlugins(NotFoundPage)

    await expect
      .element(screen.getByRole('heading', { level: 1, name: 'Sayfa bulunamadı' }))
      .toBeVisible()
    await expect
      .element(screen.getByRole('link', { name: 'Ana sayfaya dön' }))
      .toHaveAttribute('href', '/')
  })
})
