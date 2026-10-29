import { describe, expect, it } from 'vitest'

import { renderWithPlugins } from '@/testing/renderWithPlugins'

import BulunamadiPage from './BulunamadiPage.vue'

describe('BulunamadiPage', () => {
  it('başlığı ve ana sayfa bağlantısını gösterir', async () => {
    const { screen } = await renderWithPlugins(BulunamadiPage)

    await expect
      .element(screen.getByRole('heading', { level: 1, name: 'Sayfa bulunamadı' }))
      .toBeVisible()
    await expect
      .element(screen.getByRole('link', { name: 'Ana sayfaya dön' }))
      .toHaveAttribute('href', '/')
  })
})
