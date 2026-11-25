import { httpClient } from '@/common/http/httpClient'
import type { FilterValue, ListQuery, Page } from '@/common/composables/usePagedList'

import type { Schema } from './types'

export type Kullanici = Schema<'KullaniciResponse'>
export type KullaniciOlusturRequest = Schema<'KullaniciOlusturRequest'>
export type Rol = Schema<'RolResponse'>
export type RolKaydetRequest = Schema<'RolKaydetRequest'>
export type YetkiTanimi = Schema<'YetkiTanimi'>
export type IslemKaydi = Schema<'IslemKaydiResponse'>

export interface KullaniciFilter extends Record<string, FilterValue> {
  arama: string
  aktif: boolean | null
  rol: string[]
  sort: string
}

export interface IslemKaydiFilter extends Record<string, FilterValue> {
  ilgiliArama: string
  tablo: string[]
  islem: string[]
  baslangic: string
  bitis: string
  sadeceKendiKaydi: boolean | null
  sort: string
}

export async function kullanicilariGetir(
  sorgu: ListQuery<KullaniciFilter>,
  signal?: AbortSignal,
): Promise<Page<Kullanici>> {
  const { data } = await httpClient.get<Page<Kullanici>>('/api/kullanicilar', {
    params: sorgu,
    signal,
  })
  return data
}

export async function kullaniciEkle(istek: KullaniciOlusturRequest): Promise<Kullanici> {
  const { data } = await httpClient.post<Kullanici>('/api/kullanicilar', istek)
  return data
}

export async function kullaniciRolleriniGuncelle(id: number, roller: string[]): Promise<void> {
  await httpClient.put(`/api/kullanicilar/${id}/roller`, { roller })
}

export async function kullaniciAktifligiDegistir(id: number, aktif: boolean): Promise<void> {
  await httpClient.put(`/api/kullanicilar/${id}/aktiflik`, { aktif })
}

export async function parolaSifirla(id: number): Promise<void> {
  await httpClient.post(`/api/kullanicilar/${id}/parola-sifirla`)
}

export async function rolleriGetir(): Promise<Rol[]> {
  const { data } = await httpClient.get<Rol[]>('/api/roller')
  return data
}

export async function atanabilirYetkileriGetir(): Promise<YetkiTanimi[]> {
  const { data } = await httpClient.get<YetkiTanimi[]>('/api/roller/yetkiler')
  return data
}

export async function rolEkle(istek: RolKaydetRequest): Promise<Rol> {
  const { data } = await httpClient.post<Rol>('/api/roller', istek)
  return data
}

export async function rolGuncelle(id: number, istek: RolKaydetRequest): Promise<Rol> {
  const { data } = await httpClient.put<Rol>(`/api/roller/${id}`, istek)
  return data
}

export async function rolSil(id: number): Promise<void> {
  await httpClient.delete(`/api/roller/${id}`)
}

export async function islemKayitlariniGetir(
  sorgu: ListQuery<IslemKaydiFilter>,
  signal?: AbortSignal,
): Promise<Page<IslemKaydi>> {
  const { data } = await httpClient.get<Page<IslemKaydi>>('/api/islem-kayitlari', {
    params: sorgu,
    signal,
  })
  return data
}
