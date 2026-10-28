using FluentValidation;

namespace IzinTakip.Api.Features.Raporlar;

public sealed class TakvimRequestValidator : AbstractValidator<TakvimRequest>
{
    public TakvimRequestValidator() =>
        RuleFor(r => r.Bitis)
            .GreaterThanOrEqualTo(r => r.Baslangic).WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.")
            .LessThanOrEqualTo(r => r.Baslangic.AddMonths(3)).WithMessage("Takvim en çok 3 aylık aralık için gösterilebilir.");
}
