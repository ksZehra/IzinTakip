import { afterEach, describe, expect, it } from 'vitest'
import { createApp, defineComponent } from 'vue'
import { createMemoryHistory, createRouter, type Router } from 'vue-router'

import { useFiltreDurumu } from './useFiltreDurumu'

const varsayilan = { arama: '', durum: [] as string[] }
type Durum = ReturnType<typeof useFiltreDurumu<typeof varsayilan>>

async function kur(adres: string): Promise<{ durum: Durum; router: Router }> {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/liste', component: { render: () => null } }],
  })
  await router.push(adres)
  let durum!: Durum
  const Bilesen = defineComponent({
    setup() {
      durum = useFiltreDurumu(varsayilan, { gizliAlanlar: ['arama'] })
      return () => null
    },
  })
  createApp(Bilesen).use(router).mount(document.createElement('div'))
  return { durum, router }
}

describe('useFiltreDurumu', () => {
  afterEach(() => sessionStorage.clear())

  it('gizli alanı adrese değil sessionStorage’a yazar, seçim filtresini adrese yazar', async () => {
    const { durum, router } = await kur('/liste')

    durum.filtreyiAyarla('arama', 'ayşe')
    durum.filtreyiAyarla('durum', ['Beklemede'])
    durum.kaydet()

    await expect.poll(() => router.currentRoute.value.query).toEqual({ durum: ['Beklemede'] })
    expect(JSON.parse(sessionStorage.getItem('filtre:/liste') ?? '{}')).toEqual({ arama: 'ayşe' })
  })

  it('gizli alanı sessionStorage’dan okur, adresteki aynı adlı değeri yok sayar', async () => {
    sessionStorage.setItem('filtre:/liste', JSON.stringify({ arama: 'mehmet' }))

    const { durum } = await kur('/liste?arama=ali&durum=Onaylandi')

    expect(durum.filtre.arama).toBe('mehmet')
    expect(durum.filtre.durum).toEqual(['Onaylandi'])
  })

  it('arama temizlenince sessionStorage kaydını siler', async () => {
    sessionStorage.setItem('filtre:/liste', JSON.stringify({ arama: 'ali' }))
    const { durum } = await kur('/liste')

    durum.filtreyiSifirla()
    durum.kaydet()

    expect(sessionStorage.getItem('filtre:/liste')).toBeNull()
  })
})
