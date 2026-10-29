import { httpClient } from '@/common/http/httpClient'

import type { Schema } from './types'

export type Ayarlar = Schema<'AyarlarResponse'>
export type AyarlarGuncelleRequest = Schema<'AyarlarGuncelleRequest'>
export type HaftaGunu = Schema<'HaftaGunleri'>

export async function ayarlariGetir(): Promise<Ayarlar> {
  const { data } = await httpClient.get<Ayarlar>('/api/ayarlar')
  return data
}

export async function ayarlariGuncelle(istek: AyarlarGuncelleRequest): Promise<Ayarlar> {
  const { data } = await httpClient.put<Ayarlar>('/api/ayarlar', istek)
  return data
}
