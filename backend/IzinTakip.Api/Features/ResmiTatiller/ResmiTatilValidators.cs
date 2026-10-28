using FluentValidation;

namespace IzinTakip.Api.Features.ResmiTatiller;

public sealed class ResmiTatilKaydetRequestValidator : AbstractValidator<ResmiTatilKaydetRequest>
{
    public ResmiTatilKaydetRequestValidator()
    {
        RuleFor(r => r.Ad)
            .NotEmpty().WithMessage("Tatil adı zorunludur.")
            .MaximumLength(100).WithMessage("Tatil adı en çok 100 karakter olabilir.");
        RuleFor(r => r.Tarih)
            .InclusiveBetween(new DateOnly(2000, 1, 1), new DateOnly(2100, 12, 31)).WithMessage("Geçerli bir tarih girin.");
    }
}
