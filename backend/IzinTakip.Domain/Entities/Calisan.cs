namespace IzinTakip.Domain.Entities;

public sealed class Calisan : Entity
{
    public Calisan(string ad, string soyad, int departmanId, DateOnly iseGirisTarihi, DateOnly dogumTarihi, int hakBaslangicKidemYili)
    {
        Ad = ad;
        Soyad = soyad;
        DepartmanId = departmanId;
        IseGirisTarihi = iseGirisTarihi;
        DogumTarihi = dogumTarihi;
        HakBaslangicKidemYili = hakBaslangicKidemYili;
        Aktif = true;
    }

    public string Ad { get; private set; }
    public string Soyad { get; private set; }
    public int DepartmanId { get; private set; }
    public DateOnly IseGirisTarihi { get; private set; }
    public DateOnly DogumTarihi { get; private set; }
    public int HakBaslangicKidemYili { get; private set; }
    public bool Aktif { get; private set; }
    public Departman Departman { get; private set; } = null!;

    public void Guncelle(string ad, string soyad, int departmanId, DateOnly iseGirisTarihi, DateOnly dogumTarihi, int hakBaslangicKidemYili)
    {
        Ad = ad;
        Soyad = soyad;
        DepartmanId = departmanId;
        IseGirisTarihi = iseGirisTarihi;
        DogumTarihi = dogumTarihi;
        HakBaslangicKidemYili = hakBaslangicKidemYili;
    }

    public void AktiflikDegistir(bool aktif) => Aktif = aktif;
}
