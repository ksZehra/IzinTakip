import 'vue-router'

import type { Yetki } from '@/common/auth/yetkiler'

export interface MenuBilgisi {
  baslik: string
  ikon: string
  grup?: 'Yönetim'
}

declare module 'vue-router' {
  interface RouteMeta {
    baslik: string
    herkeseAcik?: boolean
    yetki?: Yetki[]
    menu?: MenuBilgisi
  }
}
