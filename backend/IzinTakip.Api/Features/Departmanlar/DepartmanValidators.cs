using FluentValidation;

namespace IzinTakip.Api.Features.Departmanlar;

public sealed class DepartmanKaydetRequestValidator : AbstractValidator<DepartmanKaydetRequest>
{
    public DepartmanKaydetRequestValidator()
    {
        RuleFor(r => r.Ad)
            .NotEmpty().WithMessage("Departman adı zorunludur.")
            .MaximumLength(100).WithMessage("Departman adı en çok 100 karakter olabilir.");
        RuleFor(r => r.YoneticiId).GreaterThan(0).When(r => r.YoneticiId is not null).WithMessage("Geçerli bir yönetici seçin.");
    }
}
