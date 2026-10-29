import type { RouteLocationNormalized, RouteLocationRaw, Router } from 'vue-router'

import type { Oturum } from '@/api/authApi'

export function rotaKarari(
  hedef: Pick<RouteLocationNormalized, 'name' | 'meta' | 'fullPath'>,
  oturum: Oturum | null,
): RouteLocationRaw | true {
  if (hedef.meta.herkeseAcik) {
    return oturum && hedef.name === 'giris' ? { name: 'ana-sayfa' } : true
  }
  if (!oturum) {
    return { name: 'giris', query: hedef.fullPath === '/' ? {} : { donus: hedef.fullPath } }
  }
  if (oturum.parolaDegistirmeli && hedef.name !== 'parola-degistir') {
    return { name: 'parola-degistir' }
  }
  const gerekenler = hedef.meta.yetki ?? []
  if (gerekenler.length > 0 && !gerekenler.some((yetki) => oturum.yetkiler.includes(yetki))) {
    return { name: 'yetkisiz' }
  }
  return true
}

export function guvenliDonusAdresi(deger: unknown, router: Router): string {
  if (typeof deger !== 'string' || !deger.startsWith('/') || /^\/[/\\]/.test(deger)) {
    return '/'
  }
  const hedef = router.resolve(deger)
  return hedef.name === 'bulunamadi' || hedef.meta.herkeseAcik ? '/' : hedef.fullPath
}
