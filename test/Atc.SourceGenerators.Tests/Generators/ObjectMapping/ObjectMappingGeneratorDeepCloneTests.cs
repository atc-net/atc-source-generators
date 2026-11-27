// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    private const string DeepCloneTypes = """
                                          [MapTo(typeof(Address))]
                                          public partial class Address
                                          {
                                              public string City { get; set; } = string.Empty;
                                          }

                                          [MapTo(typeof(OrderLine))]
                                          public partial class OrderLine
                                          {
                                              public int Quantity { get; set; }
                                          }
                                          """;

    [Fact]
    public void Generator_Should_Deep_Clone_Same_Type_Properties()
    {
        const string source = $$"""
                                #nullable enable
                                using System.Collections.Generic;
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{DeepCloneTypes}}

                                [MapTo(typeof(Order), DeepClone = true)]
                                public partial class Order
                                {
                                    public int Id { get; set; }

                                    public string Name { get; set; } = string.Empty;

                                    public Address? Address { get; set; }

                                    public List<OrderLine> Lines { get; set; } = [];

                                    public OrderLine[] LineArray { get; set; } = [];

                                    public Dictionary<string, OrderLine> LinesByKey { get; set; } = [];

                                    public List<string> Tags { get; set; } = [];
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Id = source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("Address = source.Address?.MapToAddress()!,", output, StringComparison.Ordinal);
        Assert.Contains("Lines = source.Lines?.Select(x => x.MapToOrderLine()).ToList()!,", output, StringComparison.Ordinal);
        Assert.Contains("LineArray = source.LineArray?.Select(x => x.MapToOrderLine()).ToArray()!,", output, StringComparison.Ordinal);
        Assert.Contains("LinesByKey = source.LinesByKey?.ToDictionary(x => x.Key, x => x.Value.MapToOrderLine())!,", output, StringComparison.Ordinal);
        Assert.Contains("Tags = source.Tags?.ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Copy_Same_Type_Properties_By_Reference_Without_DeepClone()
    {
        const string source = $$"""
                                #nullable enable
                                using System.Collections.Generic;
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{DeepCloneTypes}}

                                [MapTo(typeof(Order))]
                                public partial class Order
                                {
                                    public Address? Address { get; set; }

                                    public List<OrderLine> Lines { get; set; } = [];
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Address = source.Address,", output, StringComparison.Ordinal);
        Assert.Contains("Lines = source.Lines", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Deep_Clone_A_Tree()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(Node), DeepClone = true)]
                              public partial class Node
                              {
                                  public string Name { get; set; } = string.Empty;

                                  public Node? Parent { get; set; }

                                  public List<Node> Children { get; set; } = [];
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Parent = source.Parent?.MapToNode()!,", output, StringComparison.Ordinal);
        Assert.Contains("Children = source.Children?.Select(x => x.MapToNode()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Deep_Clone_Property_Without_Self_Mapping()
    {
        const string source = """
                              #nullable enable
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class Customer
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(Order), DeepClone = true)]
                              public partial class Order
                              {
                                  public Customer? Customer { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP016");
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("'Customer'", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("Customer = source.Customer", output, StringComparison.Ordinal);
    }
}