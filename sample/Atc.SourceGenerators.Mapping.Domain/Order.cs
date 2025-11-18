namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents an order in the system (demonstrates bidirectional record-to-record constructor mapping).
/// </summary>
/// <param name="Id">The order's unique identifier.</param>
/// <param name="CustomerName">The customer's name.</param>
/// <param name="TotalAmount">The order total amount.</param>
/// <param name="OrderDate">When the order was placed.</param>
[MapTo(typeof(OrderDto), Bidirectional = true)]
public partial record Order(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    DateTimeOffset OrderDate);