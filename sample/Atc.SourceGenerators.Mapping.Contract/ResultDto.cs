namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Generic result DTO demonstrating generic type mapping.
/// Maps from Result&lt;T&gt; with preserved type parameters.
/// </summary>
/// <typeparam name="T">The type of data in the result.</typeparam>
public partial class ResultDto<T>
{
    /// <summary>
    /// Gets or sets the result data.
    /// </summary>
    public T Data { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets an optional message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional error code.
    /// </summary>
    public string? ErrorCode { get; set; }
}