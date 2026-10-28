namespace IzinTakip.Domain.Entities;

public abstract class Entity
{
    public int Id { get; private set; }
    public DateTime OlusturmaTarihi { get; private set; }
    public int? OlusturanKullaniciId { get; private set; }
    public DateTime? GuncellemeTarihi { get; private set; }
    public int? GuncelleyenKullaniciId { get; private set; }
}
