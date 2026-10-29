import type { Calisan, CalisanKaydetRequest } from '@/api/calisanlarApi'

export function bosCalisanFormu(): CalisanKaydetRequest {
  return {
    ad: '',
    soyad: '',
    departmanId: 0,
    iseGirisTarihi: '',
    dogumTarihi: '',
    hakBaslangicKidemYili: null,
  }
}

export function calisanFormaCevir(calisan: Calisan): CalisanKaydetRequest {
  return {
    ad: calisan.ad,
    soyad: calisan.soyad,
    departmanId: calisan.departmanId,
    iseGirisTarihi: calisan.iseGirisTarihi,
    dogumTarihi: calisan.dogumTarihi,
    hakBaslangicKidemYili: calisan.hakBaslangicKidemYili,
  }
}
