export const Yetkiler = {
  TumSirket: 'TumSirket',
  RolYonetimi: 'Rol.Yonet',
  IzinTalebiOlustur: 'IzinTalebi.Olustur',
  IzinTalebiYonet: 'IzinTalebi.Yonet',
  IzinTalebiDuzelt: 'IzinTalebi.Duzelt',
  RaporGor: 'Rapor.Gor',
  CalisanYonet: 'Calisan.Yonet',
  TanimYonet: 'Tanim.Yonet',
  KullaniciYonet: 'Kullanici.Yonet',
  DenetimGor: 'Denetim.Gor',
} as const

export type Yetki = (typeof Yetkiler)[keyof typeof Yetkiler]
