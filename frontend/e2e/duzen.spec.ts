import { expect, test } from '@playwright/test'

import { erisilebilirlikDenetle, girisYap, kullanicilar } from './yardimcilar'

test('çalışan yönetim menüsünü görmez, yetkisiz sayfaya yönlendirilir', async ({
  page,
  isMobile,
}) => {
  await girisYap(page, kullanicilar.calisan)
  await expect(page.getByRole('heading', { level: 1, name: 'Ana Sayfa' })).toBeVisible()

  if (isMobile) {
    await page.getByRole('button', { name: 'Menüyü aç' }).click()
  }
  const menu = page.getByRole('navigation', { name: 'Ana menü' })
  await expect(menu.getByRole('link', { name: 'Ana Sayfa' })).toBeVisible()
  await expect(menu.getByText('Yönetim')).toHaveCount(0)

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

  await expect(page).toHaveURL(/\/giris$/)
  await page.goto('/bildirimler')
  await expect(page).toHaveURL(/\/giris\?donus=/)
})
