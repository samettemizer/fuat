namespace FuatAmca.Api.Common.Pagination;

/// <summary>
/// Sayfalama parametrelerini normalize eden yardimci.
/// </summary>
public static class PaginationRules
{
    public const int DefaultPageSize = 25;

    public const int MaxPageSize = 500;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var normalizedPage = page is null or < 1 ? 1 : page.Value;
        var normalizedPageSize = pageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize.Value
        };

        return (normalizedPage, normalizedPageSize);
    }
}
