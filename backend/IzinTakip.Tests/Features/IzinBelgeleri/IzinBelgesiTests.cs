using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.IzinBelgeleri;

public sealed class IzinBelgesiTests(VeritabanliApiFactory factory) : IClassFixture<VeritabanliApiFactory>
{
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj<<>>endobj\ntrailer<<>>\n%%EOF");
    private static readonly DateOnly Pazartesi = TestVeri.SonrakiYilPazartesi();

    [Fact]
    public async Task Yukle_PdfDosya_201ListedeGorunurVeAyniIcerikIndirilir()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan, "Yıllık");

        var yanit = await YukleAsync(ekip.Calisan, talepId, Pdf, "doktor raporu ü.pdf", "application/pdf");
        var belge = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var liste = await ekip.Yonetici.GetFromJsonAsync<JsonElement>($"/api/izin-talepleri/{talepId}/belgeler", TestContext.Current.CancellationToken);
        var indirme = await ekip.Yonetici.GetAsync(yanit.Headers.Location, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, yanit.StatusCode);
        Assert.Equal("doktor raporu ü.pdf", belge.GetProperty("dosyaAdi").GetString());
        Assert.Equal(Pdf.Length, belge.GetProperty("boyut").GetInt64());
        Assert.Equal(1, liste.GetArrayLength());
        Assert.Equal("application/pdf", indirme.Content.Headers.ContentType?.MediaType);
        Assert.Equal(Pdf, await indirme.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("sahte.pdf", "application/pdf", "MZ-bu-bir-program")]
    [InlineData("belge.exe", "application/octet-stream", "%PDF-1.4")]
    [InlineData("belge.png", "image/jpeg", "%PDF-1.4")]
    public async Task Yukle_BicimUygunDegil_400AlanHatasiDoner(string ad, string icerikTuru, string icerik)
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan, "Yıllık");

        var yanit = await YukleAsync(ekip.Calisan, talepId, Encoding.ASCII.GetBytes(icerik), ad, icerikTuru);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Yalnız PDF, JPG ya da PNG dosyası yüklenebilir.", govde.GetProperty("errors").GetProperty("dosya")[0].GetString());
    }

    [Fact]
    public async Task Yukle_BesMegabaytUstu_400Doner()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan, "Yıllık");
        var buyuk = new byte[5 * 1024 * 1024 + 1];
        Pdf.CopyTo(buyuk, 0);

        var yanit = await YukleAsync(ekip.Calisan, talepId, buyuk, "buyuk.pdf", "application/pdf");

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
    }

    [Fact]
    public async Task Onayla_BelgeZorunluTurBelgeEklenince_Onaylanir()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan, "Rapor");

        var belgesiz = await OnaylaAsync(ekip.Yonetici, talepId);
        await YukleAsync(ekip.Calisan, talepId, Pdf, "rapor.pdf", "application/pdf");
        var belgeli = await OnaylaAsync(ekip.Yonetici, talepId);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, belgesiz.StatusCode);
        Assert.Equal(
            "Bu izin türü belge ister; talebe belge eklenmeden onaylanamaz.",
            (await belgesiz.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString());
        Assert.Equal(HttpStatusCode.OK, belgeli.StatusCode);
    }

    [Fact]
    public async Task Sil_BeklemedeSilinirOnaylandiktanSonraSilinmez()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan, "Yıllık");
        var birinci = (await YukleAsync(ekip.Calisan, talepId, Pdf, "bir.pdf", "application/pdf")).Headers.Location;
        var ikinci = (await YukleAsync(ekip.Calisan, talepId, Pdf, "iki.pdf", "application/pdf")).Headers.Location;

        var beklemede = await ekip.Calisan.DeleteAsync(birinci, TestContext.Current.CancellationToken);
        var silinmisiIndir = await ekip.Calisan.GetAsync(birinci, TestContext.Current.CancellationToken);
        await OnaylaAsync(ekip.Yonetici, talepId);
        var onaylanmis = await ekip.Calisan.DeleteAsync(ikinci, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, beklemede.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, silinmisiIndir.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, onaylanmis.StatusCode);
    }

    [Fact]
    public async Task Yukle_BaskasininTalebine_404Doner()
    {
        var ekip = await EkipKurAsync();
        var diger = await EkipKurAsync();
        var talepId = await TalepAcAsync(diger.Calisan, "Yıllık");

        var yanit = await YukleAsync(ekip.Calisan, talepId, Pdf, "rapor.pdf", "application/pdf");

        Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
    }

    private async Task<Ekip> EkipKurAsync()
    {
        var admin = await TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Ekip-{Guid.NewGuid():N}");
        var yoneticiId = await TestVeri.CalisanEkleAsync(admin, "Yönetici", departmanId);
        await TestVeri.DepartmanYoneticisiAtaAsync(admin, departmanId, $"Ekip-{yoneticiId}", yoneticiId);
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Çalışan", departmanId);
        await TestVeri.HakEkleAsync(admin, calisanId, 14 * 480);
        var yoneticiEposta = $"yonetici-{yoneticiId}@izintakip.test";
        var calisanEposta = $"calisan-{calisanId}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, yoneticiEposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        await TestKimlik.KullaniciOlusturAsync(factory, calisanEposta, SistemRolleri.Calisan, calisanId: calisanId);
        return new Ekip(await TestKimlik.OturumluClientAsync(factory, calisanEposta), await TestKimlik.OturumluClientAsync(factory, yoneticiEposta));
    }

    private static async Task<int> TalepAcAsync(HttpClient client, string tur)
    {
        var turId = await TestVeri.IzinTuruIdAsync(client, tur);
        var yanit = await client.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { izinTuruId = turId, sureTipi = "TamGun", tarih = Pazartesi, bitisTarihi = Pazartesi.AddDays(1) },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(yanit.StatusCode == HttpStatusCode.Created, govde);
        return JsonDocument.Parse(govde).RootElement.GetProperty("id").GetInt32();
    }

    private static Task<HttpResponseMessage> YukleAsync(HttpClient client, int talepId, byte[] icerik, string ad, string icerikTuru)
    {
        var dosya = new ByteArrayContent(icerik);
        dosya.Headers.ContentType = new MediaTypeHeaderValue(icerikTuru);
        var form = new MultipartFormDataContent { { dosya, "dosya", ad } };
        return client.PostAsync($"/api/izin-talepleri/{talepId}/belgeler", form, TestContext.Current.CancellationToken);
    }

    private static Task<HttpResponseMessage> OnaylaAsync(HttpClient client, int talepId) =>
        client.PostAsJsonAsync($"/api/izin-talepleri/{talepId}/onayla", new { }, TestContext.Current.CancellationToken);

    private sealed record Ekip(HttpClient Calisan, HttpClient Yonetici);
}
