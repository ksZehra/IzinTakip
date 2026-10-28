using IzinTakip.Domain.Enums;

namespace IzinTakip.Domain.Izin;

public sealed record SureIstegi(
    SureTipi Tip,
    DateOnly Tarih,
    DateOnly? BitisTarihi,
    GunYarisi? GunYarisi,
    TimeOnly? BaslangicSaati,
    TimeOnly? BitisSaati);

public sealed record IzinSuresi(int Dakika, ZamanAraligi Aralik);
