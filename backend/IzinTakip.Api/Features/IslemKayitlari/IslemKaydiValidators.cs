using FluentValidation;
using IzinTakip.Api.Common.Validation;

namespace IzinTakip.Api.Features.IslemKayitlari;

public sealed class IslemKaydiListeRequestValidator : AbstractValidator<IslemKaydiListeRequest>
{
    public IslemKaydiListeRequestValidator()
    {
        RuleFor(r => r.Tablo).MaximumLength(50).WithMessage("Tablo adı en çok 50 karakter olabilir.");
        RuleFor(r => r.Baslangic).GecerliTarih();
        RuleFor(r => r.Bitis).GecerliTarih();
    }
}
