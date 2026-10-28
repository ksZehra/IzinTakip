using FluentValidation;
using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Features.Ayarlar;

public sealed class AyarlarGuncelleRequestValidator : AbstractValidator<AyarlarGuncelleRequest>
{
    private static readonly int[] IzinliAdimlar = [5, 10, 15, 20, 30, 60];

    public AyarlarGuncelleRequestValidator()
    {
        RuleFor(r => r.CalismaGunleri).NotEmpty().WithMessage("En az bir çalışma günü seçin.");
        RuleForEach(r => r.CalismaGunleri)
            .Must(g => g != HaftaGunleri.Yok && Enum.IsDefined(g)).WithMessage("Çalışma günü geçersiz.");
        RuleFor(r => r.SaatlikIzinAdimDakika)
            .Must(a => IzinliAdimlar.Contains(a)).WithMessage("Saatlik izin adımı 5, 10, 15, 20, 30 ya da 60 dakika olabilir.");
        RuleFor(r => r.HatirlatmaGunSayisi).InclusiveBetween(1, 30).WithMessage("Hatırlatma gün sayısı 1 ile 30 arasında olmalı.");
    }
}
