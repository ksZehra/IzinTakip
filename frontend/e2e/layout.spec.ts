import { expect, test } from '@playwright/test'

import {
  erisilebilirlikDenetle,
  girisYap,
  hakkindaPenceresiniDogrula,
  kullanicilar,
} from './helpers'

test('çalışan yönetim menüsünü görmez', async ({ page, isMobile }) => {
  test.skip(isMobile, 'Mobilde menü kapalı başlar; ayrı testte denenir.')
  await girisYap(page, kullanicilar.calisan)
  await expect(page.getByRole('heading', { level: 1, name: 'Ana Sayfa' })).toBeVisible()

  const menu = page.getByRole('navigation', { name: 'Ana menü' })
  await expect(menu.getByRole('link', { name: 'İzin Talepleri' })).toBeVisible()
  await expect(menu.getByText('Yönetim')).toHaveCount(0)
  await erisilebilirlikDenetle(page)
})

test('mobilde menü düğmeyle açılır', async ({ page, isMobile }) => {
  test.skip(!isMobile, 'Yalnız mobil görünüm.')
  await girisYap(page, kullanicilar.calisan)
  await page.getByRole('button', { name: 'Menüyü aç' }).click()

  await expect(
    page
      .getByRole('navigation', { name: 'Ana menü' })
      .getByRole('link', { name: 'İzin Talepleri' }),
  ).toBeVisible()
  await erisilebilirlikDenetle(page)
})

test('bildirimler sayfası açılır ve erişilebilir', async ({ page }) => {
  await girisYap(page, kullanicilar.yonetici)
  await page.getByRole('link', { name: /^Bildirimler/ }).click()

  await expect(page.getByRole('heading', { level: 1, name: 'Bildirimler' })).toBeVisible()
  await erisilebilirlikDenetle(page)
})

test('çıkış yapınca giriş sayfasına döner ve korumalı sayfa açılmaz', async ({ page }) => {
  await girisYap(page, kullanicilar.ik)
  await page.getByRole('button', { name: /^Kullanıcı menüsü/ }).click()
  await page.getByText('Çıkış yap').click()

  await expect(page).toHaveURL(/\/login$/)
  await page.goto('/bildirimler')
  await expect(page).toHaveURL(/\/login\?donus=/)
})

test('menü altındaki imza hakkında penceresini açar', async ({ page, isMobile }) => {
  test.skip(isMobile, 'Mobilde menü kapalı başlar; ayrı testte denenir.')
  await girisYap(page, kullanicilar.calisan)
  await hakkindaPenceresiniDogrula(page)
})
