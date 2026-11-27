// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    private const string SourcePathTypes = """
                                           public class Address
                                           {
                                               public string City { get; set; } = string.Empty;

                                               public int ZipCode { get; set; }
                                           }

                                           public class Customer
                                           {
                                               public string Name { get; set; } = string.Empty;

                                               public Address? Address { get; set; }
                                           }

                                           public class Order
                                           {
                                               public int Id { get; set; }

                                               public Customer? Customer { get; set; }
                                           }
                                           """;

    [Fact]
    public void Config_Should_Map_Nested_Source_Path_With_MapConfigProperty()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace MyApp
                                {
                                    {{SourcePathTypes}}

                                    public class OrderDto
                                    {
                                        public int Id { get; set; }

                                        public string? CustomerName { get; set; }

                                        public string? City { get; set; }

                                        public int ZipCode { get; set; }
                                    }

                                    [MappingConfiguration]
                                    public static partial class Mappings
                                    {
                                        [MapConfigProperty("Customer.Name", "CustomerName")]
                                        [MapConfigProperty("Customer.Address.City", "City")]
                                        [MapConfigProperty("Customer.Address.ZipCode", "ZipCode")]
                                        public static partial OrderDto MapToOrderDto(this Order source);
                                    }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("CustomerName = source.Customer?.Name!", output, StringComparison.Ordinal);
        Assert.Contains("City = source.Customer?.Address?.City!", output, StringComparison.Ordinal);
        Assert.Contains("ZipCode = source.Customer?.Address?.ZipCode ?? default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Nested_Source_Path_To_Constructor_Parameter()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace MyApp
                                {
                                    {{SourcePathTypes}}

                                    public sealed record OrderDto(int Id, string? CustomerName);

                                    [MappingConfiguration]
                                    public static partial class Mappings
                                    {
                                        [MapConfigProperty("Customer.Name", "CustomerName")]
                                        public static partial OrderDto MapToOrderDto(this Order source);
                                    }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("return new global::MyApp.OrderDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Customer?.Name!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Assembly_MapTypes_Should_Map_Nested_Source_Path_From_PropertyMap()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                [assembly: MapTypes(typeof(MyApp.Order), typeof(MyApp.OrderDto), PropertyMap = new[] { "Customer.Name:CustomerName" })]

                                namespace MyApp
                                {
                                    {{SourcePathTypes}}

                                    public class OrderDto
                                    {
                                        public int Id { get; set; }

                                        public string? CustomerName { get; set; }
                                    }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("CustomerName = source.Customer?.Name!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Report_Unknown_Nested_Source_Path()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace MyApp
                                {
                                    {{SourcePathTypes}}

                                    public class OrderDto
                                    {
                                        public int Id { get; set; }

                                        public string? CustomerName { get; set; }
                                    }

                                    [MappingConfiguration]
                                    public static partial class Mappings
                                    {
                                        [MapConfigProperty("Customer.Missing", "CustomerName")]
                                        public static partial OrderDto MapToOrderDto(this Order source);
                                    }
                                }
                                """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMCF006");
        Assert.Contains("Customer.Missing", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}