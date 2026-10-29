import { httpClient } from '@/common/http/httpClient'

import type { Schema } from './types'

export type Departman = Schema<'DepartmanResponse'>
export type DepartmanKaydetIstegi = Schema<'DepartmanKaydetRequest'>

export async function departmanlariGetir(): Promise<Departman[]> {
  const { data } = await httpClient.get<Departman[]>('/api/departmanlar')
  return data
}

export async function departmanEkle(istek: DepartmanKaydetIstegi): Promise<Departman> {
  const { data } = await httpClient.post<Departman>('/api/departmanlar', istek)
  return data
}

export async function departmanGuncelle(
  id: number,
  istek: DepartmanKaydetIstegi,
): Promise<Departman> {
  const { data } = await httpClient.put<Departman>(`/api/departmanlar/${id}`, istek)
  return data
}

export async function departmanAktifligiDegistir(id: number, aktif: boolean): Promise<void> {
  await httpClient.put(`/api/departmanlar/${id}/aktiflik`, { aktif })
}
