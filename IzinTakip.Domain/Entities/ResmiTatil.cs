namespace IzinTakip.Domain.Entities;

public sealed class ResmiTatil : Entity
{
    public ResmiTatil(DateOnly tarih, string ad, bool yarimGun)
    {
        Tarih = tarih;
        Ad = ad;
        YarimGun = yarimGun;
    }

    public DateOnly Tarih { get; private set; }
    public string Ad { get; private set; }
    public bool YarimGun { get; private set; }
}
