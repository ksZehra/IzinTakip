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
  await page.getByLabel('Parola').fill(parola)
  await page.getByRole('button', { name: 'Giriş yap' }).click()
}

export async function erisilebilirlikDenetle(page: Page) {
  const sonuc = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).analyze()
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
