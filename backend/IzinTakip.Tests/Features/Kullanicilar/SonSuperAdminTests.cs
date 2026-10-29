using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.Kullanicilar;

public sealed class SonSuperAdminTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task RolleriDegistir_SonAktifSuperAdmin_RoluKaldirilamazIkinciVarkenKaldirilir()
    {
        var admin = await TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
        var adminId = (await admin.GetFromJsonAsync<JsonElement>("/api/auth/ben", TestContext.Current.CancellationToken)).GetProperty("kullaniciId").GetInt32();
        var ikinciEposta = $"superadmin-{Guid.NewGuid():N}@izintakip.test";

        var tekken = await RolAyarlaAsync(admin, adminId, SistemRolleri.IK);
        var ikinciId = await TestAuth.KullaniciOlusturAsync(factory, ikinciEposta, SistemRolleri.SuperAdmin);
        var ikinci = await TestAuth.OturumluClientAsync(factory, ikinciEposta);
        var ikinciVarken = await RolAyarlaAsync(ikinci, adminId, SistemRolleri.IK);
        var geriAl = await RolAyarlaAsync(ikinci, adminId, SistemRolleri.SuperAdmin);
        var ikinciyiKaldir = await RolAyarlaAsync(admin, ikinciId, SistemRolleri.IK);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, tekken.StatusCode);
        Assert.Equal(
            "Son aktif SuperAdmin'in SuperAdmin rolü kaldırılamaz.",
            (await tekken.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString());
        Assert.Equal(HttpStatusCode.OK, ikinciVarken.StatusCode);
        Assert.Equal(HttpStatusCode.OK, geriAl.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ikinciyiKaldir.StatusCode);
    }

    private static Task<HttpResponseMessage> RolAyarlaAsync(HttpClient client, int kullaniciId, string rol) =>
        client.PutAsJsonAsync($"/api/kullanicilar/{kullaniciId}/roller", new { roller = new[] { rol } }, TestContext.Current.CancellationToken);
}
