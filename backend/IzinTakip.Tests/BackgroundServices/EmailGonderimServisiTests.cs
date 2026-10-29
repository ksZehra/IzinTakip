using IzinTakip.Api.BackgroundServices;
using IzinTakip.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;

namespace IzinTakip.Tests.BackgroundServices;

public sealed class EmailGonderimServisiTests
{
    [Fact]
    public async Task KaliciHata_TekrarDenenmez()
    {
        var gonderici = new SiraliGonderici(new KaliciEmailHatasiException("Alici adresi gecersiz."));

        await GonderAsync(gonderici);

        Assert.Equal(1, gonderici.Deneme);
        Assert.False(gonderici.Gonderildi);
    }

    [Fact]
    public async Task GeciciHata_TekrarDenenipGonderilir()
    {
        var gonderici = new SiraliGonderici(new IOException("Baglanti koptu."), new IOException("Baglanti koptu."));

        await GonderAsync(gonderici);

        Assert.Equal(3, gonderici.Deneme);
        Assert.True(gonderici.Gonderildi);
    }

    [Fact]
    public async Task GeciciHataSurerse_DortDenemedenSonraVazgecilir()
    {
        var gonderici = new SiraliGonderici(Enumerable.Range(0, 10).Select(Exception (_) => new IOException("Sunucu yanit vermiyor.")).ToArray());

        await GonderAsync(gonderici);

        Assert.Equal(4, gonderici.Deneme);
        Assert.False(gonderici.Gonderildi);
    }

    private static async Task GonderAsync(SiraliGonderici gonderici)
    {
        var kuyruk = new ChannelEmailQueue();
        var servis = new EmailGonderimServisi(kuyruk, gonderici, new BeklemesizZaman(), NullLogger<EmailGonderimServisi>.Instance);
        await servis.StartAsync(TestContext.Current.CancellationToken);

        kuyruk.Enqueue(new EmailMesaji("alici@izintakip.test", "Konu", "<p>Govde</p>"));
        kuyruk.Enqueue(new EmailMesaji("bitti@izintakip.test", "Son", "<p>Govde</p>"));
        await gonderici.SonMesaj.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await servis.StopAsync(TestContext.Current.CancellationToken);
    }

    private sealed class SiraliGonderici(params Exception[] hatalar) : IEmailSender
    {
        private readonly Queue<Exception> _hatalar = new(hatalar);

        public int Deneme { get; private set; }
        public bool Gonderildi { get; private set; }
        public TaskCompletionSource SonMesaj { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task SendAsync(EmailMesaji mesaj, CancellationToken cancellationToken)
        {
            if (mesaj.Konu == "Son")
            {
                SonMesaj.TrySetResult();
                return Task.CompletedTask;
            }

            Deneme++;
            if (_hatalar.TryDequeue(out var hata))
            {
                throw hata;
            }

            Gonderildi = true;
            return Task.CompletedTask;
        }
    }

    private sealed class BeklemesizZaman : TimeProvider
    {
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
            System.CreateTimer(callback, state, TimeSpan.Zero, period);
    }
}
