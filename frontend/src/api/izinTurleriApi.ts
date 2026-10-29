import { httpClient } from '@/common/http/httpClient'

import type { Schema } from './types'

export type IzinTuru = Schema<'IzinTuruResponse'>
export type IzinTuruKaydetRequest = Schema<'IzinTuruKaydetRequest'>

export async function izinTurleriniGetir(sadeceAktif = false): Promise<IzinTuru[]> {
  const { data } = await httpClient.get<IzinTuru[]>('/api/izin-turleri', {
    params: sadeceAktif ? { aktif: true } : undefined,
  })
  return data
}

export async function izinTuruEkle(istek: IzinTuruKaydetRequest): Promise<IzinTuru> {
  const { data } = await httpClient.post<IzinTuru>('/api/izin-turleri', istek)
  return data
}

export async function izinTuruGuncelle(
  id: number,
  istek: IzinTuruKaydetRequest,
): Promise<IzinTuru> {
  const { data } = await httpClient.put<IzinTuru>(`/api/izin-turleri/${id}`, istek)
  return data
}

export async function izinTuruAktifligiDegistir(id: number, aktif: boolean): Promise<void> {
  await httpClient.put(`/api/izin-turleri/${id}/aktiflik`, { aktif })
}
