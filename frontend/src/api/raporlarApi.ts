import { dosyaIndir } from '@/common/http/dosyaIndir'
import { httpClient } from '@/common/http/httpClient'
import type { ListeSorgusu, Sayfa } from '@/common/composables/useSayfaliListe'

import type { Schema } from './types'

export type TakvimKaydi = Schema<'TakvimKaydiResponse'>
export type BakiyeSatiri = Schema<'BakiyeRaporuResponse'>

export interface BakiyeFiltresi extends Record<string, number | null> {
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
  sorgu: ListeSorgusu<BakiyeFiltresi>,
  signal?: AbortSignal,
): Promise<Sayfa<BakiyeSatiri>> {
  const { data } = await httpClient.get<Sayfa<BakiyeSatiri>>('/api/raporlar/bakiyeler', {
    params: sorgu,
    signal,
  })
  return data
}

export function bakiyeleriExceleAktar(filtre: BakiyeFiltresi): Promise<void> {
  return dosyaIndir('/api/raporlar/bakiyeler/excel', filtre, 'bakiyeler.xlsx')
}
