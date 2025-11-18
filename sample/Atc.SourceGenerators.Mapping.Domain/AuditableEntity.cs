namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Auditable entity class with audit trail properties.
/// Demonstrates multi-level inheritance in mapping (BaseEntity → AuditableEntity → Concrete Entity).
/// </summary>
public abstract partial class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets when the entity was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}