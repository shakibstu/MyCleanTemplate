using System.ComponentModel.DataAnnotations;

namespace MyCleanTemplate.Application.Contract.Common.Paging;

public sealed record PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 200;

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; init; } = DefaultPageSize;

    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;

    public int Skip => (Page - 1) * PageSize;
}
