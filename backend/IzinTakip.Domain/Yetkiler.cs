namespace IzinTakip.Domain;

public static class Yetkiler
{
    public const string ClaimType = "yetki";
    public const string TumSirket = "TumSirket";
    public const string RolYonetimi = "Rol.Yonet";

    public static class IzinTalebi
    {
        public const string Olustur = "IzinTalebi.Olustur";
        public const string Yonet = "IzinTalebi.Yonet";
        public const string Duzelt = "IzinTalebi.Duzelt";
    }

    public static class Rapor
    {
        public const string Gor = "Rapor.Gor";
    }

    public static class Calisan
    {
        public const string Yonet = "Calisan.Yonet";
    }

    public static class Tanim
    {
        public const string Yonet = "Tanim.Yonet";
    }

    public static class Kullanici
    {
        public const string Yonet = "Kullanici.Yonet";
    }

    public static class Denetim
    {
        public const string Gor = "Denetim.Gor";
    }

    public static IReadOnlyList<YetkiTanimi> Atanabilir { get; } =
    [
        new(IzinTalebi.Olustur, "Kendi adına izin talebi açma, kendi talebini iptal etme, kendi bakiyesini görme"),
        new(IzinTalebi.Yonet, "Kapsamdaki çalışanlar adına talep açma; onaylama, reddetme, iptal etme"),
        new(IzinTalebi.Duzelt, "Geçmiş tarihli talep açma, başlamış talebi iptal etme"),
        new(Rapor.Gor, "Kapsamdaki raporları ve Excel çıktılarını görme"),
        new(Calisan.Yonet, "Çalışan ekleme, düzenleme, pasifleştirme; elle hak satırı ekleme"),
        new(Tanim.Yonet, "Departman, izin türü, resmi tatil, hak kuralı ve iş takvimi tanımları"),
        new(Kullanici.Yonet, "Kullanıcı hesabı açma, pasifleştirme, parola sıfırlama, korumasız rol atama"),
        new(Denetim.Gor, "İşlem kayıtlarını görme: kim, ne zaman, hangi kaydı nasıl değiştirdi"),
        new(TumSirket, "Kapsamı tüm şirket yapar")
    ];
}

public sealed record YetkiTanimi(string Kod, string Aciklama);
