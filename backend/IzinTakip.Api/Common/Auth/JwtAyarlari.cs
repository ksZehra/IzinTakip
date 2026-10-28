using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IzinTakip.Api.Common.Auth;

public sealed class JwtAyarlari
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int SureSaat { get; init; } = 8;

    public SymmetricSecurityKey ImzaAnahtari() => new(Encoding.UTF8.GetBytes(SigningKey));
}
