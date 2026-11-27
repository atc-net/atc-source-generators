// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

/// <summary>
/// A [MapTo] mapping whose nested type is mapped by a configuration mapping: the generated call must stay,
/// without the missing-mapper diagnostic (ATCMAP015).
/// </summary>
public partial class MappingConfigurationGeneratorTests
{
    private const string MixedNestedTypes = """
                                            public class AddressDto
                                            {
                                                public string City { get; set; } = string.Empty;
                                            }

                                            public class Address
                                            {
                                                public string City { get; set; } = string.Empty;
                                            }

                                            public class CustomerDto
                                            {
                                                public AddressDto? Address { get; set; }
                                            }

                                            [MapTo(typeof(CustomerDto))]
                                            public partial class Customer
                                            {
                                                public Address? Address { get; set; }
                                            }
                                            """;

    [Fact]
    public void MapTo_Should_Use_Nested_Mapper_From_MappingConfiguration()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace MyApp
                                {
                                    {{MixedNestedTypes}}

                                    [MappingConfiguration]
                                    public static partial class Mappings
                                    {
                                        public static partial AddressDto MapToAddressDto(this Address source);
                                    }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMAP");

        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void MapTo_Should_Use_Nested_Mapper_From_Assembly_MapTypes()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                [assembly: MapTypes(typeof(MyApp.Address), typeof(MyApp.AddressDto))]

                                namespace MyApp
                                {
                                    {{MixedNestedTypes}}
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMAP");

        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void MapTo_Should_Use_Nested_Mapper_From_MappingBuilder()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace MyApp
                                {
                                    {{MixedNestedTypes}}

                                    public static class Startup
                                    {
                                        public static void Configure()
                                            => MappingBuilder.Configure(map => map.Map<Address, AddressDto>());
                                    }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMAP");

        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }
}