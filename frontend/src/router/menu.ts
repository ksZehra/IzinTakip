import type { RouteRecordRaw } from 'vue-router'

import type { Yetki } from '@/common/auth/yetkiler'

import type { MenuMeta } from './meta'

export interface MenuItem extends MenuMeta {
  rotaAdi: string
}

export function menuOgeleri(
  rotalar: RouteRecordRaw[],
  yetkiVar: (...yetkiler: Yetki[]) => boolean,
): MenuItem[] {
  return rotalar.flatMap((rota) => {
    const altOgeler = rota.children ? menuOgeleri(rota.children, yetkiVar) : []
    const { menu, yetki = [] } = rota.meta ?? {}
    if (!menu || typeof rota.name !== 'string' || (yetki.length > 0 && !yetkiVar(...yetki))) {
      return altOgeler
    }
    return [{ ...menu, rotaAdi: rota.name }, ...altOgeler]
  })
}
