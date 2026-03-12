namespace Atc.SourceGenerators.MappingOnlyConfiguration.Domain;

/// <summary>
/// Domain model for a customer - our own type that we control.
/// </summary>
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public Address? Address { get; set; }

    public CustomerCategory Category { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}