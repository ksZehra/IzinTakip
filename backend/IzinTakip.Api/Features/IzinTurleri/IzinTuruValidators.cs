using FluentValidation;

namespace IzinTakip.Api.Features.IzinTurleri;

public sealed class IzinTuruKaydetRequestValidator : AbstractValidator<IzinTuruKaydetRequest>
{
    public IzinTuruKaydetRequestValidator()
    {
        RuleFor(r => r.Ad)
            .NotEmpty().WithMessage("İzin türü adı zorunludur.")
            .MaximumLength(100).WithMessage("İzin türü adı en çok 100 karakter olabilir.");
        RuleFor(r => r.GunHesabi).IsInEnum().WithMessage("Gün hesabı geçersiz.");
        RuleFor(r => r.TalepBasinaEnFazlaDakika).GreaterThan(0).When(r => r.TalepBasinaEnFazlaDakika is not null)
            .WithMessage("Talep başına sınır sıfırdan büyük olmalı.");
        RuleFor(r => r.YillikEnFazlaDakika).GreaterThan(0).When(r => r.YillikEnFazlaDakika is not null)
            .WithMessage("Yıllık sınır sıfırdan büyük olmalı.");
    }
}
