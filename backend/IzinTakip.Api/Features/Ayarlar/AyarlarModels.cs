using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Features.Ayarlar;

public sealed record AyarlarGuncelleRequest(
    IReadOnlyList<HaftaGunleri> CalismaGunleri,
    TimeOnly MesaiBaslangic,
    TimeOnly MesaiBitis,
    TimeOnly OgleArasiBaslangic,
    TimeOnly OgleArasiBitis,
    int SaatlikIzinAdimDakika,
    int HatirlatmaGunSayisi);

public sealed record AyarlarResponse(
    IReadOnlyList<HaftaGunleri> CalismaGunleri,
    TimeOnly MesaiBaslangic,
    TimeOnly MesaiBitis,
    TimeOnly OgleArasiBaslangic,
    TimeOnly OgleArasiBitis,
    int SaatlikIzinAdimDakika,
    int HatirlatmaGunSayisi,
    int GunlukCalismaDakikasi);
