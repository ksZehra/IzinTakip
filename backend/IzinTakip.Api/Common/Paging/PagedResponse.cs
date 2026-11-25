using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Paging;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public static class PagingExtensions
{
    private const int EnFazlaSayfaBoyutu = 100;

    public static async Task<PagedResponse<T>> SayfalaAsync<T>(
        this IQueryable<T> sorgu,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        pageSize = Math.Clamp(pageSize, 1, EnFazlaSayfaBoyutu);

        var toplam = await sorgu.CountAsync(cancellationToken);
        var sonSayfa = Math.Max(1, (int)Math.Ceiling(toplam / (double)pageSize));
        page = Math.Clamp(page, 1, sonSayfa);
        var kayitlar = await sorgu.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResponse<T>(kayitlar, toplam, page, pageSize);
    }
}
