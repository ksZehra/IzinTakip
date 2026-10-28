using IzinTakip.Api.BackgroundServices;
using IzinTakip.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;

namespace IzinTakip.Tests.BackgroundServices;

public sealed class EmailGonderimServisiTests
{
    [Fact]
    public async Task KaliciHata_TekrarDenenmez()
    {
        var kuyruk = new ChannelEmailQueue();
        var gonderici = new HataVerenGonderici();
        var servis = new EmailGonderimServisi(kuyruk, gonderici, TimeProvider.System, NullLogger<EmailGonderimServisi>.Instance);
        await servis.StartAsync(TestContext.Current.CancellationToken);

        kuyruk.Enqueue(new EmailMesaji("gecersiz", "Konu", "<p>Govde</p>"));
        await Task.Delay(TimeSpan.FromSeconds(1.5), TestContext.Current.CancellationToken);
        await servis.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, gonderici.Deneme);
    }

    private sealed class HataVerenGonderici : IEmailSender
    {
        public int Deneme { get; private set; }

        public Task SendAsync(EmailMesaji mesaj, CancellationToken cancellationToken)
        {
            Deneme++;
            throw new KaliciEmailHatasiException("Alici adresi gecersiz.");
        }
    }
}
