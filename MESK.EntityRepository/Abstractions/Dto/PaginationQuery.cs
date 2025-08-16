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
    public MatchModes MatchMode { get; set; }
    
    /// <summary>
    /// Gets or sets the string value to filter on.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Defines pagination, sorting, and filtering parameters for a query.
/// </summary>
public class PaginationQuery
{
    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }
    
    /// <summary>
    /// Gets the maximum number of items to include in a page.
    /// </summary>
    public int PageSize { get; init; }
    
    /// <summary>
    /// Gets the name of the field to sort results by. Null if no sorting is applied.
    /// </summary>
    public string? SortField { get; init; }
    
    /// <summary>
    /// Gets or sets the sort direction (Ascending by default).
    /// </summary>
    public SortDirections SortDirection { get; set; } = SortDirections.Asc;
    
    /// <summary>
    /// Gets or sets the filter to apply as a key-value pair:
    /// Key → field name, Value → filter value and match mode.
    /// </summary>
    public KeyValuePair<string, FiltersValue> Filters { get; set; }
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