import { dosyaIndir } from '@/common/http/dosyaIndir'
import { httpClient } from '@/common/http/httpClient'

import type { Schema } from './types'

export type IzinBelgesi = Schema<'IzinBelgesiResponse'>

const adres = (talepId: number) => `/api/izin-talepleri/${talepId}/belgeler`

export async function belgeleriGetir(talepId: number): Promise<IzinBelgesi[]> {
  const { data } = await httpClient.get<IzinBelgesi[]>(adres(talepId))
  return data
}

export async function belgeYukle(talepId: number, dosya: File): Promise<void> {
  const form = new FormData()
  form.append('dosya', dosya)
  await httpClient.post(adres(talepId), form)
}

export function belgeIndir(talepId: number, belge: IzinBelgesi): Promise<void> {
  return dosyaIndir(`${adres(talepId)}/${belge.id}`, undefined, belge.dosyaAdi)
}

export async function belgeSil(talepId: number, belgeId: number): Promise<void> {
  await httpClient.delete(`${adres(talepId)}/${belgeId}`)
}
