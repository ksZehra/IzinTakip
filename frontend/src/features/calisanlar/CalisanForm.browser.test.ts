import { describe, expect, it } from 'vitest'

import { renderWithPlugins } from '@/testing/renderWithPlugins'

import CalisanForm from './CalisanForm.vue'
import { bosCalisanFormu } from './calisanFormu'

function cerceveRengi(alan: HTMLElement) {
  const cerceve = alan.closest('.v-field')?.querySelector('.v-field__outline')
  return cerceve ? getComputedStyle(cerceve).color : ''
}

describe('CalisanForm', () => {
  it('zorunlu alanların çerçevesi kırmızı, isteğe bağlı alanınki normal renktedir', async () => {
    const { screen } = await renderWithPlugins(CalisanForm, {
      props: {
        modelValue: true,
        form: bosCalisanFormu(),
        baslik: 'Yeni Çalışan',
        departmanlar: [],
        kaydediliyor: false,
        alanHatasi: () => [],
      },
    })

    const zorunlular = ['Ad', 'Soyad', 'Departman', 'İşe Giriş Tarihi', 'Doğum Tarihi']
    const kirmizi = cerceveRengi(screen.getByLabelText('Ad').element() as HTMLElement)
    for (const etiket of zorunlular) {
      const alan = screen.getByLabelText(etiket).element() as HTMLElement
      expect(cerceveRengi(alan)).toBe(kirmizi)
    }

    const istegeBagli = screen.getByLabelText('Hak Başlangıç Kıdem Yılı').element() as HTMLElement
    expect(cerceveRengi(istegeBagli)).not.toBe(kirmizi)
  })
})
