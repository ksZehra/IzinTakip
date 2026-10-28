using IzinTakip.Api.Common;

namespace IzinTakip.Api.BackgroundServices;

public abstract class GunlukGorev(SirketSaati sirketSaati, TimeProvider timeProvider, ILogger logger) : BackgroundService
{
    protected SirketSaati SirketSaati { get; } = sirketSaati;

    protected abstract TimeOnly Saat { get; }

    public abstract Task CalistirAsync(DateOnly bugun, CancellationToken cancellationToken);

    public static DateTime SonrakiCalisma(DateTime simdi, TimeOnly saat)
    {
        var bugunku = simdi.Date + saat.ToTimeSpan();
        return bugunku > simdi ? bugunku : bugunku.AddDays(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var simdi = SirketSaati.Simdi();
            await Task.Delay(SonrakiCalisma(simdi, Saat) - simdi, timeProvider, stoppingToken);

            try
            {
                await CalistirAsync(SirketSaati.Bugun(), stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Arka plan gorevi hata verdi {Gorev}", GetType().Name);
            }
        }
    }
}
