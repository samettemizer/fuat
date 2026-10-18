namespace FuatAmca.Api.Common.Pagination;

/// <summary>
/// Datagrid tarafinin bekledigi sayfali liste yaniti.
/// </summary>
public sealed record PagedResult<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResult<TItem> Empty(int page, int pageSize) => new([], 0, page, pageSize);
}
