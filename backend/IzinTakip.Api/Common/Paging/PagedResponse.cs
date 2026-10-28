using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Paging;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public static class SayfalamaExtensions
{
    public const int EnFazlaSayfaBoyutu = 100;

    public static async Task<PagedResponse<T>> SayfalaAsync<T>(
        this IQueryable<T> sorgu,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, EnFazlaSayfaBoyutu);

        var toplam = await sorgu.CountAsync(cancellationToken);
        var kayitlar = await sorgu.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResponse<T>(kayitlar, toplam, page, pageSize);
    }
}
