namespace IzinTakip.Domain.Entities;

public sealed class IzinBelgesi : Entity
{
    public IzinBelgesi(int izinTalebiId, string dosyaAdi, string depoYolu, string icerikTuru, long boyut)
    {
        IzinTalebiId = izinTalebiId;
        DosyaAdi = dosyaAdi;
        DepoYolu = depoYolu;
        IcerikTuru = icerikTuru;
        Boyut = boyut;
    }

    public int IzinTalebiId { get; private set; }
    public string DosyaAdi { get; private set; }
    public string DepoYolu { get; private set; }
    public string IcerikTuru { get; private set; }
    public long Boyut { get; private set; }
}
