namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Base entity class with common properties for all domain entities.
/// Demonstrates base class property inheritance in mapping.
/// </summary>
public abstract partial class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}