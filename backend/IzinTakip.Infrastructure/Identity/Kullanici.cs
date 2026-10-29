using Microsoft.AspNetCore.Identity;

namespace IzinTakip.Infrastructure.Identity;

public sealed class Kullanici : IdentityUser<int>
{
    public int? CalisanId { get; set; }
    public bool Aktif { get; set; } = true;
    public bool ParolaDegistirmeliMi { get; set; }
    public DateTime? GeciciParolaBitis { get; set; }
}
