using FluentValidation;
using IzinTakip.Api.Common.Validation;

namespace IzinTakip.Api.Features.ResmiTatiller;

public sealed class ResmiTatilKaydetRequestValidator : AbstractValidator<ResmiTatilKaydetRequest>
{
    public ResmiTatilKaydetRequestValidator()
    {
        RuleFor(r => r.Ad)
            .NotEmpty().WithMessage("Tatil adı zorunludur.")
            .MaximumLength(100).WithMessage("Tatil adı en çok 100 karakter olabilir.");
        RuleFor(r => r.Tarih).GecerliTarih();
    }
}
