using FluentValidation;
using IzinTakip.Domain;

namespace IzinTakip.Api.Features.Roller;

public sealed class RolKaydetRequestValidator : AbstractValidator<RolKaydetRequest>
{
    public RolKaydetRequestValidator()
    {
        RuleFor(r => r.Ad)
            .NotEmpty().WithMessage("Rol adı zorunludur.")
            .MaximumLength(50).WithMessage("Rol adı en çok 50 karakter olabilir.");
        RuleFor(r => r.Yetkiler).NotNull().WithMessage("Yetki listesi gönderilmeli.");
        RuleForEach(r => r.Yetkiler)
            .Must(kod => Yetkiler.Atanabilir.Any(y => y.Kod == kod))
            .WithMessage((_, kod) => $"'{kod}' atanabilir bir yetki değil.");
    }
}
