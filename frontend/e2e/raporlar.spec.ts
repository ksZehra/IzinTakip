import { expect, test } from '@playwright/test'

import { erisilebilirlikDenetle, girisYap, kullanicilar } from './helpers'

test('İK bakiye raporunu görür', async ({ page }) => {
  await girisYap(page, kullanicilar.ik)
  await page.goto('/raporlar/bakiyeler')

  await expect(page.getByRole('heading', { level: 1, name: 'Bakiye Raporu' })).toBeVisible()
  await expect(page.getByText('Zeynep Demir', { exact: true })).toBeVisible()
  await erisilebilirlikDenetle(page)
})

test('takvim ay ay gezilir', async ({ page }) => {
  await girisYap(page, kullanicilar.yonetici)
  await page.goto('/takvim')
  await expect(page.getByRole('heading', { level: 1, name: 'İzin Takvimi' })).toBeVisible()

  const ay = page.getByRole('heading', { level: 2 })
  const ilkAy = await ay.textContent()
  await page.getByRole('button', { name: 'Sonraki ay' }).click()

  await expect(ay).not.toHaveText(ilkAy ?? '')
  await erisilebilirlikDenetle(page)
})

test('çalışan bakiye raporunu açamaz', async ({ page }) => {
  await girisYap(page, kullanicilar.calisan)
  await page.goto('/raporlar/bakiyeler')

  await expect(page.getByRole('heading', { level: 1, name: 'Yetkiniz yok' })).toBeVisible()
})
