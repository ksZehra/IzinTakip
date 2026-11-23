using FluentValidation;

namespace IzinTakip.Api.Features.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Eposta)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");
        RuleFor(r => r.Parola).NotEmpty().WithMessage("Parola zorunludur.");
    }
}

public sealed class ParolaDegistirRequestValidator : AbstractValidator<ParolaDegistirRequest>
{
    public ParolaDegistirRequestValidator()
    {
        RuleFor(r => r.MevcutParola).NotEmpty().WithMessage("Mevcut parola zorunludur.");
        RuleFor(r => r.YeniParola)
            .NotEmpty().WithMessage("Yeni parola zorunludur.")
            .NotEqual(r => r.MevcutParola).WithMessage("Yeni parola mevcut paroladan farklı olmalı.");
    }
}
