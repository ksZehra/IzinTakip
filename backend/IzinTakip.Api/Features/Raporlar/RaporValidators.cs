using FluentValidation;
using IzinTakip.Api.Common.Validation;

namespace IzinTakip.Api.Features.Raporlar;

public sealed class TakvimRequestValidator : AbstractValidator<TakvimRequest>
{
    public TakvimRequestValidator()
    {
        RuleFor(r => r.Baslangic).GecerliTarih();
        RuleFor(r => r.Bitis)
            .GreaterThanOrEqualTo(r => r.Baslangic).WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.")
            .LessThanOrEqualTo(r => r.Baslangic.AddMonths(3)).WithMessage("Takvim en çok 3 aylık aralık için gösterilebilir.")
            .When(r => r.Baslangic <= DateRuleExtensions.EnGec);
    }
}

public sealed class BakiyeRaporuFiltreRequestValidator : AbstractValidator<BakiyeRaporuFiltreRequest>
{
    public BakiyeRaporuFiltreRequestValidator()
    {
        RuleFor(r => r.Arama).MaximumLength(200).WithMessage("Arama metni en çok 200 karakter olabilir.");
        RuleFor(r => r.Sort).GecerliSiralama(RaporService.BakiyeSiralamaAlanlari.Keys);
        RuleFor(r => r.IseGirisBaslangic).GecerliTarih();
        RuleFor(r => r.IseGirisBitis).GecerliTarih();
        RuleFor(r => r.IseGirisBitis).BaslangictanOnceDegil(r => r.IseGirisBaslangic);
        RuleFor(r => r.DepartmanId).SinirliSecim();
    }
}

public sealed class BakiyeRaporuRequestValidator : AbstractValidator<BakiyeRaporuRequest>
{
    public BakiyeRaporuRequestValidator() => Include(new BakiyeRaporuFiltreRequestValidator());
}
