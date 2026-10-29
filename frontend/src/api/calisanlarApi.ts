import { httpClient } from '@/common/http/httpClient'
import type { ListeSorgusu, Sayfa } from '@/common/composables/useSayfaliListe'

import type { Schema } from './types'

export type Calisan = Schema<'CalisanResponse'>
export type CalisanSatiri = Schema<'CalisanListeResponse'>
export type CalisanKaydetIstegi = Schema<'CalisanKaydetRequest'>
export type Bakiye = Schema<'BakiyeResponse'>
export type IzinHaklari = Schema<'IzinHaklariResponse'>
export type IzinHakkiEkleIstegi = Schema<'IzinHakkiEkleRequest'>

export interface CalisanFiltresi extends Record<string, string | number | boolean | null> {
  arama: string
  departmanId: number | null
  aktif: boolean | null
}

export async function calisanlariGetir(
  sorgu: Partial<ListeSorgusu<CalisanFiltresi>>,
  signal?: AbortSignal,
): Promise<Sayfa<CalisanSatiri>> {
  const { data } = await httpClient.get<Sayfa<CalisanSatiri>>('/api/calisanlar', {
    params: sorgu,
    signal,
  })
  return data
}

export async function calisaniGetir(id: number): Promise<Calisan> {
  const { data } = await httpClient.get<Calisan>(`/api/calisanlar/${id}`)
  return data
}

export async function calisanEkle(istek: CalisanKaydetIstegi): Promise<Calisan> {
  const { data } = await httpClient.post<Calisan>('/api/calisanlar', istek)
  return data
}

export async function calisanGuncelle(id: number, istek: CalisanKaydetIstegi): Promise<Calisan> {
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

export async function hakEkle(calisanId: number, istek: IzinHakkiEkleIstegi): Promise<void> {
  await httpClient.post(`/api/calisanlar/${calisanId}/haklar`, istek)
}
