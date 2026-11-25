using FluentValidation;
using IzinTakip.Api.Common.Validation;

namespace IzinTakip.Api.Features.Calisanlar;

public sealed class CalisanListeRequestValidator : AbstractValidator<CalisanListeRequest>
{
    public CalisanListeRequestValidator()
    {
        RuleFor(r => r.Arama).MaximumLength(200).WithMessage("Arama metni en çok 200 karakter olabilir.");
        RuleFor(r => r.Sort).GecerliSiralama(CalisanService.SiralamaAlanlari.Keys);
        RuleFor(r => r.IseGirisBaslangic).GecerliTarih();
        RuleFor(r => r.IseGirisBitis).GecerliTarih();
        RuleFor(r => r.IseGirisBitis).BaslangictanOnceDegil(r => r.IseGirisBaslangic);
        RuleFor(r => r.DepartmanId).SinirliSecim();
    }
}

public sealed class CalisanKaydetRequestValidator : AbstractValidator<CalisanKaydetRequest>
{
    public CalisanKaydetRequestValidator()
    {
        RuleFor(r => r.Ad)
            .NotEmpty().WithMessage("Ad zorunludur.")
            .MaximumLength(100).WithMessage("Ad en çok 100 karakter olabilir.");
        RuleFor(r => r.Soyad)
            .NotEmpty().WithMessage("Soyad zorunludur.")
            .MaximumLength(100).WithMessage("Soyad en çok 100 karakter olabilir.");
        RuleFor(r => r.DepartmanId).GreaterThan(0).WithMessage("Departman seçin.");
        RuleFor(r => r.DogumTarihi)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Doğum tarihi zorunludur.")
            .GreaterThan(new DateOnly(1900, 1, 1)).WithMessage("Geçerli bir doğum tarihi girin.");
        RuleFor(r => r.DogumTarihi)
            .LessThan(r => r.IseGirisTarihi)
            .When(r => r.DogumTarihi is not null && r.IseGirisTarihi is not null)
            .WithMessage("Doğum tarihi işe giriş tarihinden önce olmalı.");
        RuleFor(r => r.IseGirisTarihi)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("İşe giriş tarihi zorunludur.")
            .GreaterThan(new DateOnly(1950, 1, 1)).WithMessage("Geçerli bir işe giriş tarihi girin.");
        RuleFor(r => r.HakBaslangicKidemYili)
            .InclusiveBetween(1, 60).When(r => r.HakBaslangicKidemYili is not null)
            .WithMessage("Otomatik hak başlangıç yılı 1 ile 60 arasında olmalı.");
    }
}

public sealed class IzinHakkiKaydetRequestValidator : AbstractValidator<IzinHakkiKaydetRequest>
{
    private const int BirYilinDakikasi = 366 * 24 * 60;

    public IzinHakkiKaydetRequestValidator()
    {
        RuleFor(r => r.Dakika)
            .NotEqual(0).WithMessage("Süre sıfır olamaz.")
            .InclusiveBetween(-BirYilinDakikasi, BirYilinDakikasi).WithMessage("Bir hak satırı en çok bir yıllık süre olabilir.");
        RuleFor(r => r.KazanimTarihi).GecerliTarih();
        RuleFor(r => r.Aciklama)
            .NotEmpty().WithMessage("Açıklama zorunludur.")
            .MaximumLength(250).WithMessage("Açıklama en çok 250 karakter olabilir.");
    }
}
