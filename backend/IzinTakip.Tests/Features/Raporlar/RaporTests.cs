using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClosedXML.Excel;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.Raporlar;

public sealed class RaporTests(VeritabanliApiFactory factory) : IClassFixture<VeritabanliApiFactory>
{
    private const int GunlukDakika = 480;
    private static readonly DateOnly Pazartesi = TestVeri.SonrakiYilPazartesi();

    [Fact]
    public async Task Takvim_Yonetici_EkibininAcikIzinleriniGorurDigerleriniGormez()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Ekip-{Guid.NewGuid():N}");
        var yoneticiId = await TestVeri.CalisanEkleAsync(admin, "Yönetici", departmanId);
        await TestVeri.DepartmanYoneticisiAtaAsync(admin, departmanId, $"Ekip-{yoneticiId}", yoneticiId);
        var izinliId = await CalisanAsync(admin, "İzinli", departmanId);
        var reddedilenId = await CalisanAsync(admin, "Reddedilen", departmanId);
        var digerId = await CalisanAsync(admin, "Diğer", await TestVeri.DepartmanEkleAsync(admin, $"Diger-{Guid.NewGuid():N}"));
        var yoneticiEposta = $"yonetici-{yoneticiId}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, yoneticiEposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        var yonetici = await TestKimlik.OturumluClientAsync(factory, yoneticiEposta);
        await TalepAcAsync(yonetici, izinliId, Pazartesi, Pazartesi.AddDays(1));
        var reddedilecek = await TalepAcAsync(yonetici, reddedilenId, Pazartesi, Pazartesi);
        await TalepAcAsync(admin, digerId, Pazartesi, Pazartesi);
        await yonetici.PostAsJsonAsync($"/api/izin-talepleri/{reddedilecek}/reddet", new { gerekce = "Yoğun dönem" }, TestContext.Current.CancellationToken);

        var takvim = await yonetici.GetFromJsonAsync<JsonElement>(
            $"/api/raporlar/takvim?baslangic={Pazartesi:yyyy-MM-dd}&bitis={Pazartesi.AddDays(6):yyyy-MM-dd}",
            TestContext.Current.CancellationToken);

        var kayit = Assert.Single(takvim.EnumerateArray());
        Assert.Equal(izinliId, kayit.GetProperty("calisanId").GetInt32());
        Assert.Equal("Beklemede", kayit.GetProperty("durum").GetString());
    }

    [Fact]
    public async Task Takvim_AralikUcAydanUzun_400Doner()
    {
        var ik = await IkAsync();

        var yanit = await ik.GetAsync(
            $"/api/raporlar/takvim?baslangic={Pazartesi:yyyy-MM-dd}&bitis={Pazartesi.AddMonths(4):yyyy-MM-dd}",
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Takvim en çok 3 aylık aralık için gösterilebilir.", govde.GetProperty("errors").GetProperty("bitis")[0].GetString());
    }

    [Fact]
    public async Task Bakiyeler_Ik_DepartmandakilerinHakKullanimVeBekleyeniniDoner()
    {
        var (departmanId, _, ik) = await BakiyeVerisiKurAsync();

        var rapor = await ik.GetFromJsonAsync<JsonElement>($"/api/raporlar/bakiyeler?departmanId={departmanId}", TestContext.Current.CancellationToken);
        var ali = rapor.GetProperty("items")[0];

        Assert.Equal(2, rapor.GetProperty("totalCount").GetInt32());
        Assert.Equal("Ali Test", ali.GetProperty("calisanAdSoyad").GetString());
        Assert.Equal(14 * GunlukDakika, ali.GetProperty("hakDakika").GetInt32());
        Assert.Equal(2 * GunlukDakika, ali.GetProperty("kullanilanDakika").GetInt32());
        Assert.Equal(GunlukDakika, ali.GetProperty("bekleyenDakika").GetInt32());
        Assert.Equal("11 gün", ali.GetProperty("kullanilabilirMetni").GetString());
        Assert.Equal("20 gün", rapor.GetProperty("items")[1].GetProperty("kullanilabilirMetni").GetString());
    }

    [Fact]
    public async Task Bakiyeler_Calisan_403Doner()
    {
        var admin = await SuperAdminAsync();
        var calisanId = await CalisanAsync(admin, "Çalışan", await TestVeri.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));
        var eposta = $"calisan-{calisanId}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan, calisanId: calisanId);
        var calisan = await TestKimlik.OturumluClientAsync(factory, eposta);

        var yanit = await calisan.GetAsync("/api/raporlar/bakiyeler", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
    }

    [Fact]
    public async Task BakiyeExcel_Ik_SatirBasinaCalisanDoner()
    {
        var (departmanId, _, ik) = await BakiyeVerisiKurAsync();

        var yanit = await ik.GetAsync($"/api/raporlar/bakiyeler/excel?departmanId={departmanId}", TestContext.Current.CancellationToken);
        using var kitap = new XLWorkbook(await yanit.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken));
        var sayfa = kitap.Worksheet(1);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", yanit.Content.Headers.ContentType?.MediaType);
        Assert.StartsWith("bakiye-raporu-", yanit.Content.Headers.ContentDisposition?.FileName);
        Assert.Equal("Çalışan", sayfa.Cell(1, 1).GetString());
        Assert.Equal("Ali Test", sayfa.Cell(2, 1).GetString());
        Assert.Equal("11 gün", sayfa.Cell(2, 7).GetString());
        Assert.Equal(3, sayfa.LastRowUsed()!.RowNumber());
    }

    [Fact]
    public async Task IzinTalepleriExcel_DurumFiltresi_YalnizEslesenTalepleriYazar()
    {
        var (_, aliId, ik) = await BakiyeVerisiKurAsync();

        var yanit = await ik.GetAsync($"/api/izin-talepleri/excel?calisanId={aliId}&durum=Onaylandi", TestContext.Current.CancellationToken);
        using var kitap = new XLWorkbook(await yanit.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken));
        var sayfa = kitap.Worksheet(1);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal(2, sayfa.LastRowUsed()!.RowNumber());
        Assert.Equal("Tam gün", sayfa.Cell(2, 4).GetString());
        Assert.Equal(Pazartesi.ToDateTime(new TimeOnly(9, 0)), sayfa.Cell(2, 5).GetDateTime());
        Assert.Equal("2 gün", sayfa.Cell(2, 7).GetString());
        Assert.Equal("Onaylandı", sayfa.Cell(2, 8).GetString());
    }

    private async Task<(int DepartmanId, int AliId, HttpClient Ik)> BakiyeVerisiKurAsync()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Rapor-{Guid.NewGuid():N}");
        var aliId = await CalisanAsync(admin, "Ali", departmanId);
        var zeynepId = await TestVeri.CalisanEkleAsync(admin, "Zeynep", departmanId);
        await TestVeri.HakEkleAsync(admin, zeynepId, 20 * GunlukDakika);
        var ik = await IkAsync();
        var onaylanacak = await TalepAcAsync(ik, aliId, Pazartesi, Pazartesi.AddDays(1));
        await TalepAcAsync(ik, aliId, Pazartesi.AddDays(2), Pazartesi.AddDays(2));
        var onay = await ik.PostAsJsonAsync($"/api/izin-talepleri/{onaylanacak}/onayla", new { }, TestContext.Current.CancellationToken);
        onay.EnsureSuccessStatusCode();
        return (departmanId, aliId, ik);
    }

    private static async Task<int> CalisanAsync(HttpClient admin, string ad, int departmanId)
    {
        var calisanId = await TestVeri.CalisanEkleAsync(admin, ad, departmanId);
        await TestVeri.HakEkleAsync(admin, calisanId, 14 * GunlukDakika);
        return calisanId;
    }

    private static async Task<int> TalepAcAsync(HttpClient client, int calisanId, DateOnly baslangic, DateOnly bitis)
    {
        var yillik = await TestVeri.IzinTuruIdAsync(client, "Yıllık");
        var yanit = await client.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { calisanId, izinTuruId = yillik, sureTipi = "TamGun", tarih = baslangic, bitisTarihi = bitis },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(yanit.StatusCode == HttpStatusCode.Created, govde);
        return JsonDocument.Parse(govde).RootElement.GetProperty("id").GetInt32();
    }

    private async Task<HttpClient> IkAsync()
    {
        var eposta = $"ik-{Guid.NewGuid():N}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, eposta, SistemRolleri.IK);
        return await TestKimlik.OturumluClientAsync(factory, eposta);
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
