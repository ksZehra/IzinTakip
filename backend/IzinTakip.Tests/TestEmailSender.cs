using System.Collections.Concurrent;
using IzinTakip.Infrastructure.Email;

namespace IzinTakip.Tests;

public sealed class TestEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMesaji> _gonderilenler = new();

    public Task SendAsync(EmailMesaji mesaj, CancellationToken cancellationToken)
    {
        _gonderilenler.Enqueue(mesaj);
        return Task.CompletedTask;
    }

    public async Task<EmailMesaji> BekleAsync(string alici, string konu)
    {
        for (var deneme = 0; deneme < 50; deneme++)
        {
            if (_gonderilenler.FirstOrDefault(m => m.Alici == alici && m.Konu == konu) is { } mesaj)
            {
                return mesaj;
            }

            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException($"{alici} adresine '{konu}' konulu e-posta gelmedi.");
    }
}
