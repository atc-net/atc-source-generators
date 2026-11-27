// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    private const string NestedMapperTypes = """
                                             public class AddressDto
                                             {
                                                 public string City { get; set; } = string.Empty;
                                             }

                                             public class Address
                                             {
                                                 public string City { get; set; } = string.Empty;
                                             }
                                             """;

    [Fact]
    public void Generator_Should_Report_Nested_Property_Without_Mapper()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{NestedMapperTypes}}

                                public class CustomerDto
                                {
                                    public string Name { get; set; } = string.Empty;

                                    public AddressDto? Address { get; set; }
                                }

                                [MapTo(typeof(CustomerDto))]
                                public partial class Customer
                                {
                                    public string Name { get; set; } = string.Empty;

                                    public Address? Address { get; set; }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("from 'TestNamespace.Address' to 'TestNamespace.AddressDto'", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.DoesNotContain("MapToAddressDto", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Nested_Constructor_Parameter_Without_Mapper()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{NestedMapperTypes}}

                                public sealed record CustomerDto(string Name, AddressDto? Address);

                                [MapTo(typeof(CustomerDto))]
                                public partial class Customer
                                {
                                    public string Name { get; set; } = string.Empty;

                                    public Address? Address { get; set; }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Contains("return new global::TestNamespace.CustomerDto(", output, StringComparison.Ordinal);
        Assert.DoesNotContain("MapToAddressDto", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Reverse_Nested_Property_Without_Mapper_Once()
    {
        const string source = $$"""
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
                                    public AddressDto? Address { get; set; }
                                }

                                [MapTo(typeof(CustomerDto), Bidirectional = true)]
                                public partial class Customer
                                {
                                    public Address? Address { get; set; }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Address -> AddressDto exists, AddressDto -> Address (for CustomerDto -> Customer) doesn't
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Contains("from 'TestNamespace.AddressDto' to 'TestNamespace.Address'", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
        Assert.DoesNotContain("MapToAddress()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Hand_Written_Nested_Mapper()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace
                                {
                                    {{NestedMapperTypes}}

                                    public class CustomerDto
                                    {
                                        public AddressDto? Address { get; set; }
                                    }

                                    [MapTo(typeof(CustomerDto))]
                                    public partial class Customer
                                    {
                                        public Address? Address { get; set; }
                                    }

                                    public static class AddressMappings
                                    {
                                        public static AddressDto MapToAddressDto(this Address source)
                                            => new() { City = source.City };
                                    }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Nested_Mapper_Of_A_Base_Class()
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

                              public class PostalAddress : Address
                              {
                              }

                              public class CustomerDto
                              {
                                  public AddressDto? Address { get; set; }
                              }

                              [MapTo(typeof(CustomerDto))]
                              public partial class Customer
                              {
                                  public PostalAddress? Address { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMAP015");
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }
}