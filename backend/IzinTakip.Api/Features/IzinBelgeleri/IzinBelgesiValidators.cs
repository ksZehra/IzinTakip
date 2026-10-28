using FluentValidation;

namespace IzinTakip.Api.Features.IzinBelgeleri;

public sealed class IzinBelgesiYukleRequestValidator : AbstractValidator<IzinBelgesiYukleRequest>
{
    public const long EnFazlaBoyut = 5 * 1024 * 1024;

    public static readonly IReadOnlyDictionary<string, string> IcerikTurleri = new Dictionary<string, string>
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    private static readonly IReadOnlyDictionary<string, byte[]> Imzalar = new Dictionary<string, byte[]>
    {
        ["application/pdf"] = "%PDF"u8.ToArray(),
        ["image/jpeg"] = [0xFF, 0xD8, 0xFF],
        ["image/png"] = [0x89, 0x50, 0x4E, 0x47]
    };

    public IzinBelgesiYukleRequestValidator() =>
        RuleFor(r => r.Dosya)
            .NotNull().WithMessage("Yüklenecek dosyayı seçin.")
            .Must(d => d.Length is > 0 and <= EnFazlaBoyut).WithMessage("Dosya boş olamaz ve en çok 5 MB olabilir.")
            .Must(BicimUygun).WithMessage("Yalnız PDF, JPG ya da PNG dosyası yüklenebilir.");

    private static bool BicimUygun(IFormFile dosya)
    {
        if (!IcerikTurleri.TryGetValue(Path.GetExtension(dosya.FileName).ToLowerInvariant(), out var icerikTuru)
            || !string.Equals(dosya.ContentType, icerikTuru, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var imza = Imzalar[icerikTuru];
        Span<byte> bas = stackalloc byte[imza.Length];
        using var akis = dosya.OpenReadStream();
        return akis.ReadAtLeast(bas, imza.Length, throwOnEndOfStream: false) == imza.Length && bas.SequenceEqual(imza);
    }
}
