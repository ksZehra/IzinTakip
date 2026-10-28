using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Api.Common;
using IzinTakip.Domain;
using IzinTakip.Domain.Izin;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests.Features.Calisanlar;

public sealed class CalisanTests(VeritabanliApiFactory factory) : IClassFixture<VeritabanliApiFactory>
{
    [Fact]
    public async Task Olustur_GecerliBilgilerle_201VeDetayDoner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, "Ar-Ge");

        var yanit = await admin.PostAsJsonAsync(
            "/api/calisanlar",
            new { ad = "Deniz", soyad = "Aydın", departmanId, iseGirisTarihi = "2021-02-01", dogumTarihi = "1993-04-04" },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, yanit.StatusCode);
        Assert.NotNull(yanit.Headers.Location);
        Assert.Equal("Ar-Ge", govde.GetProperty("departmanAdi").GetString());
        Assert.Equal(
            IzinHakkiHesaplayici.IlkHakKidemYili(new DateOnly(2021, 2, 1), factory.Services.GetRequiredService<SirketSaati>().Bugun()),
            govde.GetProperty("hakBaslangicKidemYili").GetInt32());
    }

    [Fact]
    public async Task Guncelle_HakBaslangicYili_VerilirseDegisirVerilmezseKorunur()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Selin", departmanId);
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
    public async Task Olustur_DogumTarihiIseGiristenSonra_400VeAlanHatasiDoner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, "Kalite");

        var yanit = await admin.PostAsJsonAsync(
            "/api/calisanlar",
            new { ad = "Hatalı", soyad = "Tarih", departmanId, iseGirisTarihi = "2021-02-01", dogumTarihi = "2022-01-01" },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Doğum tarihi işe giriş tarihinden önce olmalı.", govde.GetProperty("errors").GetProperty("dogumTarihi")[0].GetString());
    }

    [Fact]
    public async Task Liste_Yonetici_YalnizKendisiniVeYonettigiDepartmaniGorur()
    {
        var admin = await SuperAdminAsync();
        var ekipDepartmani = await TestVeri.DepartmanEkleAsync(admin, "Destek");
        var digerDepartman = await TestVeri.DepartmanEkleAsync(admin, "Pazarlama");
        var yoneticiId = await TestVeri.CalisanEkleAsync(admin, "Yönetici Selin", ekipDepartmani);
        await TestVeri.DepartmanYoneticisiAtaAsync(admin, ekipDepartmani, "Destek", yoneticiId);
        await TestVeri.CalisanEkleAsync(admin, "Ekip Ali", ekipDepartmani);
        await TestVeri.CalisanEkleAsync(admin, "Dışarıda Veli", digerDepartman);
        await TestKimlik.KullaniciOlusturAsync(factory, "selin@izintakip.test", SistemRolleri.Yonetici, calisanId: yoneticiId);
        var yonetici = await TestKimlik.OturumluClientAsync(factory, "selin@izintakip.test");

        var liste = await yonetici.GetFromJsonAsync<JsonElement>("/api/calisanlar?pageSize=100", TestContext.Current.CancellationToken);
        var adlar = liste.GetProperty("items").EnumerateArray().Select(c => c.GetProperty("ad").GetString()).ToList();

        Assert.Equal(["Ekip Ali", "Yönetici Selin"], adlar);
        Assert.Equal(2, liste.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Detay_KapsamDisindakiCalisan_404Doner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, "Hukuk");
        var kendisiId = await TestVeri.CalisanEkleAsync(admin, "Kendisi", departmanId);
        var baskasiId = await TestVeri.CalisanEkleAsync(admin, "Başkası", departmanId);
        await TestKimlik.KullaniciOlusturAsync(factory, "kendisi@izintakip.test", SistemRolleri.Calisan, calisanId: kendisiId);
        var calisan = await TestKimlik.OturumluClientAsync(factory, "kendisi@izintakip.test");

        var kendi = await calisan.GetAsync($"/api/calisanlar/{kendisiId}", TestContext.Current.CancellationToken);
        var baskasi = await calisan.GetAsync($"/api/calisanlar/{baskasiId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, kendi.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, baskasi.StatusCode);
    }

    [Fact]
    public async Task Liste_CalisanRolu_403Doner()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "liste-calisan@izintakip.test", SistemRolleri.Calisan);
        var calisan = await TestKimlik.OturumluClientAsync(factory, "liste-calisan@izintakip.test");

        var yanit = await calisan.GetAsync("/api/calisanlar", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
    }

    [Fact]
    public async Task Pasiflestir_DepartmanYoneticisiyse_422Doner()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, "Bilgi İşlem");
        var yoneticiId = await TestVeri.CalisanEkleAsync(admin, "Yönetici Okan", departmanId);
        await TestVeri.DepartmanYoneticisiAtaAsync(admin, departmanId, "Bilgi İşlem", yoneticiId);

        var yanit = await admin.PutAsJsonAsync($"/api/calisanlar/{yoneticiId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
    }

    [Fact]
    public async Task Pasiflestir_BagliKullanici_ArtikGirisYapamaz()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, "Üretim");
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Ayrılan Mert", departmanId);
        await TestKimlik.KullaniciOlusturAsync(factory, "mert@izintakip.test", SistemRolleri.Calisan, calisanId: calisanId);
        var calisan = await TestKimlik.OturumluClientAsync(factory, "mert@izintakip.test");

        var pasiflestir = await admin.PutAsJsonAsync($"/api/calisanlar/{calisanId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);
        var sonraki = await calisan.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, pasiflestir.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, sonraki.StatusCode);
    }

    [Fact]
    public async Task HakEkle_ElleSatirlar_ToplamaYansir()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, "Finans");
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Bakiye Eda", departmanId);

        await admin.PostAsJsonAsync($"/api/calisanlar/{calisanId}/haklar", new { dakika = 14 * 480, aciklama = "Açılış bakiyesi" }, TestContext.Current.CancellationToken);
        var yanit = await admin.PostAsJsonAsync($"/api/calisanlar/{calisanId}/haklar", new { dakika = -240, aciklama = "Düzeltme" }, TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal(2, govde.GetProperty("satirlar").GetArrayLength());
        Assert.Equal(13 * 480 + 240, govde.GetProperty("toplamDakika").GetInt32());
        Assert.Equal("13 gün 4 saat", govde.GetProperty("toplamSureMetni").GetString());
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
