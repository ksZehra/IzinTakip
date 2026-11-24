/// <reference lib="dom" />
import process from 'node:process'

import AxeBuilder from '@axe-core/playwright'
import { expect, type Page } from '@playwright/test'

export const kullanicilar = {
  calisan: 'calisan@izintakip.local',
  yonetici: 'yonetici@izintakip.local',
  ik: 'ik@izintakip.local',
} as const

export function demoParolasi(): string {
  const parola = process.env.E2E_PAROLA
  if (!parola) {
    throw new Error('E2E_PAROLA ortam değişkeni demo kullanıcılarının parolasını içermeli.')
  }
  return parola
}

export async function girisYap(page: Page, eposta: string, parola = demoParolasi()) {
  await page.goto('/login')
  await girisFormunuDoldur(page, eposta, parola)
  await page.waitForURL((adres) => !adres.pathname.startsWith('/login'))
}

export async function girisFormunuDoldur(page: Page, eposta: string, parola = demoParolasi()) {
  await page.getByLabel('E-posta').fill(eposta)
  await page.getByLabel('Parola', { exact: true }).fill(parola)
  await page.getByRole('button', { name: 'Giriş yap' }).click()
}

export async function erisilebilirlikDenetle(page: Page, kapsam?: string) {
  await page.evaluate(() =>
    Promise.all(
      document
        .getAnimations()
        .filter((animasyon) => animasyon.effect?.getComputedTiming().iterations !== Infinity)
        .map((animasyon) => animasyon.finished.catch(() => null)),
    ),
  )
  const denetim = new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa'])
  if (kapsam) {
    denetim.include(kapsam)
  }
  const sonuc = await denetim.analyze()
  expect(
    sonuc.violations.map(
      (ihlal) => `${ihlal.id}: ${ihlal.nodes.map((n) => n.target.join(' ')).join(', ')}`,
    ),
  ).toEqual([])
}

export async function secimYap(page: Page, alanEtiketi: string, secenek: string) {
  await page.locator('.v-select, .v-autocomplete').filter({ hasText: alanEtiketi }).first().click()
  await page.getByRole('option', { name: secenek, exact: true }).click()
}

export async function hakkindaPenceresiniDogrula(page: Page) {
  await page.getByRole('button', { name: /Zehra Nur Köse/ }).click()

  const pencere = page.getByRole('dialog')
  await expect(pencere.getByRole('heading', { name: 'Personel İzin Takip Sistemi' })).toBeVisible()
  await expect(pencere.getByText(/^Sürüm \d+\.\d+\.\d+ · 2026$/)).toBeVisible()
  await expect(pencere.getByRole('link', { name: 'github.com/ksZehra' })).toBeVisible()
  await erisilebilirlikDenetle(page, '[role=dialog]')
}
