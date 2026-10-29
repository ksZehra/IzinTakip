import { describe, expect, it } from 'vitest'
import { createMemoryHistory, createRouter, type RouteMeta } from 'vue-router'

import type { Oturum } from '@/api/authApi'
import { Yetkiler, type Yetki } from '@/common/auth/yetkiler'

import { guvenliDonusAdresi, rotaKarari } from './guards'
import { routes } from './routes'

function oturum(yetkiler: Yetki[], parolaDegistirmeli = false): Oturum {
  return {
    kullaniciId: 1,
    eposta: 'calisan@izintakip.local',
    gorunenAd: 'Zeynep Demir',
    calisanId: 3,
    roller: ['Calisan'],
    yetkiler,
    parolaDegistirmeli,
  }
}

function hedef(name: string, meta: Partial<RouteMeta> = {}, fullPath = `/${name}`) {
  return { name, fullPath, meta: { baslik: name, ...meta } }
}

describe('rotaKarari', () => {
  it('oturum yoksa giriş sayfasına dönüş adresiyle yönlendirir', () => {
    expect(rotaKarari(hedef('izin-talepleri', {}, '/izin-talepleri/5'), null)).toEqual({
      name: 'giris',
      query: { donus: '/izin-talepleri/5' },
    })
  })

  it('oturum açıkken giriş sayfasını ana sayfaya çevirir', () => {
    expect(rotaKarari(hedef('giris', { herkeseAcik: true }), oturum([]))).toEqual({
      name: 'ana-sayfa',
    })
  })

  it('parolasını değiştirmesi gerekeni parola sayfasında tutar', () => {
    const zorunlu = oturum([Yetkiler.IzinTalebiOlustur], true)

    expect(rotaKarari(hedef('ana-sayfa'), zorunlu)).toEqual({ name: 'parola-degistir' })
    expect(rotaKarari(hedef('parola-degistir'), zorunlu)).toBe(true)
  })

  it('gereken yetkilerden biri yoksa yetkisiz sayfasına yönlendirir', () => {
    const meta = { yetki: [Yetkiler.KullaniciYonet] }

    expect(rotaKarari(hedef('kullanicilar', meta), oturum([Yetkiler.IzinTalebiOlustur]))).toEqual({
      name: 'yetkisiz',
    })
    expect(rotaKarari(hedef('kullanicilar', meta), oturum([Yetkiler.KullaniciYonet]))).toBe(true)
  })
})

describe('guvenliDonusAdresi', () => {
  const router = createRouter({ history: createMemoryHistory(), routes })

  it('uygulama içi adresi kabul eder', () => {
    expect(guvenliDonusAdresi('/parola-degistir', router)).toBe('/parola-degistir')
  })

  it.each(['https://kotu.example', '//kotu.example', '/\\kotu.example', '/olmayan', 42, undefined])(
    'dış ya da bilinmeyen adresi ana sayfaya çevirir: %s',
    (deger) => {
      expect(guvenliDonusAdresi(deger, router)).toBe('/')
    },
  )
})
