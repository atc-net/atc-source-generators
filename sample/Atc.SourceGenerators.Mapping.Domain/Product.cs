namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a product in the system (demonstrates constructor mapping).
/// </summary>
[MapTo(typeof(ProductDto))]
public partial class Product
{
    /// <summary>
    /// Gets or sets the product's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the product's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product's price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the product's description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the product was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}