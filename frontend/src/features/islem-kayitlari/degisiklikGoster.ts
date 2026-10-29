import { gunHesabiEtiketleri, rolEtiketi } from '@/common/utils/etiketler'

const alanEtiketleri: Record<string, string> = {
  Ad: 'Ad',
  Soyad: 'Soyad',
  Aktif: 'Aktif',
  Aciklama: 'Açıklama',
  DepartmanId: 'Departman',
  YoneticiId: 'Yönetici',
  CalisanId: 'Çalışan',
  IseGirisTarihi: 'İşe giriş tarihi',
  DogumTarihi: 'Doğum tarihi',
  HakBaslangicKidemYili: 'Hak başlangıç kıdem yılı',
  Dakika: 'Süre',
  Kaynak: 'Kaynak',
  KazanimTarihi: 'Kazanım tarihi',
  KidemYili: 'Kıdem yılı',
  BakiyedenDuser: 'Bakiyeden düşer',
  BelgeZorunlu: 'Belge zorunlu',
  GunHesabi: 'Gün hesabı',
  TamGunAlinir: 'Tam gün alınır',
  YarimGunAlinir: 'Yarım gün alınır',
  SaatlikAlinir: 'Saatlik alınır',
  TalepBasinaEnFazlaDakika: 'Talep başına en fazla',
  YillikEnFazlaDakika: 'Yılda en fazla',
  MinKidemYil: 'En az kıdem (yıl)',
  MaxKidemYil: 'En çok kıdem (yıl)',
  MinYas: 'En az yaş',
  MaxYas: 'En çok yaş',
  Gun: 'Gün',
  Tarih: 'Tarih',
  YarimGun: 'Yarım gün tatil',
  CalismaGunleri: 'Çalışma günleri',
  MesaiBaslangic: 'Mesai başlangıcı',
  MesaiBitis: 'Mesai bitişi',
  OgleArasiBaslangic: 'Öğle arası başlangıcı',
  OgleArasiBitis: 'Öğle arası bitişi',
  SaatlikIzinAdimDakika: 'Saatlik izin adımı (dk)',
  HatirlatmaGunSayisi: 'Hatırlatma (gün)',
  Email: 'E-posta',
  ParolaDegistirmeliMi: 'Parola değiştirmeli',
  Parola: 'Parola',
  Name: 'Ad',
  Korumali: 'Korumalı',
  UserId: 'Kullanıcı',
  RoleId: 'Rol',
  ClaimValue: 'Yetki',
}

const kaynakEtiketleri: Record<string, string> = { Kural: 'Kurala göre', Elle: 'Elle girildi' }

export function alanEtiketi(alan: string) {
  return alanEtiketleri[alan] ?? alan
}

export function degerGoster(alan: string, deger: string | null) {
  if (deger === null) {
    return '—'
  }
  if (deger === 'True') {
    return 'Evet'
  }
  if (deger === 'False') {
    return 'Hayır'
  }
  if (alan === 'Kaynak') {
    return kaynakEtiketleri[deger] ?? deger
  }
  if (alan === 'GunHesabi') {
    return gunHesabiEtiketleri[deger as keyof typeof gunHesabiEtiketleri] ?? deger
  }
  if (alan === 'RoleId') {
    return rolEtiketi(deger)
  }
  if (/^\d{4}-\d{2}-\d{2}$/.test(deger)) {
    const [yil, ay, gun] = deger.split('-')
    return `${gun}.${ay}.${yil}`
  }
  return deger
}
