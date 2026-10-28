using FluentValidation;

namespace IzinTakip.Api.Features.HakKurallari;

public sealed class HakKuraliKaydetRequestValidator : AbstractValidator<HakKuraliKaydetRequest>
{
    public HakKuraliKaydetRequestValidator()
    {
        RuleFor(r => r.Ad)
            .NotEmpty().WithMessage("Kural adı zorunludur.")
            .MaximumLength(100).WithMessage("Kural adı en çok 100 karakter olabilir.");
        RuleFor(r => r.MinKidemYil).InclusiveBetween(0, 60).WithMessage("En az kıdem 0 ile 60 yıl arasında olmalı.");
        RuleFor(r => r.MaxKidemYil).InclusiveBetween(0, 60).When(r => r.MaxKidemYil is not null)
            .WithMessage("En çok kıdem 0 ile 60 yıl arasında olmalı.");
        RuleFor(r => r.MinYas).InclusiveBetween(14, 100).When(r => r.MinYas is not null).WithMessage("En az yaş 14 ile 100 arasında olmalı.");
        RuleFor(r => r.MaxYas).InclusiveBetween(14, 100).When(r => r.MaxYas is not null).WithMessage("En çok yaş 14 ile 100 arasında olmalı.");
        RuleFor(r => r.Gun).InclusiveBetween(1, 365).WithMessage("Gün sayısı 1 ile 365 arasında olmalı.");
    }
}
