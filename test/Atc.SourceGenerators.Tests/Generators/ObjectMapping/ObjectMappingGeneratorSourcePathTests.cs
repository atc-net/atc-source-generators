// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    private const string SourcePathModel = """
                                           public class Address
                                           {
                                               public string City { get; set; } = string.Empty;
                                           }

                                           public class Customer
                                           {
                                               public string Name { get; set; } = string.Empty;

                                               public Address? Address { get; set; }
                                           }
                                           """;

    [Fact]
    public void Generator_Should_Map_Nested_Source_Paths_From_Class_Level_MapProperty()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{SourcePathModel}}

                                public class OrderDto
                                {
                                    public int Id { get; set; }

                                    public string? CustomerName { get; set; }

                                    public string? City { get; set; }
                                }

                                [MapTo(typeof(OrderDto))]
                                [MapProperty("Customer.Name", "CustomerName")]
                                [MapProperty("Customer.Address.City", "City")]
                                public partial class Order
                                {
                                    public int Id { get; set; }

                                    public Customer? Customer { get; set; }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("CustomerName = source.Customer?.Name!", output, StringComparison.Ordinal);
        Assert.Contains("City = source.Customer?.Address?.City!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Nested_Source_Path_To_Constructor_Parameter()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{SourcePathModel}}

                                public sealed record OrderDto(int Id, string? CustomerName);

                                [MapTo(typeof(OrderDto))]
                                [MapProperty("Customer.Name", "CustomerName")]
                                public partial class Order
                                {
                                    public int Id { get; set; }

                                    public Customer? Customer { get; set; }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("return new global::TestNamespace.OrderDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Customer?.Name!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Unknown_Source_Path_In_MapProperty()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{SourcePathModel}}

                                public class OrderDto
                                {
                                    public string? CustomerName { get; set; }
                                }

                                [MapTo(typeof(OrderDto))]
                                [MapProperty("Customer.Missing", "CustomerName")]
                                public partial class Order
                                {
                                    public Customer? Customer { get; set; }
                                }
                                """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP017");
        Assert.Contains("Customer.Missing", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Unknown_Target_Of_Class_Level_MapProperty()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{SourcePathModel}}

                                public class OrderDto
                                {
                                    public string? CustomerName { get; set; }
                                }

                                [MapTo(typeof(OrderDto))]
                                [MapProperty("Customer.Name", "Missing")]
                                public partial class Order
                                {
                                    public Customer? Customer { get; set; }
                                }
                                """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.Single(diagnostics, d => d.Id == "ATCMAP003");
    }

    [Fact]
    public void Generator_Should_Apply_Class_Level_MapProperty_Only_To_Targets_With_The_Property()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{SourcePathModel}}

                                public class OrderDto
                                {
                                    public string? CustomerName { get; set; }
                                }

                                public class OrderSummaryDto
                                {
                                    public int Id { get; set; }
                                }

                                [MapTo(typeof(OrderDto))]
                                [MapTo(typeof(OrderSummaryDto))]
                                [MapProperty("Customer.Name", "CustomerName")]
                                public partial class Order
                                {
                                    public int Id { get; set; }

                                    public Customer? Customer { get; set; }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("CustomerName = source.Customer?.Name!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Flatten_A_Target_Mapped_By_A_Source_Path()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{SourcePathModel}}

                                public class CustomerDto
                                {
                                    public string? AddressCity { get; set; }
                                }

                                [MapTo(typeof(CustomerDto), EnableFlattening = true)]
                                [MapProperty("Address.City", "AddressCity")]
                                public partial class CustomerSource
                                {
                                    public Address? Address { get; set; }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(output, "AddressCity = "));
    }
}