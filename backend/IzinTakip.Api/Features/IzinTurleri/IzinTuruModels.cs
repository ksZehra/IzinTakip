using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Features.IzinTurleri;

public sealed record IzinTuruKaydetRequest(
    string Ad,
    bool BakiyedenDuser,
    bool BelgeZorunlu,
    GunHesabi GunHesabi,
    bool TamGunAlinir,
    bool YarimGunAlinir,
    bool SaatlikAlinir,
    int? TalepBasinaEnFazlaDakika,
    int? YillikEnFazlaDakika);

public sealed record IzinTuruResponse(
    int Id,
    string Ad,
    bool BakiyedenDuser,
    bool BelgeZorunlu,
    GunHesabi GunHesabi,
    bool TamGunAlinir,
    bool YarimGunAlinir,
    bool SaatlikAlinir,
    int? TalepBasinaEnFazlaDakika,
    string? TalepBasinaEnFazlaMetni,
    int? YillikEnFazlaDakika,
    string? YillikEnFazlaMetni,
    bool Aktif);
