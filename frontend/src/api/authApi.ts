import { httpClient } from '@/common/http/httpClient'

import type { Schema } from './types'

export type Oturum = Schema<'CurrentUserResponse'>

export async function girisYap(eposta: string, parola: string): Promise<void> {
  await httpClient.post('/bff/giris', { eposta, parola }, { sessiz: true })
}

export async function cikisYap(): Promise<void> {
  await httpClient.post('/bff/cikis', null, { sessiz: true })
}

export async function oturumuGetir(): Promise<Oturum> {
  const { data } = await httpClient.get<Oturum>('/api/auth/ben', { sessiz: true })
  return data
}

export async function parolaDegistir(istek: Schema<'ParolaDegistirRequest'>): Promise<void> {
  await httpClient.post('/api/auth/parola-degistir', istek)
}
