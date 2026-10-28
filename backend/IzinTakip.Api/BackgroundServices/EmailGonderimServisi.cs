using IzinTakip.Infrastructure.Email;

namespace IzinTakip.Api.BackgroundServices;

public sealed class EmailGonderimServisi(
    IEmailQueue kuyruk,
    IEmailSender gonderici,
    TimeProvider timeProvider,
    ILogger<EmailGonderimServisi> logger) : BackgroundService
{
    private static readonly TimeSpan[] TekrarBeklemeleri = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60)];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var mesaj in kuyruk.ReadAllAsync(stoppingToken))
        {
            for (var deneme = 0; ; deneme++)
            {
                try
                {
                    await gonderici.SendAsync(mesaj, stoppingToken);
                    logger.LogInformation("E-posta gonderildi {Konu}", mesaj.Konu);
                    break;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    if (exception is KaliciEmailHatasiException || deneme == TekrarBeklemeleri.Length)
                    {
                        logger.LogError(exception, "E-posta gonderilemedi, tekrar denenmeyecek {Konu} {Deneme}", mesaj.Konu, deneme + 1);
                        break;
                    }

                    logger.LogWarning("E-posta gonderilemedi, tekrar denenecek {Deneme} {Konu}: {Hata}", deneme + 1, mesaj.Konu, exception.Message);
                    await Task.Delay(TekrarBeklemeleri[deneme], timeProvider, stoppingToken);
                }
            }
        }
    }
}
