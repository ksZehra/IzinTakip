using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Email;

namespace IzinTakip.Api.Common.Notifications;

public sealed record NotificationRecipient(int KullaniciId, string Eposta);

public sealed class NotificationPublisher(IzinTakipDbContext db, IEmailQueue emailQueue, AppOptions uygulama)
{
    private readonly List<EmailMessage> _epostalar = [];

    public void Ekle(IEnumerable<NotificationRecipient> alicilar, string baslik, string metin, string baglanti, string baglantiMetni)
    {
        var html = EmailTemplate.Bildirim(baslik, metin, uygulama.Adres.TrimEnd('/') + baglanti, baglantiMetni);
        foreach (var alici in alicilar)
        {
            db.Bildirimler.Add(new Bildirim(alici.KullaniciId, baslik, metin, baglanti));
            _epostalar.Add(new EmailMessage(alici.Eposta, baslik, html));
        }
    }

    public void EpostalariKuyrugaAl()
    {
        foreach (var eposta in _epostalar)
        {
            emailQueue.Enqueue(eposta);
        }

        _epostalar.Clear();
    }
}
