using Microsoft.AspNetCore.Identity;

namespace IzinTakip.Infrastructure.Identity;

public sealed class TurkceIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Hata(nameof(DefaultError), "Beklenmeyen bir hata oluştu.");
    public override IdentityError ConcurrencyFailure() => Hata(nameof(ConcurrencyFailure), "Kayıt başka bir kullanıcı tarafından değiştirildi, sayfayı yenileyin.");
    public override IdentityError PasswordMismatch() => Hata(nameof(PasswordMismatch), "Mevcut parola hatalı.");
    public override IdentityError PasswordTooShort(int length) => Hata(nameof(PasswordTooShort), $"Parola en az {length} karakter olmalı.");
    public override IdentityError PasswordRequiresDigit() => Hata(nameof(PasswordRequiresDigit), "Parola en az bir rakam içermeli.");
    public override IdentityError PasswordRequiresLower() => Hata(nameof(PasswordRequiresLower), "Parola en az bir küçük harf içermeli.");
    public override IdentityError PasswordRequiresUpper() => Hata(nameof(PasswordRequiresUpper), "Parola en az bir büyük harf içermeli.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Hata(nameof(PasswordRequiresNonAlphanumeric), "Parola en az bir sembol içermeli.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Hata(nameof(PasswordRequiresUniqueChars), $"Parola en az {uniqueChars} farklı karakter içermeli.");
    public override IdentityError DuplicateEmail(string email) => Hata(nameof(DuplicateEmail), "Bu e-posta adresi başka bir kullanıcıda kayıtlı.");
    public override IdentityError DuplicateUserName(string userName) => Hata(nameof(DuplicateUserName), "Bu e-posta adresi başka bir kullanıcıda kayıtlı.");
    public override IdentityError InvalidEmail(string? email) => Hata(nameof(InvalidEmail), "Geçerli bir e-posta adresi girin.");
    public override IdentityError InvalidUserName(string? userName) => Hata(nameof(InvalidUserName), "Kullanıcı adı geçersiz.");
    public override IdentityError DuplicateRoleName(string role) => Hata(nameof(DuplicateRoleName), $"'{role}' adında bir rol zaten var.");
    public override IdentityError InvalidRoleName(string? role) => Hata(nameof(InvalidRoleName), "Rol adı geçersiz.");
    public override IdentityError UserAlreadyInRole(string role) => Hata(nameof(UserAlreadyInRole), $"Kullanıcı zaten '{role}' rolünde.");
    public override IdentityError UserNotInRole(string role) => Hata(nameof(UserNotInRole), $"Kullanıcı '{role}' rolünde değil.");

    private static IdentityError Hata(string kod, string aciklama) => new() { Code = kod, Description = aciklama };
}
