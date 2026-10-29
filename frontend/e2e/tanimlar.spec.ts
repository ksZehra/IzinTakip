import { expect, test } from '@playwright/test'

import { erisilebilirlikDenetle, girisYap, kullanicilar } from './helpers'

const sayfalar = [
  ['/departmanlar', 'Departmanlar'],
  ['/izin-turleri', 'İzin Türleri'],
  ['/hak-kurallari', 'Hak Kuralları'],
  ['/ayarlar', 'İş Takvimi Ayarları'],
  ['/calisanlar', 'Çalışanlar'],
] as const

test('İK tanım ekranlarını açar ve sayfalar erişilebilir', async ({ page }) => {
  await girisYap(page, kullanicilar.ik)

  for (const [adres, baslik] of sayfalar) {
    await page.goto(adres)
    await expect(page.getByRole('heading', { level: 1, name: baslik })).toBeVisible()
    await expect(page.getByRole('progressbar')).toHaveCount(0)
    await erisilebilirlikDenetle(page)
  }
})

test('İK çalışan detayında hak satırı formunu açar', async ({ page }) => {
  await girisYap(page, kullanicilar.ik)
  await page.goto('/calisanlar')
  await page.getByRole('link', { name: 'Zeynep Demir' }).click()

  await expect(page.getByRole('heading', { level: 1, name: 'Zeynep Demir' })).toBeVisible()
  await page.getByRole('button', { name: 'Hak satırı ekle' }).click()
  await expect(page.getByRole('dialog').getByText('Süre', { exact: true })).toBeVisible()
  await erisilebilirlikDenetle(page)
})

test('İK resmi tatil ekler ve siler', async ({ page }) => {
  const yil = new Date().getFullYear() + 2
  const gun = `${yil}-0${1 + Math.floor(Math.random() * 9)}-${10 + Math.floor(Math.random() * 18)}`
  const ad = `Deneme tatili ${Date.now()}`

  await girisYap(page, kullanicilar.ik)
  await page.goto('/resmi-tatiller')
  await page.getByRole('combobox', { name: 'Yıl' }).locator('..').click()
  await page.getByRole('option', { name: String(yil), exact: true }).click()
  await page.getByRole('button', { name: 'Yeni tatil' }).click()
  const dialog = page.getByRole('dialog')
  await dialog.getByLabel('Tarih').fill(gun)
  await dialog.getByLabel('Ad').fill(ad)
  await dialog.getByRole('button', { name: 'Kaydet' }).click()

  await expect(page.getByText(ad, { exact: true })).toBeVisible()
  await page.getByRole('button', { name: `${ad} sil` }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Sil' }).click()
  await expect(page.getByText(ad, { exact: true })).toHaveCount(0)
})
