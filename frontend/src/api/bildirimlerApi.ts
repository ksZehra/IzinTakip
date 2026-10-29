import { httpClient } from '@/common/http/httpClient'

import type { Schema } from './types'

export type Bildirim = Schema<'BildirimResponse'>
export type BildirimSayfasi = Schema<'PagedResponseOfBildirimResponse'>

export async function okunmamisSayisiniGetir(): Promise<number> {
  const { data } = await httpClient.get<Schema<'BildirimSayisiResponse'>>('/api/bildirimler/sayi', {
    sessiz: true,
  })
  return data.okunmamis
}

export async function bildirimleriGetir(
  filtre: { sadeceOkunmamis: boolean; page: number; pageSize: number },
  signal?: AbortSignal,
): Promise<BildirimSayfasi> {
  const { data } = await httpClient.get<BildirimSayfasi>('/api/bildirimler', {
    params: filtre,
    signal,
  })
  return data
}

export async function okunduIsaretle(id: number): Promise<void> {
  await httpClient.put(`/api/bildirimler/${id}/okundu`)
}

export async function tumunuOkunduIsaretle(): Promise<void> {
  await httpClient.put('/api/bildirimler/okundu')
}
