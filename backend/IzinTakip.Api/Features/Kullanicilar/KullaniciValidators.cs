using FluentValidation;
using IzinTakip.Api.Common.Validation;

namespace IzinTakip.Api.Features.Kullanicilar;

public sealed class KullaniciOlusturRequestValidator : AbstractValidator<KullaniciOlusturRequest>
{
    public KullaniciOlusturRequestValidator()
    {
        RuleFor(r => r.Eposta)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .MaximumLength(256).WithMessage("E-posta en çok 256 karakter olabilir.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");
        RuleFor(r => r.CalisanId).GreaterThan(0).When(r => r.CalisanId is not null).WithMessage("Geçerli bir çalışan seçin.");
        RuleFor(r => r.Roller).NotEmpty().WithMessage("En az bir rol seçin.");
    }
}

public sealed class KullaniciRolleriRequestValidator : AbstractValidator<KullaniciRolleriRequest>
{
    public KullaniciRolleriRequestValidator() =>
        RuleFor(r => r.Roller).NotEmpty().WithMessage("En az bir rol seçin.");
}

public sealed class KullaniciListeRequestValidator : AbstractValidator<KullaniciListeRequest>
{
    public KullaniciListeRequestValidator()
    {
        RuleFor(r => r.Arama).MaximumLength(200).WithMessage("Arama metni en çok 200 karakter olabilir.");
        RuleForEach(r => r.Rol).MaximumLength(256).WithMessage("Rol adı en çok 256 karakter olabilir.");
        RuleFor(r => r.Sort).GecerliSiralama(KullaniciService.SiralamaAlanAdlari);
        RuleFor(r => r.Rol).SinirliSecim();
    }
}
