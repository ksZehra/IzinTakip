using FluentValidation;
using IzinTakip.Api.Common.Validation;
using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Features.IzinTalepleri;

public sealed class SureHesaplaRequestValidator : AbstractValidator<SureHesaplaRequest>
{
    public SureHesaplaRequestValidator()
    {
        RuleFor(r => r.IzinTuruId).GreaterThan(0).WithMessage("İzin türü seçin.");
        RuleFor(r => r.SureTipi).IsInEnum().WithMessage("Süre biçimi seçin: tam gün, yarım gün ya da saatlik.");
        RuleFor(r => r.Tarih).GecerliTarih();

        When(r => r.SureTipi == SureTipi.TamGun && r.BitisTarihi is not null, () =>
        {
            RuleFor(r => r.BitisTarihi!.Value)
                .GreaterThanOrEqualTo(r => r.Tarih).WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.")
                .LessThanOrEqualTo(r => r.Tarih.AddDays(366)).WithMessage("Bir talep en çok bir yılı kapsayabilir.")
                .OverridePropertyName(nameof(SureHesaplaRequest.BitisTarihi));
        });

        RuleFor(r => r.BitisTarihi)
            .Null().When(r => r.SureTipi != SureTipi.TamGun)
            .WithMessage("Bitiş tarihi yalnız tam gün izinde girilir.");

        When(r => r.SureTipi == SureTipi.YarimGun, () =>
            RuleFor(r => r.GunYarisi)
                .NotNull().WithMessage("Sabah ya da öğleden sonra seçin.")
                .IsInEnum().WithMessage("Gün yarısı geçersiz."));

        When(r => r.SureTipi == SureTipi.Saatlik, () =>
        {
            RuleFor(r => r.BaslangicSaati).NotNull().WithMessage("Başlangıç saati zorunludur.");
            RuleFor(r => r.BitisSaati)
                .NotNull().WithMessage("Bitiş saati zorunludur.")
                .GreaterThan(r => r.BaslangicSaati).When(r => r.BaslangicSaati is not null)
                .WithMessage("Bitiş saati başlangıç saatinden sonra olmalı.");
        });

        RuleFor(r => r.BaslangicSaati).TamDakika();
        RuleFor(r => r.BitisSaati).TamDakika();
    }
}

public sealed class IzinTalebiFiltreRequestValidator : AbstractValidator<IzinTalebiFiltreRequest>
{
    public IzinTalebiFiltreRequestValidator()
    {
        RuleFor(r => r.Baslangic).GecerliTarih();
        RuleFor(r => r.Bitis).GecerliTarih();
    }
}

public sealed class IzinTalebiListeRequestValidator : AbstractValidator<IzinTalebiListeRequest>
{
    public IzinTalebiListeRequestValidator() => Include(new IzinTalebiFiltreRequestValidator());
}

public sealed class IzinTalebiKararRequestValidator : AbstractValidator<IzinTalebiKararRequest>
{
    public IzinTalebiKararRequestValidator() =>
        RuleFor(r => r.Not).MaximumLength(500).WithMessage("Not en çok 500 karakter olabilir.");
}

public sealed class IzinTalebiReddetRequestValidator : AbstractValidator<IzinTalebiReddetRequest>
{
    public IzinTalebiReddetRequestValidator() =>
        RuleFor(r => r.Gerekce)
            .NotEmpty().WithMessage("Red gerekçesi zorunludur.")
            .MaximumLength(500).WithMessage("Gerekçe en çok 500 karakter olabilir.");
}

public sealed class IzinTalebiOlusturRequestValidator : AbstractValidator<IzinTalebiOlusturRequest>
{
    public IzinTalebiOlusturRequestValidator()
    {
        Include(new SureHesaplaRequestValidator());
        RuleFor(r => r.CalisanId).GreaterThan(0).When(r => r.CalisanId is not null).WithMessage("Geçerli bir çalışan seçin.");
        RuleFor(r => r.Aciklama).MaximumLength(500).WithMessage("Açıklama en çok 500 karakter olabilir.");
    }
}
