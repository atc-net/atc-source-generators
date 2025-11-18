namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Data transfer object for Product (demonstrates constructor mapping with records).
/// </summary>
/// <param name="Id">The product's unique identifier.</param>
/// <param name="Name">The product's name.</param>
/// <param name="Price">The product's price.</param>
/// <param name="Description">The product's description.</param>
/// <param name="CreatedAt">When the product was created.</param>
public record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Description,
    DateTimeOffset CreatedAt);


