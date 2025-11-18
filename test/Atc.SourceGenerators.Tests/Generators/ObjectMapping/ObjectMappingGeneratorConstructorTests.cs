// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Map_Class_To_Record()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);

        // Should use constructor call (constructor mapping feature)
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Record_To_Record()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto))]
                              public partial record Source(int Id, string Name);
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);

        // Should use constructor call (constructor mapping feature)
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Record_To_Class()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial record Source(int Id, string Name);
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_For_Simple_Record()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should use constructor call instead of object initializer
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);

        // Should NOT use object initializer syntax
        Assert.DoesNotContain("Id = source.Id", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_For_Record_With_All_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record UserDto(int Id, string Name, string Email, int Age);

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                                  public int Age { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should use constructor call with all parameters
        Assert.Contains("return new TestNamespace.UserDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("source.Email,", output, StringComparison.Ordinal);
        Assert.Contains("source.Age", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Mixed_Constructor_And_Initializer_For_Record_With_Extra_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name)
                              {
                                  public string Email { get; set; } = string.Empty;
                                  public int Age { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                                  public int Age { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should use constructor for primary parameters
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);

        // Should use object initializer for extra properties
        Assert.Contains("Email = source.Email,", output, StringComparison.Ordinal);
        Assert.Contains("Age = source.Age", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_For_Bidirectional_Record_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto), Bidirectional = true)]
                              public partial record Source(int Id, string Name);
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: Source.MapToTargetDto() - should use constructor
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);

        // Reverse mapping: TargetDto.MapToSource() - should also use constructor
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
        Assert.Contains("return new TestNamespace.Source(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_With_Nested_Object_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record AddressDto(string Street, string City);

                              [MapTo(typeof(AddressDto))]
                              public partial record Address(string Street, string City);

                              public record UserDto(int Id, string Name, AddressDto? Address);

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public Address? Address { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should use constructor with nested mapping
        Assert.Contains("return new TestNamespace.UserDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_With_Enum_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus { Active = 0, Inactive = 1 }

                              public enum TargetStatus { Active = 0, Inactive = 1 }

                              public record UserDto(int Id, string Name, TargetStatus Status);

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public SourceStatus Status { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should use constructor with enum mapping method
        Assert.Contains("return new TestNamespace.UserDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("source.Status.MapToTargetStatus()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_With_Collection_In_Initializer()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TagDto(string Value);

                              [MapTo(typeof(TagDto))]
                              public partial record Tag(string Value);

                              public record PostDto(int Id, string Title)
                              {
                                  public System.Collections.Generic.List<TagDto> Tags { get; set; } = new();
                              }

                              [MapTo(typeof(PostDto))]
                              public partial class Post
                              {
                                  public int Id { get; set; }
                                  public string Title { get; set; } = string.Empty;
                                  public System.Collections.Generic.List<Tag> Tags { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToPostDto", output, StringComparison.Ordinal);

        // Should use constructor for Id and Title
        Assert.Contains("return new TestNamespace.PostDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Title", output, StringComparison.Ordinal);

        // Should use initializer for collection
        Assert.Contains("Tags = source.Tags?.Select(x => x.MapToTagDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Case_Insensitive_Constructor_Parameter_Matching()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int id, string name);

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should match properties to constructor parameters case-insensitively
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
    }
}