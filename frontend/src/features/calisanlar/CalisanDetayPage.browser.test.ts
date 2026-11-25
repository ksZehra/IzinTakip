import { beforeEach, describe, expect, it, vi } from 'vitest'

import * as calisanlarApi from '@/api/calisanlarApi'
import { renderWithPlugins } from '@/testing/renderWithPlugins'

import CalisanDetayPage from './CalisanDetayPage.vue'

vi.mock('@/api/calisanlarApi')

const haklar = {
  satirlar: [
    {
      id: 1,
      kazanimTarihi: '2026-01-01',
      dakika: 6720,
      sureMetni: '14 gün',
      kaynak: 'Kural',
      kidemYili: 1,
      aciklama: '1. yıl hakkı',
    },
    {
      id: 2,
      kazanimTarihi: '2026-03-02',
      dakika: 9600,
      sureMetni: '20 gün',
      kaynak: 'Elle',
      kidemYili: null,
      aciklama: 'Yanlış giriş',
    },
  ],
  toplamDakika: 16320,
  toplamSureMetni: '34 gün',
}

describe('CalisanDetayPage', () => {
  beforeEach(() => {
    vi.mocked(calisanlarApi.calisaniGetir).mockResolvedValue({
      id: 7,
      ad: 'Zehra',
      soyad: 'Köse',
      departmanId: 1,
      departmanAdi: 'Yazılım',
      iseGirisTarihi: '2020-08-10',
      dogumTarihi: '1998-01-02',
      hakBaslangicKidemYili: 1,
      aktif: true,
      kullaniciEposta: null,
    })
    vi.mocked(calisanlarApi.bakiyeGetir).mockResolvedValue({
      hakDakika: 16320,
      hakMetni: '34 gün',
      kullanilanDakika: 0,
      kullanilanMetni: '0 gün',
      bekleyenDakika: 0,
      bekleyenMetni: '0 gün',
      kullanilabilirDakika: 16320,
      kullanilabilirMetni: '34 gün',
    })
    vi.mocked(calisanlarApi.haklariGetir).mockResolvedValue(haklar as calisanlarApi.IzinHaklari)
  })

  it('düzenle ve sil düğmelerini yalnız elle girilen hak satırında gösterir', async () => {
    const { screen } = await renderWithPlugins(CalisanDetayPage, { props: { id: 7 } })

    await expect.element(screen.getByRole('button', { name: 'Yanlış giriş düzenle' })).toBeVisible()
    await expect.element(screen.getByRole('button', { name: 'Yanlış giriş sil' })).toBeVisible()
    expect(screen.getByRole('button', { name: '1. yıl hakkı düzenle' }).elements()).toHaveLength(0)
    expect(screen.getByRole('button', { name: '1. yıl hakkı sil' }).elements()).toHaveLength(0)
  })

  it('silmeyi onaylayınca satırı siler', async () => {
    vi.mocked(calisanlarApi.hakSil).mockResolvedValue()
    const { screen } = await renderWithPlugins(CalisanDetayPage, { props: { id: 7 } })

    await screen.getByRole('button', { name: 'Yanlış giriş sil' }).click()
    await screen.getByRole('dialog').getByRole('button', { name: 'Sil' }).click()

    await vi.waitFor(() => expect(calisanlarApi.hakSil).toHaveBeenCalledWith(7, 2))
  })
})
