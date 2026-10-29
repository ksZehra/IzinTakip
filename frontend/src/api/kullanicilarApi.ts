import { httpClient } from '@/common/http/httpClient'
import type { ListeSorgusu, Sayfa } from '@/common/composables/useSayfaliListe'

import type { Schema } from './types'

export type Kullanici = Schema<'KullaniciResponse'>
export type KullaniciOlusturIstegi = Schema<'KullaniciOlusturRequest'>
export type Rol = Schema<'RolResponse'>
export type RolKaydetIstegi = Schema<'RolKaydetRequest'>
export type YetkiTanimi = Schema<'YetkiTanimi'>
export type IslemKaydi = Schema<'IslemKaydiResponse'>

export interface KullaniciFiltresi extends Record<string, string | boolean | null> {
  arama: string
  aktif: boolean | null
}

export interface IslemKaydiFiltresi extends Record<string, string | boolean | null> {
  tablo: string | null
  baslangic: string | null
  bitis: string | null
  sadeceKendiKaydi: boolean
}

export async function kullanicilariGetir(
  sorgu: ListeSorgusu<KullaniciFiltresi>,
  signal?: AbortSignal,
): Promise<Sayfa<Kullanici>> {
  const { data } = await httpClient.get<Sayfa<Kullanici>>('/api/kullanicilar', {
    params: sorgu,
    signal,
  })
  return data
}

export async function kullaniciEkle(istek: KullaniciOlusturIstegi): Promise<Kullanici> {
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

export async function rolEkle(istek: RolKaydetIstegi): Promise<Rol> {
  const { data } = await httpClient.post<Rol>('/api/roller', istek)
  return data
}

export async function rolGuncelle(id: number, istek: RolKaydetIstegi): Promise<Rol> {
  const { data } = await httpClient.put<Rol>(`/api/roller/${id}`, istek)
  return data
}

export async function rolSil(id: number): Promise<void> {
  await httpClient.delete(`/api/roller/${id}`)
}

export async function islemKayitlariniGetir(
  sorgu: ListeSorgusu<IslemKaydiFiltresi>,
  signal?: AbortSignal,
): Promise<Sayfa<IslemKaydi>> {
  const { data } = await httpClient.get<Sayfa<IslemKaydi>>('/api/islem-kayitlari', {
    params: sorgu,
    signal,
  })
  return data
}
