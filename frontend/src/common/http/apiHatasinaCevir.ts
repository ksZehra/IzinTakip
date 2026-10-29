import { isAxiosError, isCancel } from 'axios'

import { ApiError, type FieldErrors } from './ApiError'

export const SUNUCUYA_ULASILAMIYOR =
  'Sunucuya şu anda ulaşılamıyor. Bağlantınızı kontrol edip tekrar deneyin.'
export const BEKLENMEYEN_HATA = 'Beklenmeyen bir hata oluştu. Sayfayı yenileyip tekrar deneyin.'

interface ProblemDetails {
  detail?: string
  errors?: FieldErrors
  traceId?: string
}

export async function apiHatasinaCevir(hata: unknown): Promise<ApiError> {
  if (isCancel(hata)) {
    return new ApiError({ durum: 0, mesaj: '', iptalEdildi: true })
  }
  if (!isAxiosError(hata)) {
    return new ApiError({ durum: 0, mesaj: BEKLENMEYEN_HATA })
  }
  if (!hata.response) {
    return new ApiError({ durum: 0, mesaj: SUNUCUYA_ULASILAMIYOR })
  }

  const problem = await problemDetailsOku(hata.response.data)
  return new ApiError({
    durum: hata.response.status,
    mesaj: problem?.detail ?? BEKLENMEYEN_HATA,
    alanHatalari: problem?.errors,
    izKodu: problem?.traceId,
  })
}

async function problemDetailsOku(govde: unknown): Promise<ProblemDetails | undefined> {
  const icerik = govde instanceof Blob ? await govde.text() : govde
  if (typeof icerik === 'string') {
    try {
      return JSON.parse(icerik) as ProblemDetails
    } catch {
      return undefined
    }
  }
  return typeof icerik === 'object' && icerik !== null ? icerik : undefined
}
