using IzinTakip.Domain;

namespace IzinTakip.Infrastructure.Identity;

public sealed record KullaniciYetkileri(
    int KullaniciId,
    string Eposta,
    int? CalisanId,
    bool Aktif,
    bool ParolaDegistirmeli,
    IReadOnlyList<string> Roller,
    IReadOnlySet<string> Yetkiler)
{
    public bool SuperAdmin => Roller.Contains(SistemRolleri.SuperAdmin);

    public bool VarMi(string yetki) => SuperAdmin || Yetkiler.Contains(yetki);
}
