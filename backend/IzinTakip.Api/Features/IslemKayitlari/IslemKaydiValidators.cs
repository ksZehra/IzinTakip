using FluentValidation;
using IzinTakip.Api.Common.Validation;

namespace IzinTakip.Api.Features.IslemKayitlari;

public sealed class IslemKaydiListeRequestValidator : AbstractValidator<IslemKaydiListeRequest>
{
    public IslemKaydiListeRequestValidator()
    {
        RuleForEach(r => r.Tablo).MaximumLength(50).WithMessage("Tablo adı en çok 50 karakter olabilir.");
        RuleForEach(r => r.Islem).MaximumLength(20).WithMessage("İşlem adı en çok 20 karakter olabilir.");
        RuleFor(r => r.IlgiliArama).MaximumLength(200).WithMessage("Arama metni en çok 200 karakter olabilir.");
        RuleFor(r => r.Sort).GecerliSiralama(IslemKaydiService.SiralamaAlanlari.Keys);
        RuleFor(r => r.Baslangic).GecerliTarih();
        RuleFor(r => r.Bitis).GecerliTarih();
        RuleFor(r => r.Bitis).BaslangictanOnceDegil(r => r.Baslangic);
        RuleFor(r => r.Tablo).SinirliSecim();
        RuleFor(r => r.Islem).SinirliSecim();
    }
}
