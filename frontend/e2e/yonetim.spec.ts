import process from 'node:process'

import { expect, test } from '@playwright/test'

import { erisilebilirlikDenetle, girisYap, kullanicilar } from './yardimcilar'

function yoneticiParolasi(): string {
  const parola = process.env.E2E_ADMIN_PAROLA
  if (!parola) {
    throw new Error('E2E_ADMIN_PAROLA ortam değişkeni SuperAdmin parolasını içermeli.')
  }
  return parola
}

test('İK kullanıcıları yönetir ama rolleri açamaz', async ({ page }) => {
  await girisYap(page, kullanicilar.ik)
  await page.goto('/kullanicilar')

  await expect(page.getByRole('heading', { level: 1, name: 'Kullanıcılar' })).toBeVisible()
  await expect(
    page.getByRole('cell', { name: 'calisan@izintakip.local', exact: true }),
  ).toBeVisible()
  await erisilebilirlikDenetle(page)

  await page.goto('/roller')
  await expect(page.getByRole('heading', { level: 1, name: 'Yetkiniz yok' })).toBeVisible()
})

test('SuperAdmin rolleri ve işlem kayıtlarını görür', async ({ page }) => {
  await girisYap(page, 'admin@izintakip.local', yoneticiParolasi())

  await page.goto('/roller')
  await expect(page.getByRole('heading', { level: 1, name: 'Roller' })).toBeVisible()
  await page.getByRole('button', { name: 'Yonetici düzenle' }).click()
  await expect(page.getByRole('dialog').getByLabel('Ad', { exact: true })).toHaveValue('Yonetici')
  await erisilebilirlikDenetle(page)
  await page.keyboard.press('Escape')

  await page.goto('/islem-kayitlari')
  await expect(page.getByRole('heading', { level: 1, name: 'İşlem Kayıtları' })).toBeVisible()
  await expect(page.getByRole('progressbar')).toHaveCount(0)
  await erisilebilirlikDenetle(page)
})
