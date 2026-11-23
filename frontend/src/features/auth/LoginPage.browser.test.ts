import { beforeEach, describe, expect, it, vi } from 'vitest'
import { userEvent } from 'vitest/browser'

import * as authApi from '@/api/authApi'
import { ApiError } from '@/common/http/ApiError'
import { renderWithPlugins } from '@/testing/renderWithPlugins'

import LoginPage from './LoginPage.vue'

vi.mock('@/api/authApi')

const oturum: authApi.Oturum = {
  kullaniciId: 4,
  eposta: 'calisan@izintakip.local',
  gorunenAd: 'Zeynep Demir',
  calisanId: 3,
  roller: ['Calisan'],
  yetkiler: ['IzinTalebi.Olustur'],
  parolaDegistirmeli: false,
}

async function formuDoldur(screen: Awaited<ReturnType<typeof renderWithPlugins>>['screen']) {
  await userEvent.fill(screen.getByLabelText('E-posta'), 'calisan@izintakip.local')
  await userEvent.fill(screen.getByLabelText('Parola'), 'Parola123')
  await userEvent.click(screen.getByRole('button', { name: 'Giriş yap' }))
}

describe('LoginPage', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  it('hatalı girişte sunucunun mesajını formda gösterir', async () => {
    vi.mocked(authApi.girisYap).mockRejectedValue(
      new ApiError({ durum: 401, mesaj: 'E-posta ya da parola hatalı.' }),
    )
    const { screen } = await renderWithPlugins(LoginPage, { yol: '/login' })

    await formuDoldur(screen)

    await expect
      .element(screen.getByRole('alert'))
      .toHaveTextContent('E-posta ya da parola hatalı.')
  })

  it('başarılı girişte güvenli dönüş adresine gider', async () => {
    vi.mocked(authApi.girisYap).mockResolvedValue()
    vi.mocked(authApi.oturumuGetir).mockResolvedValue(oturum)
    const { screen, router } = await renderWithPlugins(LoginPage, {
      yol: '/login?donus=//kotu.example',
    })

    await formuDoldur(screen)

    await vi.waitFor(() => expect(router.currentRoute.value.fullPath).toBe('/'))
    expect(authApi.girisYap).toHaveBeenCalledWith('calisan@izintakip.local', 'Parola123')
  })
})
