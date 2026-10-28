namespace IzinTakip.Domain.Entities;

public sealed class Departman : Entity
{
    public Departman(string ad)
    {
        Ad = ad;
        Aktif = true;
    }

    public string Ad { get; private set; }
    public bool Aktif { get; private set; }
}
