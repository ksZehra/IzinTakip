namespace IzinTakip.Domain.Entities;

public sealed class Departman : Entity
{
    public Departman(string ad, int? yoneticiId)
    {
        Ad = ad;
        YoneticiId = yoneticiId;
        Aktif = true;
    }

    public string Ad { get; private set; }
    public int? YoneticiId { get; private set; }
    public bool Aktif { get; private set; }

    public void Guncelle(string ad, int? yoneticiId)
    {
        Ad = ad;
        YoneticiId = yoneticiId;
    }

    public void AktiflikDegistir(bool aktif) => Aktif = aktif;
}
