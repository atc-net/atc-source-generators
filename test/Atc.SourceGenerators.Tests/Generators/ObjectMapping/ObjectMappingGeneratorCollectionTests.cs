// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_List_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class AddressDto
                              {
                                  public string Street { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(AddressDto))]
                              public partial class Address
                              {
                                  public string Street { get; set; } = string.Empty;
                              }

                              public class UserDto
                              {
                                  public int Id { get; set; }
                                  public System.Collections.Generic.List<AddressDto> Addresses { get; set; } = new();
                              }

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public System.Collections.Generic.List<Address> Addresses { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);
        Assert.Contains("Addresses = source.Addresses?.Select(x => x.MapToAddressDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IEnumerable_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ItemDto
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(ItemDto))]
                              public partial class Item
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              public class ContainerDto
                              {
                                  public System.Collections.Generic.IEnumerable<ItemDto> Items { get; set; } = null!;
                              }

                              [MapTo(typeof(ContainerDto))]
                              public partial class Container
                              {
                                  public System.Collections.Generic.IEnumerable<Item> Items { get; set; } = null!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToContainerDto", output, StringComparison.Ordinal);
        Assert.Contains("Items = source.Items?.Select(x => x.MapToItemDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IReadOnlyList_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TagDto
                              {
                                  public string Value { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TagDto))]
                              public partial class Tag
                              {
                                  public string Value { get; set; } = string.Empty;
                              }

                              public class PostDto
                              {
                                  public System.Collections.Generic.IReadOnlyList<TagDto> Tags { get; set; } = null!;
                              }

                              [MapTo(typeof(PostDto))]
                              public partial class Post
                              {
                                  public System.Collections.Generic.IReadOnlyList<Tag> Tags { get; set; } = null!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToPostDto", output, StringComparison.Ordinal);
        Assert.Contains("Tags = source.Tags?.Select(x => x.MapToTagDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Array_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ElementDto
                              {
                                  public int Value { get; set; }
                              }

                              [MapTo(typeof(ElementDto))]
                              public partial class Element
                              {
                                  public int Value { get; set; }
                              }

                              public class ArrayContainerDto
                              {
                                  public ElementDto[] Elements { get; set; } = null!;
                              }

                              [MapTo(typeof(ArrayContainerDto))]
                              public partial class ArrayContainer
                              {
                                  public Element[] Elements { get; set; } = null!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToArrayContainerDto", output, StringComparison.Ordinal);
        Assert.Contains("Elements = source.Elements?.Select(x => x.MapToElementDto()).ToArray()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Collection_ObjectModel_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ValueDto
                              {
                                  public string Data { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(ValueDto))]
                              public partial class Value
                              {
                                  public string Data { get; set; } = string.Empty;
                              }

                              public class CollectionDto
                              {
                                  public System.Collections.ObjectModel.Collection<ValueDto> Values { get; set; } = new();
                              }

                              [MapTo(typeof(CollectionDto))]
                              public partial class CollectionContainer
                              {
                                  public System.Collections.ObjectModel.Collection<Value> Values { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCollectionDto", output, StringComparison.Ordinal);
        Assert.Contains("new global::System.Collections.ObjectModel.Collection<TestNamespace.ValueDto>(source.Values?.Select(x => x.MapToValueDto()).ToList()!)", output, StringComparison.Ordinal);
    }
}