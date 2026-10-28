namespace IzinTakip.Domain;

public static class SistemRolleri
{
    public const string SuperAdmin = "SuperAdmin";
    public const string IK = "IK";
    public const string Yonetici = "Yonetici";
    public const string Calisan = "Calisan";

    public static IReadOnlyList<string> Tumu { get; } = [SuperAdmin, IK, Yonetici, Calisan];
}
