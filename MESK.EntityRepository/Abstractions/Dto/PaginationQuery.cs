namespace MESK.EntityRepository.Abstractions.Dto;

public enum SortDirections
{
    Asc,
    Desc
}

public enum MatchModes
{
    Contains,
    StartsWith,
    EndsWith
}

public sealed class FiltersValue
{
    public MatchModes MatchMode { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class PaginationQuery
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public string? SortField { get; init; }
    public SortDirections SortDirection { get; set; } = SortDirections.Asc;
    public KeyValuePair<string, FiltersValue> Filters { get; set; }
}

public class PaginationResult<T>
{
    public List<T> Items { get; init; } = new();
    public int Count { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}