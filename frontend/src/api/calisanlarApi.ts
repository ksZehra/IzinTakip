import { httpClient } from '@/common/http/httpClient'
import type { FilterValue, ListQuery, Page } from '@/common/composables/usePagedList'

import type { Schema } from './types'

export type Calisan = Schema<'CalisanResponse'>
export type CalisanSatiri = Schema<'CalisanListeResponse'>
export type CalisanKaydetRequest = Schema<'CalisanKaydetRequest'>
export type Bakiye = Schema<'BakiyeResponse'>
export type IzinHaklari = Schema<'IzinHaklariResponse'>
export type IzinHakkiKaydetRequest = Schema<'IzinHakkiKaydetRequest'>

export interface CalisanFilter extends Record<string, FilterValue> {
  arama: string
  departmanId: number[]
  aktif: boolean | null
  iseGirisBaslangic: string
  iseGirisBitis: string
  sort: string
}

export async function calisanlariGetir(
  sorgu: Partial<ListQuery<CalisanFilter>>,
  signal?: AbortSignal,
): Promise<Page<CalisanSatiri>> {
  const { data } = await httpClient.get<Page<CalisanSatiri>>('/api/calisanlar', {
    params: sorgu,
    signal,
  })
  return data
}

export async function calisaniGetir(id: number): Promise<Calisan> {
  const { data } = await httpClient.get<Calisan>(`/api/calisanlar/${id}`)
  return data
}

export async function calisanEkle(istek: CalisanKaydetRequest): Promise<Calisan> {
  const { data } = await httpClient.post<Calisan>('/api/calisanlar', istek)
  return data
}

export async function calisanGuncelle(id: number, istek: CalisanKaydetRequest): Promise<Calisan> {
  const { data } = await httpClient.put<Calisan>(`/api/calisanlar/${id}`, istek)
  return data
}

export async function calisanAktifligiDegistir(id: number, aktif: boolean): Promise<void> {
  await httpClient.put(`/api/calisanlar/${id}/aktiflik`, { aktif })
}

export async function bakiyeGetir(calisanId: number): Promise<Bakiye> {
  const { data } = await httpClient.get<Bakiye>(`/api/calisanlar/${calisanId}/bakiye`)
  return data
}

export async function haklariGetir(calisanId: number): Promise<IzinHaklari> {
  const { data } = await httpClient.get<IzinHaklari>(`/api/calisanlar/${calisanId}/haklar`)
  return data
}

export async function hakEkle(calisanId: number, istek: IzinHakkiKaydetRequest): Promise<void> {
  await httpClient.post(`/api/calisanlar/${calisanId}/haklar`, istek)
}

export async function hakGuncelle(
  calisanId: number,
  hakId: number,
  istek: IzinHakkiKaydetRequest,
): Promise<void> {
  await httpClient.put(`/api/calisanlar/${calisanId}/haklar/${hakId}`, istek)
}

export async function hakSil(calisanId: number, hakId: number): Promise<void> {
  await httpClient.delete(`/api/calisanlar/${calisanId}/haklar/${hakId}`)
}
