import { describe, expect, it } from 'vitest'
import type { RouteRecordRaw } from 'vue-router'

import { Yetkiler, type Yetki } from '@/common/auth/yetkiler'

import { menuOgeleri } from './menu'

const bos = { render: () => null }

const rotalar: RouteRecordRaw[] = [
  {
    path: '/',
    component: bos,
    children: [
      {
        path: '',
        name: 'ana-sayfa',
        component: bos,
        meta: { baslik: 'Ana Sayfa', menu: { baslik: 'Ana Sayfa', ikon: 'a' } },
      },
      {
        path: 'kullanicilar',
        name: 'kullanicilar',
        component: bos,
        meta: {
          baslik: 'Kullanıcılar',
          yetki: [Yetkiler.KullaniciYonet],
          menu: { baslik: 'Kullanıcılar', ikon: 'a', grup: 'Yönetim' },
        },
      },
      { path: 'yetkisiz', name: 'yetkisiz', component: bos, meta: { baslik: 'Yetkiniz Yok' } },
    ],
  },
]

const yetkileri =
  (sahip: Yetki[]) =>
  (...istenen: Yetki[]) =>
    istenen.some((y) => sahip.includes(y))

describe('menuOgeleri', () => {
  it('yalnız menü bilgisi olan ve yetkisi yeten rotaları verir', () => {
    expect(menuOgeleri(rotalar, yetkileri([])).map((oge) => oge.rotaAdi)).toEqual(['ana-sayfa'])
    expect(
      menuOgeleri(rotalar, yetkileri([Yetkiler.KullaniciYonet])).map((oge) => oge.rotaAdi),
    ).toEqual(['ana-sayfa', 'kullanicilar'])
  })
})
