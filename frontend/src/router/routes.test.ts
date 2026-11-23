import { describe, expect, it } from 'vitest'
import { createMemoryHistory, createRouter } from 'vue-router'

import { routes } from './routes'

describe('routes', () => {
  const router = createRouter({ history: createMemoryHistory(), routes })

  it('bilinmeyen adresi bulunamadı sayfasına çözer', () => {
    expect(router.resolve('/olmayan/adres').name).toBe('bulunamadi')
  })

  it('e-posta ve bildirim bağlantılarının yollarını açar', () => {
    expect(router.resolve('/login').name).toBe('login')
    expect(router.resolve('/izin-talepleri/42').name).toBe('izin-talebi')
    expect(router.resolve('/izin-talepleri/yeni').name).toBe('yeni-izin-talebi')
    expect(router.resolve('/calisanlar/7').name).toBe('calisan')
  })
})
