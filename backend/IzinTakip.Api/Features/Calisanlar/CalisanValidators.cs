using FluentValidation;

namespace IzinTakip.Api.Features.Calisanlar;

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
            .GreaterThan(new DateOnly(1900, 1, 1)).WithMessage("Geçerli bir doğum tarihi girin.")
            .LessThan(r => r.IseGirisTarihi).WithMessage("Doğum tarihi işe giriş tarihinden önce olmalı.");
        RuleFor(r => r.IseGirisTarihi).GreaterThan(new DateOnly(1950, 1, 1)).WithMessage("Geçerli bir işe giriş tarihi girin.");
        RuleFor(r => r.HakBaslangicKidemYili)
            .InclusiveBetween(1, 60).When(r => r.HakBaslangicKidemYili is not null)
            .WithMessage("Otomatik hak başlangıç yılı 1 ile 60 arasında olmalı.");
    }
}

public sealed class IzinHakkiEkleRequestValidator : AbstractValidator<IzinHakkiEkleRequest>
{
    public IzinHakkiEkleRequestValidator()
    {
        RuleFor(r => r.Dakika).NotEqual(0).WithMessage("Süre sıfır olamaz.");
        RuleFor(r => r.Aciklama)
            .NotEmpty().WithMessage("Açıklama zorunludur.")
            .MaximumLength(250).WithMessage("Açıklama en çok 250 karakter olabilir.");
    }
}
