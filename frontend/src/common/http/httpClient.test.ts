import { AxiosError, type AxiosAdapter, type InternalAxiosRequestConfig } from 'axios'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { useToast } from '@/common/composables/useToast'

import { ApiHatasi } from './ApiHatasi'
import { SUNUCUYA_ULASILAMIYOR } from './apiHatasinaCevir'
import { httpClient, oturumDustugundeCalistir } from './httpClient'

function hataDonen(durum: number, govde: unknown): AxiosAdapter {
  return (config: InternalAxiosRequestConfig) =>
    Promise.reject(
      new AxiosError('hata', 'ERR_BAD_RESPONSE', config, null, {
        status: durum,
        statusText: '',
        headers: {},
        config,
        data: govde,
      }),
    )
}

async function hatayiYakala(istek: Promise<unknown>): Promise<ApiHatasi> {
  try {
    await istek
  } catch (hata) {
    if (hata instanceof ApiHatasi) return hata
  }
  throw new Error('ApiHatasi bekleniyordu')
}

describe('httpClient', () => {
  const { mesajlar } = useToast()

  afterEach(() => {
    mesajlar.value = []
    oturumDustugundeCalistir(() => {})
  })

  it('her isteğe CSRF başlığını ekler', async () => {
    const adapter = vi.fn<AxiosAdapter>((config) =>
      Promise.resolve({ data: {}, status: 200, statusText: '', headers: {}, config }),
    )

    await httpClient.get('/api/ornek', { adapter })

    expect(adapter.mock.calls[0]![0].headers['X-CSRF']).toBe('1')
  })

  it('iş kuralı hatasının mesajını toast olarak gösterir', async () => {
    const hata = await hatayiYakala(
      httpClient.get('/api/ornek', { adapter: hataDonen(422, { detail: 'Bakiyeniz yetersiz.' }) }),
    )

    expect(hata.durum).toBe(422)
    expect(hata.message).toBe('Bakiyeniz yetersiz.')
    expect(mesajlar.value).toEqual([expect.objectContaining({ text: 'Bakiyeniz yetersiz.' })])
  })

  it('alan hatalı 400 yanıtında toast göstermez, hataları taşır', async () => {
    const hata = await hatayiYakala(
      httpClient.post(
        '/api/ornek',
        {},
        { adapter: hataDonen(400, { detail: 'Hatalı', errors: { ad: ['Ad zorunlu.'] } }) },
      ),
    )

    expect(hata.alanHatalari).toEqual({ ad: ['Ad zorunlu.'] })
    expect(mesajlar.value).toHaveLength(0)
  })

  it('401 yanıtında oturum düştü işlemini çalıştırır', async () => {
    const oturumDustu = vi.fn()
    oturumDustugundeCalistir(oturumDustu)

    await hatayiYakala(httpClient.get('/api/ornek', { adapter: hataDonen(401, {}) }))

    expect(oturumDustu).toHaveBeenCalledOnce()
    expect(mesajlar.value).toHaveLength(0)
  })

  it('sessiz istekte toast ve yönlendirme yapmaz', async () => {
    const oturumDustu = vi.fn()
    oturumDustugundeCalistir(oturumDustu)

    await hatayiYakala(
      httpClient.get('/api/ornek', { sessiz: true, adapter: hataDonen(401, { detail: 'x' }) }),
    )

    expect(oturumDustu).not.toHaveBeenCalled()
    expect(mesajlar.value).toHaveLength(0)
  })

  it('indirme isteğinin Blob hata gövdesini okur', async () => {
    const govde = new Blob([JSON.stringify({ detail: 'Kayıt bulunamadı.', traceId: 'abc' })])

    const hata = await hatayiYakala(
      httpClient.get('/api/ornek', { responseType: 'blob', adapter: hataDonen(404, govde) }),
    )

    expect(hata.message).toBe('Kayıt bulunamadı.')
    expect(hata.izKodu).toBe('abc')
  })

  it('yanıt gelmezse bağlantı mesajı gösterir', async () => {
    const adapter: AxiosAdapter = (config) =>
      Promise.reject(new AxiosError('Network Error', 'ERR_NETWORK', config))

    const hata = await hatayiYakala(httpClient.get('/api/ornek', { adapter }))

    expect(hata.durum).toBe(0)
    expect(mesajlar.value).toEqual([expect.objectContaining({ text: SUNUCUYA_ULASILAMIYOR })])
  })

  it('iptal edilen istekte toast göstermez', async () => {
    const denetleyici = new AbortController()
    denetleyici.abort()

    const hata = await hatayiYakala(httpClient.get('/api/ornek', { signal: denetleyici.signal }))

    expect(hata.iptalEdildi).toBe(true)
    expect(mesajlar.value).toHaveLength(0)
  })
})
