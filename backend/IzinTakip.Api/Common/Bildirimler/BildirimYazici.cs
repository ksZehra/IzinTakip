using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Email;

namespace IzinTakip.Api.Common.Bildirimler;

public sealed record Alici(int KullaniciId, string Eposta);

public sealed class BildirimYazici(IzinTakipDbContext db, IEmailQueue emailQueue, UygulamaAyarlari uygulama)
{
    private readonly List<EmailMesaji> _epostalar = [];

    public void Ekle(IEnumerable<Alici> alicilar, string baslik, string metin, string baglanti, string baglantiMetni)
    {
        var html = EmailSablonu.Bildirim(baslik, metin, uygulama.Adres.TrimEnd('/') + baglanti, baglantiMetni);
        foreach (var alici in alicilar)
        {
            db.Bildirimler.Add(new Bildirim(alici.KullaniciId, baslik, metin, baglanti));
            _epostalar.Add(new EmailMesaji(alici.Eposta, baslik, html));
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
