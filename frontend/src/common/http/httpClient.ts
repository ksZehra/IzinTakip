import axios, { isAxiosError } from 'axios'

import { useToast } from '@/common/composables/useToast'

import type { ApiHatasi } from './ApiHatasi'
import { apiHatasinaCevir } from './apiHatasinaCevir'

declare module 'axios' {
  interface AxiosRequestConfig {
    sessiz?: boolean
  }
}

let oturumDustugunde: () => void = () => {}

export function oturumDustugundeCalistir(islem: () => void) {
  oturumDustugunde = islem
}

export const httpClient = axios.create({
  headers: { 'X-CSRF': '1' },
  timeout: 30_000,
})

httpClient.interceptors.response.use(undefined, async (hata: unknown) => {
  const apiHatasi = await apiHatasinaCevir(hata)
  const sessiz = isAxiosError(hata) && hata.config?.sessiz === true
  if (!sessiz && !apiHatasi.iptalEdildi) {
    hatayiBildir(apiHatasi)
  }
  throw apiHatasi
})

function hatayiBildir(hata: ApiHatasi) {
  if (hata.durum === 401) {
    oturumDustugunde()
    return
  }
  if (hata.durum === 400 && hata.alanHatasiVar) {
    return
  }
  useToast().hataGoster(hata.message)
}
