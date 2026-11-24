export type TemaSecimi = 'light' | 'dark' | 'system'

const ANAHTAR = 'izintakip-tema'

export function kayitliTema(): TemaSecimi {
  try {
    const deger = localStorage.getItem(ANAHTAR)
    return deger === 'light' || deger === 'dark' ? deger : 'system'
  } catch {
    return 'system'
  }
}

export function temaKaydet(secim: TemaSecimi) {
  try {
    localStorage.setItem(ANAHTAR, secim)
  } catch {
    return
  }
}
