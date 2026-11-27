// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Map_Private_Properties_With_IncludePrivateMembers()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetDto), IncludePrivateMembers = true)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  private string PrivateCode { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string PrivateCode { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("UnsafeAccessor", output, StringComparison.Ordinal);
        Assert.Contains("UnsafeGetSource_PrivateCode", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Internal_Properties_With_IncludePrivateMembers()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetDto), IncludePrivateMembers = true)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  internal int SecurityLevel { get; set; }
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public int SecurityLevel { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("UnsafeAccessor", output, StringComparison.Ordinal);
        Assert.Contains("UnsafeGetSource_SecurityLevel", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Map_Private_Properties_When_IncludePrivateMembers_Is_False()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetDto), IncludePrivateMembers = false)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  private string PrivateCode { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string PrivateCode { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("[global::System.Runtime.CompilerServices.UnsafeAccessor(", output, StringComparison.Ordinal);
        Assert.DoesNotContain("PrivateCode", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_UnsafeAccessor_Setter_For_UpdateTarget()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetDto), IncludePrivateMembers = true, UpdateTarget = true)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  private string Code { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  private string Code { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("UnsafeAccessor", output, StringComparison.Ordinal);
        Assert.Contains("UnsafeGetSource_Code", output, StringComparison.Ordinal);
        Assert.Contains("UnsafeSetTargetDto_Code", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Work_With_Bidirectional_And_IncludePrivateMembers()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetDto), IncludePrivateMembers = true, Bidirectional = true)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  private string InternalData { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  private string InternalData { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
        Assert.Contains("UnsafeGetSource_InternalData", output, StringComparison.Ordinal);
        Assert.Contains("UnsafeGetTargetDto_InternalData", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Mix_Public_And_Private_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetDto), IncludePrivateMembers = true)]
                              public partial class Source
                              {
                                  public int PublicId { get; set; }
                                  private string PrivateCode { get; set; } = string.Empty;
                                  internal int InternalLevel { get; set; }
                                  public string PublicName { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int PublicId { get; set; }
                                  public string PrivateCode { get; set; } = string.Empty;
                                  public int InternalLevel { get; set; }
                                  public string PublicName { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should use direct access for public properties
        Assert.Contains("PublicId = source.PublicId", output, StringComparison.Ordinal);
        Assert.Contains("PublicName = source.PublicName", output, StringComparison.Ordinal);

        // Should use UnsafeAccessor for private/internal properties
        Assert.Contains("UnsafeGetSource_PrivateCode", output, StringComparison.Ordinal);
        Assert.Contains("UnsafeGetSource_InternalLevel", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Read_Private_Members_Through_UnsafeAccessor_For_Conversions()
    {
        const string source = """
                              #nullable enable
                              using System;
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class LineDto
                              {
                                  public int Quantity { get; set; }
                              }

                              [MapTo(typeof(LineDto))]
                              public partial class Line
                              {
                                  public int Quantity { get; set; }
                              }

                              public class Address
                              {
                                  public string City { get; set; } = string.Empty;
                              }

                              public class OrderDto
                              {
                                  public long Count { get; set; }

                                  public string Created { get; set; } = string.Empty;

                                  public List<LineDto> Lines { get; set; } = [];

                                  public string? AddressCity { get; set; }
                              }

                              [MapTo(typeof(OrderDto), IncludePrivateMembers = true, EnableFlattening = true)]
                              public partial class Order
                              {
                                  private int Count { get; set; }

                                  private DateTime Created { get; set; }

                                  private List<Line> Lines { get; set; } = [];

                                  private Address? Address { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("source.Count", output, StringComparison.Ordinal);
        Assert.DoesNotContain("source.Created", output, StringComparison.Ordinal);
        Assert.DoesNotContain("source.Lines", output, StringComparison.Ordinal);
        Assert.DoesNotContain("source.Address?.City", output, StringComparison.Ordinal);
        Assert.Contains("?.City", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Two_Private_Nested_Properties_In_One_Mapping()
    {
        const string source = """
                              #nullable enable
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class AddressDto
                              {
                                  public string City { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(AddressDto))]
                              public partial class Address
                              {
                                  public string City { get; set; } = string.Empty;
                              }

                              public class CustomerDto
                              {
                                  public AddressDto? Home { get; set; }

                                  public AddressDto? Work { get; set; }
                              }

                              [MapTo(typeof(CustomerDto), IncludePrivateMembers = true)]
                              public partial class Customer
                              {
                                  private Address? Home { get; set; }

                                  private Address? Work { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("is { } __home ? __home.MapToAddressDto() : default!", output, StringComparison.Ordinal);
        Assert.Contains("is { } __work ? __work.MapToAddressDto() : default!", output, StringComparison.Ordinal);
    }
}