namespace IzinTakip.Domain.Entities;

public sealed class Bildirim : Entity
{
    public Bildirim(int kullaniciId, string baslik, string metin, string? baglanti)
    {
        KullaniciId = kullaniciId;
        Baslik = baslik;
        Metin = metin;
        Baglanti = baglanti;
    }

    public int KullaniciId { get; private set; }
    public string Baslik { get; private set; }
    public string Metin { get; private set; }
    public string? Baglanti { get; private set; }
    public bool OkunduMu { get; private set; }

    public void OkunduIsaretle() => OkunduMu = true;
}
