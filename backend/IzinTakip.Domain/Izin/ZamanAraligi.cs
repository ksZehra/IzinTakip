namespace IzinTakip.Domain.Izin;

public readonly record struct ZamanAraligi(DateTime Baslangic, DateTime Bitis)
{
    public int Dakika => (int)(Bitis - Baslangic).TotalMinutes;

    public bool CakisirMi(ZamanAraligi diger) => Baslangic < diger.Bitis && diger.Baslangic < Bitis;

    public int KesisimDakika(ZamanAraligi diger)
    {
        var baslangic = Baslangic > diger.Baslangic ? Baslangic : diger.Baslangic;
        var bitis = Bitis < diger.Bitis ? Bitis : diger.Bitis;
        return bitis > baslangic ? (int)(bitis - baslangic).TotalMinutes : 0;
    }
}
