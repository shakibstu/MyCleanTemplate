namespace MyCleanTemplate.Application.Contract.Common.Paging;

public sealed record Page<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    long TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public static Page<T> Empty(PageRequest request) =>
        new(Array.Empty<T>(), request.Page, request.PageSize, 0);
}
