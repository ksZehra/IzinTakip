import { describe, expect, it } from 'vitest'
import { createMemoryHistory, createRouter } from 'vue-router'

import { routes } from './routes'

describe('routes', () => {
  it('bilinmeyen adresi bulunamadı sayfasına çözer', () => {
    const router = createRouter({ history: createMemoryHistory(), routes })

    expect(router.resolve('/olmayan/adres').name).toBe('bulunamadi')
  })
})
