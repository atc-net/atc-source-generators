namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Data transfer object for Order (demonstrates record-to-record constructor mapping).
/// </summary>
/// <param name="Id">The order's unique identifier.</param>
/// <param name="CustomerName">The customer's name.</param>
/// <param name="TotalAmount">The order total amount.</param>
/// <param name="OrderDate">When the order was placed.</param>
public record OrderDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    DateTimeOffset OrderDate);