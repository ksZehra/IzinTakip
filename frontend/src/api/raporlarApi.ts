import { dosyaIndir } from '@/common/http/dosyaIndir'
import { httpClient } from '@/common/http/httpClient'
import type { ListQuery, Page } from '@/common/composables/usePagedList'

import type { Schema } from './types'

export type TakvimKaydi = Schema<'TakvimKaydiResponse'>
export type BakiyeSatiri = Schema<'BakiyeRaporuResponse'>

export interface BakiyeFilter extends Record<string, number | null> {
  departmanId: number | null
}

export async function takvimiGetir(sorgu: {
  baslangic: string
  bitis: string
  departmanId: number | null
}): Promise<TakvimKaydi[]> {
  const { data } = await httpClient.get<TakvimKaydi[]>('/api/raporlar/takvim', { params: sorgu })
  return data
}

export async function bakiyeleriGetir(
  sorgu: ListQuery<BakiyeFilter>,
  signal?: AbortSignal,
): Promise<Page<BakiyeSatiri>> {
  const { data } = await httpClient.get<Page<BakiyeSatiri>>('/api/raporlar/bakiyeler', {
    params: sorgu,
    signal,
  })
  return data
}

export function bakiyeleriExceleAktar(filtre: BakiyeFilter): Promise<void> {
  return dosyaIndir('/api/raporlar/bakiyeler/excel', filtre, 'bakiyeler.xlsx')
}
