import { dosyaIndir } from '@/common/http/dosyaIndir'
import { httpClient } from '@/common/http/httpClient'
import type { ListQuery, Page } from '@/common/composables/usePagedList'

import type { Schema } from './types'

export type IzinTalebi = Schema<'IzinTalebiResponse'>
export type IzinTalebiSatiri = Schema<'IzinTalebiListeResponse'>
export type IzinDurumu = Schema<'IzinDurumu'>
export type SureTipi = Schema<'SureTipi'>
export type GunYarisi = NonNullable<Schema<'GunYarisi'>>
export type SureHesaplaRequest = Schema<'SureHesaplaRequest'>
export type TalepOlusturRequest = Schema<'IzinTalebiOlusturRequest'>
export type SureOnizlemesi = Schema<'SureHesaplaResponse'>

export interface TalepFilter extends Record<string, string | number | null> {
  calisanId: number | null
  durum: IzinDurumu | null
  izinTuruId: number | null
  baslangic: string | null
  bitis: string | null
}

export async function talepleriGetir(
  sorgu: ListQuery<TalepFilter>,
  signal?: AbortSignal,
): Promise<Page<IzinTalebiSatiri>> {
  const { data } = await httpClient.get<Page<IzinTalebiSatiri>>('/api/izin-talepleri', {
    params: sorgu,
    signal,
  })
  return data
}

export async function talebiGetir(id: number): Promise<IzinTalebi> {
  const { data } = await httpClient.get<IzinTalebi>(`/api/izin-talepleri/${id}`)
  return data
}

export async function talepOlustur(istek: TalepOlusturRequest): Promise<IzinTalebi> {
  const { data } = await httpClient.post<IzinTalebi>('/api/izin-talepleri', istek)
  return data
}

export async function sureHesapla(
  istek: SureHesaplaRequest,
  signal: AbortSignal,
): Promise<SureOnizlemesi> {
  const { data } = await httpClient.post<SureOnizlemesi>(
    '/api/izin-talepleri/sure-hesapla',
    istek,
    {
      sessiz: true,
      signal,
    },
  )
  return data
}

export async function talebiOnayla(id: number, not: string | null): Promise<IzinTalebi> {
  const { data } = await httpClient.post<IzinTalebi>(`/api/izin-talepleri/${id}/onayla`, { not })
  return data
}

export async function talebiReddet(id: number, gerekce: string): Promise<IzinTalebi> {
  const { data } = await httpClient.post<IzinTalebi>(`/api/izin-talepleri/${id}/reddet`, {
    gerekce,
  })
  return data
}

export async function talebiIptalEt(id: number, not: string | null): Promise<IzinTalebi> {
  const { data } = await httpClient.post<IzinTalebi>(`/api/izin-talepleri/${id}/iptal`, { not })
  return data
}

export function talepleriExceleAktar(filtre: TalepFilter): Promise<void> {
  return dosyaIndir('/api/izin-talepleri/excel', filtre, 'izin-talepleri.xlsx')
}
