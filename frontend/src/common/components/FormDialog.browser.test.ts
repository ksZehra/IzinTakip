import { describe, expect, it } from 'vitest'

import { renderWithPlugins } from '@/testing/renderWithPlugins'

import FormDialog from './FormDialog.vue'

describe('FormDialog', () => {
  it('sağ üstte kapatma düğmesini Vazgeç düğmesiyle birlikte gösterir', async () => {
    const { screen } = await renderWithPlugins(FormDialog, {
      props: { modelValue: true, baslik: 'Çalışanı Düzenle', kaydediliyor: false },
    })

    await expect.element(screen.getByText('Çalışanı Düzenle')).toBeVisible()
    const kapat = screen.getByRole('button', { name: 'Kapat' })
    await expect.element(kapat).toBeVisible()
    await expect.element(screen.getByRole('button', { name: 'Vazgeç' })).toBeVisible()
  })
})
