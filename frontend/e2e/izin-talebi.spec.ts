import { expect, test } from '@playwright/test'

import { erisilebilirlikDenetle, girisYap, kullanicilar, secimYap } from './yardimcilar'

function rastgeleIsGunu(): string {
  const tarih = new Date(2028 + Math.floor(Math.random() * 3), Math.floor(Math.random() * 12), 1)
  tarih.setDate(1 + Math.floor(Math.random() * 25))
  while (tarih.getDay() === 0 || tarih.getDay() === 6) tarih.setDate(tarih.getDate() + 1)
  const ay = String(tarih.getMonth() + 1).padStart(2, '0')
  const gun = String(tarih.getDate()).padStart(2, '0')
  return `${tarih.getFullYear()}-${ay}-${gun}`
}

test('çalışan talep açar, yönetici onaylar ve iptal eder', async ({ page }) => {
  const tarih = rastgeleIsGunu()

  await girisYap(page, kullanicilar.calisan)
  await page.getByRole('link', { name: 'Yeni talep' }).click()
  await secimYap(page, 'İzin türü', 'Ücretsiz')
  await page.getByLabel('Tam gün').check()
  await page.getByLabel('Başlangıç tarihi').fill(tarih)
  await page.getByLabel('Bitiş tarihi').fill(tarih)
  await expect(page.getByText('Süre: 1 gün')).toBeVisible()
  await erisilebilirlikDenetle(page)
  await page.getByRole('button', { name: 'Talebi gönder' }).click()

  await expect(page).toHaveURL(/\/izin-talepleri\/\d+$/)
  await expect(page.getByText('Beklemede', { exact: true })).toBeVisible()
  const talepAdresi = new URL(page.url()).pathname

  await page.getByRole('button', { name: /^Kullanıcı menüsü/ }).click()
  await page.getByText('Çıkış yap').click()
  await girisYap(page, kullanicilar.yonetici)
  await page.goto(talepAdresi)
  await page.getByRole('button', { name: 'Onayla' }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Onayla' }).click()
  await expect(page.getByText('Onaylandı', { exact: true })).toBeVisible()

  await page.getByRole('button', { name: 'İptal et' }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'İptal et' }).click()
  await expect(page.getByText('İptal edildi', { exact: true })).toBeVisible()
})

test('talep listesi filtrelenir ve erişilebilir', async ({ page }) => {
  await girisYap(page, kullanicilar.yonetici)
  await page.goto('/izin-talepleri?durum=IptalEdildi')

  await expect(page.getByRole('heading', { level: 1, name: 'İzin Talepleri' })).toBeVisible()
  await expect(page.getByRole('table')).toBeVisible()
  await erisilebilirlikDenetle(page)
})
