using System.Globalization;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Common.Izin;

public static class TalepOzeti
{
    private static readonly CultureInfo Turkce = CultureInfo.GetCultureInfo("tr-TR");

    public static string Olustur(IzinTalebi talep, string turAdi, int gunlukDakika)
    {
        var tarih = talep.SureTipi switch
        {
            SureTipi.TamGun when talep.Baslangic.Date == talep.Bitis.Date => talep.Baslangic.ToString("d MMMM yyyy", Turkce),
            SureTipi.TamGun => $"{talep.Baslangic.ToString("d MMMM yyyy", Turkce)} – {talep.Bitis.ToString("d MMMM yyyy", Turkce)}",
            _ => $"{talep.Baslangic.ToString("d MMMM yyyy HH:mm", Turkce)}–{talep.Bitis.ToString("HH:mm", Turkce)}"
        };

        return $"{turAdi}: {tarih} ({SureMetni.Olustur(talep.Dakika, gunlukDakika)})";
    }
}
