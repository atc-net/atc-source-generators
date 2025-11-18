// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Include_Base_Class_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public abstract partial class BaseEntity
                              {
                                  public System.Guid Id { get; set; }
                                  public System.DateTime CreatedAt { get; set; }
                              }

                              [MapTo(typeof(UserDto))]
                              public partial class UserEntity : BaseEntity
                              {
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                              }

                              public class UserDto
                              {
                                  public System.Guid Id { get; set; }
                                  public System.DateTime CreatedAt { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("CreatedAt = source.CreatedAt", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.Contains("Email = source.Email", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Multiple_Inheritance_Levels()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public abstract partial class Entity
                              {
                                  public System.Guid Id { get; set; }
                              }

                              public abstract partial class AuditableEntity : Entity
                              {
                                  public System.DateTime CreatedAt { get; set; }
                                  public System.DateTime? UpdatedAt { get; set; }
                              }

                              [MapTo(typeof(ProductDto))]
                              public partial class Product : AuditableEntity
                              {
                                  public string Name { get; set; } = string.Empty;
                                  public decimal Price { get; set; }
                              }

                              public class ProductDto
                              {
                                  public System.Guid Id { get; set; }
                                  public System.DateTime CreatedAt { get; set; }
                                  public System.DateTime? UpdatedAt { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public decimal Price { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("CreatedAt = source.CreatedAt", output, StringComparison.Ordinal);
        Assert.Contains("UpdatedAt = source.UpdatedAt", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.Contains("Price = source.Price", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Overridden_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public abstract partial class Animal
                              {
                                  public virtual string Name { get; set; } = string.Empty;
                                  public int Age { get; set; }
                              }

                              [MapTo(typeof(DogDto))]
                              public partial class Dog : Animal
                              {
                                  public override string Name { get; set; } = "Dog";
                                  public string Breed { get; set; } = string.Empty;
                              }

                              public class DogDto
                              {
                                  public string Name { get; set; } = string.Empty;
                                  public int Age { get; set; }
                                  public string Breed { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should only have one Name mapping (the overridden one)
        var nameCount = CountOccurrences(output, "Name = source.Name");
        Assert.Equal(1, nameCount);
        Assert.Contains("Age = source.Age", output, StringComparison.Ordinal);
        Assert.Contains("Breed = source.Breed", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Respect_MapIgnore_On_Base_Class_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public abstract partial class BaseEntity
                              {
                                  public System.Guid Id { get; set; }

                                  [MapIgnore]
                                  public System.DateTime InternalTimestamp { get; set; }
                              }

                              [MapTo(typeof(UserDto))]
                              public partial class UserEntity : BaseEntity
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              public class UserDto
                              {
                                  public System.Guid Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.DoesNotContain("InternalTimestamp", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Work_With_Bidirectional_And_Base_Classes()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public abstract partial class Entity
                              {
                                  public System.Guid Id { get; set; }
                              }

                              [MapTo(typeof(UserDto), Bidirectional = true)]
                              public partial class User : Entity
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              public partial class UserDto
                              {
                                  public System.Guid Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: User → UserDto
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);

        // Reverse mapping: UserDto → User
        Assert.Contains("MapToUser", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Work_With_PropertyNameStrategy_And_Base_Classes()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public abstract partial class BaseEntity
                              {
                                  public System.Guid EntityId { get; set; }
                                  public System.DateTime CreatedAt { get; set; }
                              }

                              [MapTo(typeof(UserDto), PropertyNameStrategy = PropertyNameStrategy.CamelCase)]
                              public partial class UserEntity : BaseEntity
                              {
                                  public string UserName { get; set; } = string.Empty;
                              }

                              public class UserDto
                              {
                                  #pragma warning disable IDE1006
                                  public System.Guid entityId { get; set; }
                                  public System.DateTime createdAt { get; set; }
                                  public string userName { get; set; } = string.Empty;
                                  #pragma warning restore IDE1006
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("entityId = source.EntityId", output, StringComparison.Ordinal);
        Assert.Contains("createdAt = source.CreatedAt", output, StringComparison.Ordinal);
        Assert.Contains("userName = source.UserName", output, StringComparison.Ordinal);
    }
}