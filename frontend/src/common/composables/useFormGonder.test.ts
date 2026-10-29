import { describe, expect, it } from 'vitest'

import { ApiHatasi } from '@/common/http/ApiHatasi'

import { useFormGonder } from './useFormGonder'

describe('useFormGonder', () => {
  it('başarılı işlemin sonucunu döner ve kilidi açar', async () => {
    const { gonder, gonderiliyor } = useFormGonder()

    const sonuc = await gonder(() => Promise.resolve(42))

    expect(sonuc).toBe(42)
    expect(gonderiliyor.value).toBe(false)
  })

  it('alan hatalarını saklar ve undefined döner', async () => {
    const { gonder, alanHatasi } = useFormGonder()

    const sonuc = await gonder(() =>
      Promise.reject(
        new ApiHatasi({ durum: 400, mesaj: 'x', alanHatalari: { ad: ['Ad zorunlu.'] } }),
      ),
    )

    expect(sonuc).toBeUndefined()
    expect(alanHatasi('ad')).toEqual(['Ad zorunlu.'])
    expect(alanHatasi('soyad')).toEqual([])
  })

  it('gönderim sürerken ikinci gönderimi çalıştırmaz', async () => {
    const { gonder } = useFormGonder()
    let cagri = 0
    const islem = () => {
      cagri++
      return new Promise((coz) => setTimeout(coz, 10))
    }

    await Promise.all([gonder(islem), gonder(islem)])

    expect(cagri).toBe(1)
  })

  it('beklenmeyen hatayı yukarı fırlatır', async () => {
    const { gonder } = useFormGonder()

    await expect(gonder(() => Promise.reject(new TypeError('hata')))).rejects.toThrow(TypeError)
  })
})
