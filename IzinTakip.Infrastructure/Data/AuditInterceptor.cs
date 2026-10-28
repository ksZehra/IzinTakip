using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IzinTakip.Infrastructure.Data;

public sealed class AuditInterceptor(TimeProvider timeProvider, IGecerliKullanici gecerliKullanici) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DenetimAlanlariniDoldur(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DenetimAlanlariniDoldur(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void DenetimAlanlariniDoldur(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var simdi = timeProvider.GetUtcNow().UtcDateTime;
        var kullaniciId = gecerliKullanici.KullaniciId;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.OlusturmaTarihi).CurrentValue = simdi;
                entry.Property(e => e.OlusturanKullaniciId).CurrentValue = kullaniciId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.GuncellemeTarihi).CurrentValue = simdi;
                entry.Property(e => e.GuncelleyenKullaniciId).CurrentValue = kullaniciId;
            }
        }
    }
}
