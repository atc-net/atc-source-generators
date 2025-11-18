namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Represents a book DTO.
/// Note: This class doesn't need to inherit from any base class - the mapper will handle properties from all levels of the source hierarchy.
/// </summary>
public class BookDto
{
    /// <summary>
    /// Gets or sets the unique identifier (from BaseEntity).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets when the entity was created (from BaseEntity).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the entity was last updated (from AuditableEntity).
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who last updated the entity (from AuditableEntity).
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the book title (from Book).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author name (from Book).
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ISBN (from Book).
    /// </summary>
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publication year (from Book).
    /// </summary>
    public int PublicationYear { get; set; }

    /// <summary>
    /// Gets or sets the price (from Book).
    /// </summary>
    public decimal Price { get; set; }
}