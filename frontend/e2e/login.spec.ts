import { expect, test } from '@playwright/test'

import { erisilebilirlikDenetle, girisFormunuDoldur, kullanicilar } from './helpers'

test('hatalı parolada formda uyarı gösterir', async ({ page }) => {
  await page.goto('/login')
  await girisFormunuDoldur(page, 'kayitsiz@izintakip.local', 'Yanlis1Parola')

  await expect(page.getByRole('alert')).toContainText('E-posta ya da parola hatalı')
  await expect(page).toHaveURL(/\/login/)
})

test('giriş sayfası erişilebilirlik denetiminden geçer', async ({ page }) => {
  await page.goto('/login')
  await expect(page.getByRole('heading', { name: 'Giriş' })).toBeVisible()

  await erisilebilirlikDenetle(page)
})

test('oturumsuz istenen sayfaya girişten sonra döner', async ({ page }) => {
  await page.goto('/parola-degistir')
  await expect(page).toHaveURL(/\/login\?donus=(%2F|\/)parola-degistir/)

  await girisFormunuDoldur(page, kullanicilar.yonetici)

  await expect(page).toHaveURL(/\/parola-degistir$/)
  await expect(page.getByRole('heading', { name: 'Parola Değiştir' })).toBeVisible()
})
