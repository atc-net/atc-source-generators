namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Generic paged result DTO demonstrating generic type mapping with constraints.
/// Maps from PagedResult&lt;T&gt; where T must be a class.
/// </summary>
/// <typeparam name="T">The type of items in the paged result.</typeparam>
public class PagedResultDto<T>
    where T : class
{
    /// <summary>
    /// Gets or sets the items in the current page.
    /// </summary>
    public ICollection<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets a value indicating whether there are more pages available.
    /// </summary>
    public bool HasNextPage { get; set; }
}