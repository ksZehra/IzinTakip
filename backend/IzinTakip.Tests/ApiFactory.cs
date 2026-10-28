using System.Globalization;
using FluentValidation;
using IzinTakip.Infrastructure.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IzinTakip.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string SuperAdminEposta = "superadmin@izintakip.test";
    public const string SuperAdminParola = "SuperAdmin2026";

    public TestEmailSender EmailSender { get; } = new();

    protected virtual string ConnectionString =>
        "Server=127.0.0.1,1;Database=IzinTakipTest;User Id=test;Password=test;TrustServerCertificate=True";

    protected virtual bool VeritabaniHazirlanir => false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var veritabaniHazirlanir = VeritabaniHazirlanir.ToString(CultureInfo.InvariantCulture);

        builder.UseEnvironment("Test");
        builder.UseSetting("ConnectionStrings:IzinTakip", ConnectionString);
        builder.UseSetting("Database:MigrateOnStartup", veritabaniHazirlanir);
        builder.UseSetting("Database:SeedOnStartup", veritabaniHazirlanir);
        builder.UseSetting("Jwt:SigningKey", "test-ortami-icin-en-az-otuz-iki-karakterlik-anahtar");
        builder.UseSetting("Seed:SuperAdmin:Eposta", SuperAdminEposta);
        builder.UseSetting("Seed:SuperAdmin:Parola", SuperAdminParola);
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
        builder.UseSetting("LogKlasoru", Path.Combine(Path.GetTempPath(), "IzinTakip.Tests", "logs"));
        builder.UseSetting("DosyaDeposu:Klasor", Path.Combine(Path.GetTempPath(), "IzinTakip.Tests", "belgeler"));

        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);
            services.AddValidatorsFromAssemblyContaining<ApiFactory>();
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }
}
