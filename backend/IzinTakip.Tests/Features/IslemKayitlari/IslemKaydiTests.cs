using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.IslemKayitlari;

public sealed class IslemKaydiTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task HakEkle_IkKendiKaydina_KendiKaydiOlarakGorunur()
    {
        var admin = await SuperAdminAsync();
        var calisanId = await TestData.CalisanEkleAsync(admin, "İK Kişi", await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));
        var ikEposta = $"ik-{calisanId}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, ikEposta, SistemRolleri.IK, calisanId: calisanId);
        var ik = await TestAuth.OturumluClientAsync(factory, ikEposta);

        await ik.PostAsJsonAsync($"/api/calisanlar/{calisanId}/haklar", new { dakika = 4800, aciklama = "Düzeltme" }, TestContext.Current.CancellationToken);
        var kayitlar = await admin.GetFromJsonAsync<JsonElement>(
            $"/api/islem-kayitlari?calisanId={calisanId}&sadeceKendiKaydi=true",
            TestContext.Current.CancellationToken);

        var kayit = Assert.Single(kayitlar.GetProperty("items").EnumerateArray());
        Assert.Equal("IzinHakki", kayit.GetProperty("tablo").GetString());
        Assert.Equal("Eklendi", kayit.GetProperty("islem").GetString());
        Assert.Equal("İK Kişi Test", kayit.GetProperty("kullaniciAdi").GetString());
        Assert.True(kayit.GetProperty("kendiKaydi").GetBoolean());
        Assert.Contains(kayit.GetProperty("degisiklikler").EnumerateArray(), d => d.GetProperty("alan").GetString() == "Dakika" && d.GetProperty("yeni").GetString() == "4800");
    }

    [Fact]
    public async Task Listele_IkGoremezSuperAdminGorur()
    {
        var ikEposta = $"ik-{Guid.NewGuid():N}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, ikEposta, SistemRolleri.IK);
        var ik = await TestAuth.OturumluClientAsync(factory, ikEposta);
        var admin = await SuperAdminAsync();

        var ikYaniti = await ik.GetAsync("/api/islem-kayitlari", TestContext.Current.CancellationToken);
        var adminYaniti = await admin.GetAsync("/api/islem-kayitlari", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, ikYaniti.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminYaniti.StatusCode);
    }

    [Fact]
    public async Task CalisanGuncelle_YalnizDegisenAlanEskiVeYeniDegeriyleYazilir()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var calisanId = await TestData.CalisanEkleAsync(admin, "Tarihi Düzelen", departmanId);

        await admin.PutAsJsonAsync(
            $"/api/calisanlar/{calisanId}",
            new { ad = "Tarihi Düzelen", soyad = "Test", departmanId, iseGirisTarihi = "2019-12-06", dogumTarihi = "1990-05-15" },
            TestContext.Current.CancellationToken);
        var kayitlar = await admin.GetFromJsonAsync<JsonElement>(
            $"/api/islem-kayitlari?calisanId={calisanId}&tablo=Calisan",
            TestContext.Current.CancellationToken);

        var guncelleme = kayitlar.GetProperty("items").EnumerateArray().Single(k => k.GetProperty("islem").GetString() == "Guncellendi");
        var degisiklik = Assert.Single(guncelleme.GetProperty("degisiklikler").EnumerateArray());
        Assert.Equal("IseGirisTarihi", degisiklik.GetProperty("alan").GetString());
        Assert.Equal("2020-01-06", degisiklik.GetProperty("eski").GetString());
        Assert.Equal("2019-12-06", degisiklik.GetProperty("yeni").GetString());
        Assert.False(guncelleme.GetProperty("kendiKaydi").GetBoolean());
        Assert.Equal(calisanId, guncelleme.GetProperty("kayitId").GetInt32());
    }

    [Fact]
    public async Task ParolaDegistir_ParolaOzetiKaydaYazilmaz()
    {
        var eposta = $"parola-{Guid.NewGuid():N}@izintakip.test";
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        var kullanici = await TestAuth.OturumluClientAsync(factory, eposta);
        await kullanici.PostAsJsonAsync(
            "/api/auth/parola-degistir",
            new { mevcutParola = TestAuth.VarsayilanParola, yeniParola = "YeniParola2026" },
            TestContext.Current.CancellationToken);

        var yanit = await (await SuperAdminAsync()).GetStringAsync($"/api/islem-kayitlari?tablo=Kullanici&kullaniciId={kullaniciId}", TestContext.Current.CancellationToken);
        var kayit = Assert.Single(JsonDocument.Parse(yanit).RootElement.GetProperty("items").EnumerateArray());

        Assert.True(kayit.GetProperty("kendiKaydi").GetBoolean());
        Assert.Contains(kayit.GetProperty("degisiklikler").EnumerateArray(), d => d.GetProperty("alan").GetString() == "Parola" && d.GetProperty("yeni").GetString() == "değiştirildi");
        Assert.DoesNotContain("AQAAAA", yanit);
    }

    [Fact]
    public async Task HataliGiris_ParolaDegistiKaydiYazilmaz()
    {
        var eposta = $"hatali-{Guid.NewGuid():N}@izintakip.test";
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        var client = factory.CreateClient();

        var giris = await client.PostAsJsonAsync("/api/auth/login", new { eposta, parola = "Yanlis2026" }, TestContext.Current.CancellationToken);
        var kayitlar = await (await SuperAdminAsync()).GetFromJsonAsync<JsonElement>(
            "/api/islem-kayitlari?tablo=Kullanici&pageSize=100",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, giris.StatusCode);
        Assert.DoesNotContain(kayitlar.GetProperty("items").EnumerateArray(), k => k.GetProperty("kayitId").ValueKind == JsonValueKind.Number && k.GetProperty("kayitId").GetInt32() == kullaniciId);
    }

    [Fact]
    public async Task RolDegistir_RolAdiylaYazilir()
    {
        var admin = await SuperAdminAsync();
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, $"terfi-{Guid.NewGuid():N}@izintakip.test", SistemRolleri.Calisan);

        await admin.PutAsJsonAsync($"/api/kullanicilar/{kullaniciId}/roller", new { roller = new[] { SistemRolleri.Yonetici } }, TestContext.Current.CancellationToken);
        var kayitlar = await admin.GetFromJsonAsync<JsonElement>("/api/islem-kayitlari?tablo=KullaniciRolu&pageSize=100", TestContext.Current.CancellationToken);

        var ilgili = kayitlar.GetProperty("items").EnumerateArray().Where(k => k.GetProperty("ilgiliKullaniciId").GetInt32() == kullaniciId).ToList();
        Assert.Contains(ilgili, k => k.GetProperty("islem").GetString() == "Eklendi" && RolDegeri(k, "yeni") == SistemRolleri.Yonetici);
        Assert.Contains(ilgili, k => k.GetProperty("islem").GetString() == "Silindi" && RolDegeri(k, "eski") == SistemRolleri.Calisan);
    }

    private static string? RolDegeri(JsonElement kayit, string taraf) =>
        kayit.GetProperty("degisiklikler").EnumerateArray().Single(d => d.GetProperty("alan").GetString() == "RoleId").GetProperty(taraf).GetString();

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
