namespace IzinTakip.Domain;

public static class SureMetni
{
    public static string Olustur(int dakika, int gunlukDakika)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gunlukDakika);

        if (dakika == 0)
        {
            return "0 dk";
        }

        var isaret = dakika < 0 ? "-" : string.Empty;
        var mutlak = Math.Abs(dakika);
        if (mutlak * 2 == gunlukDakika)
        {
            return isaret + "yarım gün";
        }

        var gun = mutlak / gunlukDakika;
        var saat = mutlak % gunlukDakika / 60;
        var dk = mutlak % gunlukDakika % 60;

        var parcalar = new List<string>(3);
        if (gun > 0)
        {
            parcalar.Add($"{gun} gün");
        }

        if (saat > 0)
        {
            parcalar.Add($"{saat} saat");
        }

        if (dk > 0)
        {
            parcalar.Add($"{dk} dk");
        }

        return isaret + string.Join(" ", parcalar);
    }
}
