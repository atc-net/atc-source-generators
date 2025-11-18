namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a book in the domain layer.
/// Demonstrates 3-level inheritance: BaseEntity → AuditableEntity → Book.
/// The mapping will automatically include Id, CreatedAt (from BaseEntity) and UpdatedAt, UpdatedBy (from AuditableEntity).
/// </summary>
[MapTo(typeof(Contract.BookDto))]
public partial class Book : AuditableEntity
{
    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ISBN.
    /// </summary>
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publication year.
    /// </summary>
    public int PublicationYear { get; set; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public decimal Price { get; set; }
}