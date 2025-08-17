namespace MESK.EntityRepository.Abstractions.Dto;

/// <summary>
/// Specifies the direction of sorting for query results.
/// </summary>
public enum SortDirections
{
    /// <summary>
    /// Sort results in ascending order.
    /// </summary>
    Asc,
    
    /// <summary>
    /// Sort results in descending order.
    /// </summary>
    Desc
}

/// <summary>
/// Specifies the type of string matching to apply in filters.
/// </summary>
public enum MatchModes
{
    /// <summary>
    /// Matches values that are exactly equal to the filter value.
    /// </summary>
    Equals,
    
    /// <summary>
    /// Matches values that are not equal to the filter value.
    /// </summary>
    NotEquals,
    
    /// <summary>
    /// Matches values that are greater than the filter value.
    /// Typically used for numeric or date fields.
    /// </summary>
    GreaterThan,
    
    /// <summary>
    /// Matches values that are greater than or equal to the filter value.
    /// Typically used for numeric or date fields.
    /// </summary>
    GreaterThanOrEqual,
    
    /// <summary>
    /// Matches values that are less than the filter value.
    /// Typically used for numeric or date fields.
    /// </summary>
    LessThan,
    
    /// <summary>
    /// Matches values that are less than or equal to the filter value.
    /// Typically used for numeric or date fields.
    /// </summary>
    LessThanOrEqual,
    
    /// <summary>
    /// Match values that contain the given string.
    /// </summary>
    Contains,
    
    /// <summary>
    /// Match values that start with the given string.
    /// </summary>
    StartsWith,
    
    /// <summary>
    /// Match values that end with the given string.
    /// </summary>
    EndsWith
}

/// <summary>
/// Represents the filter configuration for a field, including match mode and filter value.
/// </summary>
public sealed class FiltersValue
{
    /// <summary>
    /// Gets or sets the string matching mode to apply (e.g., Contains, StartsWith).
    /// </summary>
    public MatchModes MatchMode { get; init; }
    
    /// <summary>
    /// Gets or sets the string value to filter on.
    /// </summary>
    public object Value { get; init; } = default!;
}

/// <summary>
/// Defines pagination, sorting, and filtering parameters for a query.
/// </summary>
public class PaginationQuery
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    
    /// <summary>
    /// Gets the current page number (1-based).
    /// Defaults to <c>1</c>.
    /// Must be greater than or equal to <c>1</c>.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets the maximum number of items to include in a page.
    /// Defaults to <c>10</c>. 
    /// Must be greater than <c>0</c>.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 10 : value;
    }
    
    /// <summary>
    /// Gets the name of the field to sort results by. Null if no sorting is applied.
    /// </summary>
    public string? SortField { get; init; }
    
    /// <summary>
    /// Gets or sets the sort direction (Ascending by default).
    /// </summary>
    public SortDirections SortDirection { get; init; } = SortDirections.Asc;
    
    /// <summary>
    /// Gets or sets multiple filters to apply.
    /// Key → property name, Value → filter operator and value.
    /// </summary>
    public Dictionary<string, FiltersValue>? Filters { get; init; }
}

/// <summary>
/// Represents the result of a paginated query, including items and metadata.
/// </summary>
/// <typeparam name="T">The type of items contained in the result.</typeparam>
public class PaginationResult<T>
{
    /// <summary>
    /// Gets the list of items returned for the current page.
    /// </summary>
    public List<T> Items { get; init; } = new();
    
    /// <summary>
    /// Gets the number of items returned in the current page.
    /// </summary>
    public int Count { get; init; }
    
    /// <summary>
    /// Gets the total number of items across all pages (ignores pagination).
    /// </summary>
    public int TotalCount { get; init; }
    
    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of items in each page.
    /// </summary>
    public int PageSize { get; set; }
}