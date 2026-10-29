import { httpClient } from '@/common/http/httpClient'

import type { Schema } from './types'

export type ResmiTatil = Schema<'ResmiTatilResponse'>
export type ResmiTatilKaydetIstegi = Schema<'ResmiTatilKaydetRequest'>
export type HakKurali = Schema<'HakKuraliResponse'>
export type HakKuraliKaydetIstegi = Schema<'HakKuraliKaydetRequest'>

export async function resmiTatilleriGetir(yil: number): Promise<ResmiTatil[]> {
  const { data } = await httpClient.get<ResmiTatil[]>('/api/resmi-tatiller', { params: { yil } })
  return data
}

export async function resmiTatilEkle(istek: ResmiTatilKaydetIstegi): Promise<ResmiTatil> {
  const { data } = await httpClient.post<ResmiTatil>('/api/resmi-tatiller', istek)
  return data
}

export async function resmiTatilGuncelle(
  id: number,
  istek: ResmiTatilKaydetIstegi,
): Promise<ResmiTatil> {
  const { data } = await httpClient.put<ResmiTatil>(`/api/resmi-tatiller/${id}`, istek)
  return data
}

export async function resmiTatilSil(id: number): Promise<void> {
  await httpClient.delete(`/api/resmi-tatiller/${id}`)
}

export async function hakKurallariniGetir(): Promise<HakKurali[]> {
  const { data } = await httpClient.get<HakKurali[]>('/api/hak-kurallari')
  return data
}

export async function hakKuraliEkle(istek: HakKuraliKaydetIstegi): Promise<HakKurali> {
  const { data } = await httpClient.post<HakKurali>('/api/hak-kurallari', istek)
  return data
}

export async function hakKuraliGuncelle(
  id: number,
  istek: HakKuraliKaydetIstegi,
): Promise<HakKurali> {
  const { data } = await httpClient.put<HakKurali>(`/api/hak-kurallari/${id}`, istek)
  return data
}

export async function hakKuraliAktifligiDegistir(id: number, aktif: boolean): Promise<void> {
  await httpClient.put(`/api/hak-kurallari/${id}/aktiflik`, { aktif })
}
