using System.Linq.Expressions;
using IzinTakip.Domain.Entities;

namespace IzinTakip.Api.Common.Paging;

public sealed record SiralamaIstegi(string Alan, bool Azalan)
{
    public static SiralamaIstegi? Coz(string? sort)
    {
        var parcalar = sort?.Split(':');
        if (parcalar is not [var alan, var yon] || alan.Length == 0)
        {
            return null;
        }

        return yon.ToLowerInvariant() switch
        {
            "asc" => new SiralamaIstegi(alan, false),
            "desc" => new SiralamaIstegi(alan, true),
            _ => null
        };
    }
}

public static class SiralamaExtensions
{
    public static Func<IQueryable<T>, bool, IOrderedQueryable<T>> SiralamaAlani<T, TAnahtar>(Expression<Func<T, TAnahtar>> anahtar) =>
        (sorgu, azalan) => azalan ? sorgu.OrderByDescending(anahtar) : sorgu.OrderBy(anahtar);

    public static IOrderedQueryable<T> Sirala<T>(
        this IQueryable<T> sorgu,
        string? sort,
        IReadOnlyDictionary<string, Func<IQueryable<T>, bool, IOrderedQueryable<T>>> alanlar,
        Func<IQueryable<T>, IOrderedQueryable<T>> varsayilan)
        where T : Entity =>
        sorgu.Sirala(sort, alanlar, varsayilan, e => e.Id);

    public static IOrderedQueryable<T> Sirala<T>(
        this IQueryable<T> sorgu,
        string? sort,
        IReadOnlyDictionary<string, Func<IQueryable<T>, bool, IOrderedQueryable<T>>> alanlar,
        Func<IQueryable<T>, IOrderedQueryable<T>> varsayilan,
        Expression<Func<T, int>> id)
    {
        if (SiralamaIstegi.Coz(sort) is not { } istek || !alanlar.TryGetValue(istek.Alan, out var sirala))
        {
            return varsayilan(sorgu);
        }

        var sirali = sirala(sorgu, istek.Azalan);
        return istek.Azalan ? sirali.ThenByDescending(id) : sirali.ThenBy(id);
    }
}
