using Microsoft.AspNetCore.Identity;

namespace IzinTakip.Infrastructure.Identity;

public sealed class Rol : IdentityRole<int>
{
    public Rol()
    {
    }

    public Rol(string ad) : base(ad)
    {
    }

    public bool Korumali { get; set; }
}
