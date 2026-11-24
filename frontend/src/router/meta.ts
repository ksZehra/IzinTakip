import 'vue-router'

import type { Ikon } from '@/common/utils/ikon'

import type { Yetki } from '@/common/auth/yetkiler'

export interface MenuMeta {
  baslik: string
  ikon: Ikon
  grup?: 'Yönetim'
}

declare module 'vue-router' {
  interface RouteMeta {
    baslik: string
    herkeseAcik?: boolean
    yetki?: Yetki[]
    menu?: MenuMeta
  }
}
