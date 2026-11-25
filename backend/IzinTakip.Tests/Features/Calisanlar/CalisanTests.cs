using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Api.Common;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests.Features.Calisanlar;

public sealed class CalisanTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Olustur_GecerliBilgilerle_201VeDetayDoner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, "Ar-Ge");

        var yanit = await admin.PostAsJsonAsync(
            "/api/calisanlar",
            new { ad = "Deniz", soyad = "Aydın", departmanId, iseGirisTarihi = "2021-02-01", dogumTarihi = "1993-04-04" },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, yanit.StatusCode);
        Assert.NotNull(yanit.Headers.Location);
        Assert.Equal("Ar-Ge", govde.GetProperty("departmanAdi").GetString());
        Assert.Equal(
            IzinHakkiCalculator.IlkHakKidemYili(new DateOnly(2021, 2, 1), factory.Services.GetRequiredService<SirketSaati>().Bugun()),
            govde.GetProperty("hakBaslangicKidemYili").GetInt32());
    }

    [Fact]
    public async Task Guncelle_HakBaslangicYili_VerilirseDegisirVerilmezseKorunur()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var calisanId = await TestData.CalisanEkleAsync(admin, "Selin", departmanId);
        var govde = new { ad = "Selin", soyad = "Test", departmanId, iseGirisTarihi = "2020-01-06", dogumTarihi = "1990-05-15", hakBaslangicKidemYili = (int?)3 };

        var verilen = await (await admin.PutAsJsonAsync($"/api/calisanlar/{calisanId}", govde, TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var verilmeyen = await (await admin.PutAsJsonAsync($"/api/calisanlar/{calisanId}", govde with { iseGirisTarihi = "2019-12-06", hakBaslangicKidemYili = (int?)null }, TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var gecersiz = await admin.PutAsJsonAsync($"/api/calisanlar/{calisanId}", govde with { hakBaslangicKidemYili = (int?)0 }, TestContext.Current.CancellationToken);

        Assert.Equal(3, verilen.GetProperty("hakBaslangicKidemYili").GetInt32());
        Assert.Equal(3, verilmeyen.GetProperty("hakBaslangicKidemYili").GetInt32());
        Assert.Equal(HttpStatusCode.BadRequest, gecersiz.StatusCode);
    }

    [Fact]
    public async Task DogumTarihiIseGiristenSonra_Returns400()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, "Kalite");

        var yanit = await admin.PostAsJsonAsync(
            "/api/calisanlar",
            new { ad = "Hatalı", soyad = "Tarih", departmanId, iseGirisTarihi = "2021-02-01", dogumTarihi = "2022-01-01" },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Doğum tarihi işe giriş tarihinden önce olmalı.", govde.GetProperty("errors").GetProperty("dogumTarihi")[0].GetString());
    }

    [Fact]
    public async Task Olustur_BosForm_ZorunluAlanlarinHepsiTekYanittaDoner()
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.PostAsJsonAsync(
            "/api/calisanlar",
            new { ad = "", soyad = "", departmanId = 0, iseGirisTarihi = (string?)null, dogumTarihi = (string?)null },
            TestContext.Current.CancellationToken);
        var hatalar = (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("errors");

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Ad zorunludur.", hatalar.GetProperty("ad")[0].GetString());
        Assert.Equal("Soyad zorunludur.", hatalar.GetProperty("soyad")[0].GetString());
        Assert.Equal("Departman seçin.", hatalar.GetProperty("departmanId")[0].GetString());
        Assert.Equal("İşe giriş tarihi zorunludur.", hatalar.GetProperty("iseGirisTarihi")[0].GetString());
        Assert.Equal("Doğum tarihi zorunludur.", hatalar.GetProperty("dogumTarihi")[0].GetString());
    }

    [Fact]
    public async Task Liste_Yonetici_YalnizKendisiniVeYonettigiDepartmaniGorur()
    {
        var admin = await SuperAdminAsync();
        var ekipDepartmani = await TestData.DepartmanEkleAsync(admin, "Destek");
        var digerDepartman = await TestData.DepartmanEkleAsync(admin, "Pazarlama");
        var yoneticiId = await TestData.CalisanEkleAsync(admin, "Yönetici Selin", ekipDepartmani);
        await TestData.DepartmanYoneticisiAtaAsync(admin, ekipDepartmani, "Destek", yoneticiId);
        await TestData.CalisanEkleAsync(admin, "Ekip Ali", ekipDepartmani);
        await TestData.CalisanEkleAsync(admin, "Dışarıda Veli", digerDepartman);
        await TestAuth.KullaniciOlusturAsync(factory, "selin@izintakip.test", SistemRolleri.Yonetici, calisanId: yoneticiId);
        var yonetici = await TestAuth.OturumluClientAsync(factory, "selin@izintakip.test");

        var liste = await yonetici.GetFromJsonAsync<JsonElement>("/api/calisanlar?pageSize=100", TestContext.Current.CancellationToken);
        var adlar = liste.GetProperty("items").EnumerateArray().Select(c => c.GetProperty("ad").GetString()).ToList();

        Assert.Equal(["Ekip Ali", "Yönetici Selin"], adlar);
        Assert.Equal(2, liste.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Liste_FazlaBuyukSayfaNumarasi_SonSayfayiDoner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, "Sayfalama Deneme");
        await TestData.CalisanEkleAsync(admin, "Birinci", departmanId);
        await TestData.CalisanEkleAsync(admin, "Ikinci", departmanId);
        await TestData.CalisanEkleAsync(admin, "Ucuncu", departmanId);

        var liste = await admin.GetFromJsonAsync<JsonElement>(
            $"/api/calisanlar?departmanId={departmanId}&pageSize=2&page=999",
            TestContext.Current.CancellationToken);

        Assert.Equal(2, liste.GetProperty("page").GetInt32());
        Assert.Equal(1, liste.GetProperty("items").GetArrayLength());
        Assert.Equal(3, liste.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Detay_KapsamDisindakiCalisan_404Doner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, "Hukuk");
        var kendisiId = await TestData.CalisanEkleAsync(admin, "Kendisi", departmanId);
        var baskasiId = await TestData.CalisanEkleAsync(admin, "Başkası", departmanId);
        await TestAuth.KullaniciOlusturAsync(factory, "kendisi@izintakip.test", SistemRolleri.Calisan, calisanId: kendisiId);
        var calisan = await TestAuth.OturumluClientAsync(factory, "kendisi@izintakip.test");

        var kendi = await calisan.GetAsync($"/api/calisanlar/{kendisiId}", TestContext.Current.CancellationToken);
        var baskasi = await calisan.GetAsync($"/api/calisanlar/{baskasiId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, kendi.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, baskasi.StatusCode);
    }

    [Fact]
    public async Task Liste_CalisanRolu_403Doner()
    {
        await TestAuth.KullaniciOlusturAsync(factory, "liste-calisan@izintakip.test", SistemRolleri.Calisan);
        var calisan = await TestAuth.OturumluClientAsync(factory, "liste-calisan@izintakip.test");

        var yanit = await calisan.GetAsync("/api/calisanlar", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
    }

    [Fact]
    public async Task Pasiflestir_DepartmanYoneticisiyse_422Doner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, "Bilgi İşlem");
        var yoneticiId = await TestData.CalisanEkleAsync(admin, "Yönetici Okan", departmanId);
        await TestData.DepartmanYoneticisiAtaAsync(admin, departmanId, "Bilgi İşlem", yoneticiId);

        var yanit = await admin.PutAsJsonAsync($"/api/calisanlar/{yoneticiId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
    }

    [Fact]
    public async Task PasifKullanici_GirisYapamaz()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, "Üretim");
        var calisanId = await TestData.CalisanEkleAsync(admin, "Ayrılan Mert", departmanId);
        await TestAuth.KullaniciOlusturAsync(factory, "mert@izintakip.test", SistemRolleri.Calisan, calisanId: calisanId);
        var calisan = await TestAuth.OturumluClientAsync(factory, "mert@izintakip.test");

        var pasiflestir = await admin.PutAsJsonAsync($"/api/calisanlar/{calisanId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);
        var sonraki = await calisan.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, pasiflestir.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, sonraki.StatusCode);
    }

    [Fact]
    public async Task HakEkle_ElleSatirlar_ToplamaYansir()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, "Finans");
        var calisanId = await TestData.CalisanEkleAsync(admin, "Bakiye Eda", departmanId);

        await admin.PostAsJsonAsync($"/api/calisanlar/{calisanId}/haklar", new { dakika = 14 * 480, aciklama = "Açılış bakiyesi" }, TestContext.Current.CancellationToken);
        var yanit = await admin.PostAsJsonAsync($"/api/calisanlar/{calisanId}/haklar", new { dakika = -240, aciklama = "Düzeltme" }, TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal(2, govde.GetProperty("satirlar").GetArrayLength());
        Assert.Equal(13 * 480 + 240, govde.GetProperty("toplamDakika").GetInt32());
        Assert.Equal("13 gün 4 saat", govde.GetProperty("toplamSureMetni").GetString());
    }

    [Fact]
    public async Task HakEkle_AsiriBuyukSureVeUcTarih_400Doner()
    {
        var admin = await SuperAdminAsync();
        var calisanId = await TestData.CalisanEkleAsync(admin, "Sinir", await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));

        var buyuk = await admin.PostAsJsonAsync($"/api/calisanlar/{calisanId}/haklar", new { dakika = 2_000_000_000, aciklama = "Deneme" }, TestContext.Current.CancellationToken);
        var tarih = await admin.PostAsJsonAsync(
            $"/api/calisanlar/{calisanId}/haklar",
            new { dakika = 480, aciklama = "Deneme", kazanimTarihi = "9999-12-31" },
            TestContext.Current.CancellationToken);
        var govde = await buyuk.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, buyuk.StatusCode);
        Assert.Equal("Bir hak satırı en çok bir yıllık süre olabilir.", govde.GetProperty("errors").GetProperty("dakika")[0].GetString());
        Assert.Equal(HttpStatusCode.BadRequest, tarih.StatusCode);
    }

    [Fact]
    public async Task HakGuncelle_ElleSatir_SureAciklamaVeToplamDegisir()
    {
        var admin = await SuperAdminAsync();
        var calisanId = await TestData.CalisanEkleAsync(admin, "Düzelt Ece", await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));
        var hakId = await HakEkleAsync(admin, calisanId, 20 * 480, "Yanlış giriş");

        var yanit = await admin.PutAsJsonAsync(
            $"/api/calisanlar/{calisanId}/haklar/{hakId}",
            new { dakika = 14 * 480, aciklama = "Doğru giriş", kazanimTarihi = "2026-03-02" },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        var satir = Assert.Single(govde.GetProperty("satirlar").EnumerateArray());
        Assert.Equal("Doğru giriş", satir.GetProperty("aciklama").GetString());
        Assert.Equal("2026-03-02", satir.GetProperty("kazanimTarihi").GetString());
        Assert.Equal(14 * 480, govde.GetProperty("toplamDakika").GetInt32());
    }

    [Fact]
    public async Task HakSil_ElleSatir_ToplamdanDuserVeIslemKaydinaYazilir()
    {
        var admin = await SuperAdminAsync();
        var calisanId = await TestData.CalisanEkleAsync(admin, "Sil Selim", await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));
        var kalan = await HakEkleAsync(admin, calisanId, 5 * 480, "Kalacak");
        var silinecek = await HakEkleAsync(admin, calisanId, 20 * 480, "Yanlış");

        var yanit = await admin.DeleteAsync($"/api/calisanlar/{calisanId}/haklar/{silinecek}", TestContext.Current.CancellationToken);
        var haklar = await admin.GetFromJsonAsync<JsonElement>($"/api/calisanlar/{calisanId}/haklar", TestContext.Current.CancellationToken);
        var kayitlar = await admin.GetFromJsonAsync<JsonElement>($"/api/islem-kayitlari?calisanId={calisanId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, yanit.StatusCode);
        Assert.Equal(kalan, Assert.Single(haklar.GetProperty("satirlar").EnumerateArray()).GetProperty("id").GetInt32());
        Assert.Equal(5 * 480, haklar.GetProperty("toplamDakika").GetInt32());
        Assert.Contains(kayitlar.GetProperty("items").EnumerateArray(), k => k.GetProperty("tablo").GetString() == "IzinHakki" && k.GetProperty("islem").GetString() == "Silindi");
    }

    [Fact]
    public async Task HakGuncelleVeSil_OtomatikSatir_422DonerSatirKorunur()
    {
        var admin = await SuperAdminAsync();
        var calisanId = await TestData.CalisanEkleAsync(admin, "Otomatik Oya", await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));
        int hakId;
        using (var kapsam = factory.Services.CreateScope())
        {
            var db = kapsam.ServiceProvider.GetRequiredService<IzinTakipDbContext>();
            var hak = IzinHakki.Kural(calisanId, 1, new DateOnly(2026, 1, 1), 14 * 480, "1. yıl hakkı");
            db.IzinHaklari.Add(hak);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            hakId = hak.Id;
        }

        var guncelle = await admin.PutAsJsonAsync($"/api/calisanlar/{calisanId}/haklar/{hakId}", new { dakika = 480, aciklama = "Değişti" }, TestContext.Current.CancellationToken);
        var sil = await admin.DeleteAsync($"/api/calisanlar/{calisanId}/haklar/{hakId}", TestContext.Current.CancellationToken);
        var haklar = await admin.GetFromJsonAsync<JsonElement>($"/api/calisanlar/{calisanId}/haklar", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, guncelle.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sil.StatusCode);
        Assert.Equal(14 * 480, haklar.GetProperty("toplamDakika").GetInt32());
    }

    [Fact]
    public async Task HakGuncelleVeSil_BaskaCalisaninSatiri_404Doner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var sahibi = await TestData.CalisanEkleAsync(admin, "Sahip Sema", departmanId);
        var baskasi = await TestData.CalisanEkleAsync(admin, "Başka Baki", departmanId);
        var hakId = await HakEkleAsync(admin, sahibi, 480, "Sema'nın hakkı");

        var guncelle = await admin.PutAsJsonAsync($"/api/calisanlar/{baskasi}/haklar/{hakId}", new { dakika = 960, aciklama = "Deneme" }, TestContext.Current.CancellationToken);
        var sil = await admin.DeleteAsync($"/api/calisanlar/{baskasi}/haklar/{hakId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, guncelle.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, sil.StatusCode);
    }

    [Fact]
    public async Task HakGuncelleVeSil_CalisanRolu_403Doner()
    {
        var admin = await SuperAdminAsync();
        var calisanId = await TestData.CalisanEkleAsync(admin, "Yetkisiz Yaren", await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));
        var hakId = await HakEkleAsync(admin, calisanId, 480, "Korunan");
        var eposta = $"yaren-{calisanId}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan, calisanId: calisanId);
        var calisan = await TestAuth.OturumluClientAsync(factory, eposta);

        var guncelle = await calisan.PutAsJsonAsync($"/api/calisanlar/{calisanId}/haklar/{hakId}", new { dakika = 99999, aciklama = "Sahte" }, TestContext.Current.CancellationToken);
        var sil = await calisan.DeleteAsync($"/api/calisanlar/{calisanId}/haklar/{hakId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, guncelle.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, sil.StatusCode);
    }

    [Fact]
    public async Task Listele_AdSoyadAramasi_TamAdlaBulur()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        await TestData.CalisanEkleAsync(admin, "Aranan Ali", departmanId);
        await TestData.CalisanEkleAsync(admin, "Başka Veli", departmanId);

        var liste = await ListeAsync(admin, $"departmanId={departmanId}&arama=Ali Test");

        Assert.Equal(["Aranan Ali"], Adlar(liste));
    }

    [Fact]
    public async Task Listele_CokluDepartman_YalnizSecilenleriGetirir()
    {
        var admin = await SuperAdminAsync();
        var ilk = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var ikinci = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var ucuncu = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        await TestData.CalisanEkleAsync(admin, "Birinci", ilk);
        await TestData.CalisanEkleAsync(admin, "İkinci", ikinci);
        await TestData.CalisanEkleAsync(admin, "Üçüncü", ucuncu);

        var liste = await ListeAsync(admin, $"departmanId={ilk}&departmanId={ucuncu}");

        Assert.Equal(["Birinci", "Üçüncü"], Adlar(liste));
        Assert.Equal(2, liste.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Listele_IseGirisAraligi_BaslangicVeBitisiKapsar()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        await TestData.CalisanEkleAsync(admin, "Eski", departmanId, new DateOnly(2018, 3, 5));
        await TestData.CalisanEkleAsync(admin, "Orta", departmanId, new DateOnly(2020, 6, 1));
        await TestData.CalisanEkleAsync(admin, "Yeni", departmanId, new DateOnly(2022, 9, 1));

        var arasi = await ListeAsync(admin, $"departmanId={departmanId}&iseGirisBaslangic=2019-01-01&iseGirisBitis=2021-12-31");
        var sonrasi = await ListeAsync(admin, $"departmanId={departmanId}&iseGirisBaslangic=2020-06-01");
        var oncesi = await ListeAsync(admin, $"departmanId={departmanId}&iseGirisBitis=2020-06-01");

        Assert.Equal(["Orta"], Adlar(arasi));
        Assert.Equal(["Orta", "Yeni"], Adlar(sonrasi));
        Assert.Equal(["Eski", "Orta"], Adlar(oncesi));
    }

    [Fact]
    public async Task Listele_Sirala_SecilenAlanaGoreArtanVeAzalanDoner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        await TestData.CalisanEkleAsync(admin, "Ahmet", departmanId, new DateOnly(2022, 1, 3));
        await TestData.CalisanEkleAsync(admin, "Zeynep", departmanId, new DateOnly(2019, 1, 7));
        await TestData.CalisanEkleAsync(admin, "Mert", departmanId, new DateOnly(2021, 1, 4));

        var adaGore = await ListeAsync(admin, $"departmanId={departmanId}&sort=adSoyad:desc");
        var tarihe = await ListeAsync(admin, $"departmanId={departmanId}&sort=iseGirisTarihi:asc");
        var varsayilan = await ListeAsync(admin, $"departmanId={departmanId}");

        Assert.Equal(["Zeynep", "Mert", "Ahmet"], Adlar(adaGore));
        Assert.Equal(["Zeynep", "Mert", "Ahmet"], Adlar(tarihe));
        Assert.Equal(["Ahmet", "Mert", "Zeynep"], Adlar(varsayilan));
    }

    [Theory]
    [InlineData("sort=sifre:asc", "sort")]
    [InlineData("sort=adSoyad", "sort")]
    [InlineData("sort=adSoyad:yukari", "sort")]
    [InlineData("iseGirisBaslangic=2022-01-01&iseGirisBitis=2021-01-01", "iseGirisBitis")]
    public async Task Listele_GecersizSiralamaVeAralik_400Doner(string sorgu, string alan)
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.GetAsync($"/api/calisanlar?{sorgu}", TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.True(govde.GetProperty("errors").TryGetProperty(alan, out _));
    }

    private static async Task<JsonElement> ListeAsync(HttpClient istemci, string sorgu) =>
        await istemci.GetFromJsonAsync<JsonElement>($"/api/calisanlar?{sorgu}", TestContext.Current.CancellationToken);

    private static string[] Adlar(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray()
            .Select(c => c.GetProperty("ad").GetString()!)
            .ToArray();

    private static async Task<int> HakEkleAsync(HttpClient istemci, int calisanId, int dakika, string aciklama)
    {
        var yanit = await istemci.PostAsJsonAsync($"/api/calisanlar/{calisanId}/haklar", new { dakika, aciklama }, TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        return govde.GetProperty("satirlar").EnumerateArray().Single(s => s.GetProperty("aciklama").GetString() == aciklama).GetProperty("id").GetInt32();
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
