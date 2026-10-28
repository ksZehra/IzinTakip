using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.UseSetting(
            "ConnectionStrings:IzinTakip",
            "Server=127.0.0.1,1;Database=IzinTakipTest;User Id=test;Password=test;TrustServerCertificate=True");

        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);
            services.AddValidatorsFromAssemblyContaining<ApiFactory>();
        });
    }
}
