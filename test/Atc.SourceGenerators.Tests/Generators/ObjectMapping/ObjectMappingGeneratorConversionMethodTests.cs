// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    private const string MoneyType = """
                                     public sealed record Money(decimal Value);
                                     """;

    [Fact]
    public void Generator_Should_Use_Conversion_Method_Of_The_MapTo_Class()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{MoneyType}}

                                public class InvoiceDto
                                {
                                    public Money Amount { get; set; } = new(0m);
                                }

                                [MapTo(typeof(InvoiceDto))]
                                public partial class Invoice
                                {
                                    public decimal Amount { get; set; }

                                    internal static Money ToMoney(decimal amount) => new(amount);
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Amount = global::TestNamespace.Invoice.ToMoney(source.Amount)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Conversion_Method_For_Constructor_Parameter_And_Reverse_Mapping()
    {
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{MoneyType}}

                                public sealed record InvoiceDto(Money Amount);

                                [MapTo(typeof(InvoiceDto), Bidirectional = true)]
                                public partial class Invoice
                                {
                                    public decimal Amount { get; set; }

                                    public static Money ToMoney(decimal amount) => new(amount);

                                    public static decimal FromMoney(Money money) => money.Value;
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("return new global::TestNamespace.InvoiceDto(", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.Invoice.ToMoney(source.Amount)", output, StringComparison.Ordinal);
        Assert.Contains("Amount = global::TestNamespace.Invoice.FromMoney(source.Amount)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_Conversion_Method_Over_Built_In_Conversion()
    {
        const string source = """
                              #nullable enable
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class EventDto
                              {
                                  public string Date { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(EventDto))]
                              public partial class Event
                              {
                                  public DateTime Date { get; set; }

                                  internal static string Format(DateTime date) => date.ToString("yyyy-MM-dd");
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Date = global::TestNamespace.Event.Format(source.Date)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Use_Private_Conversion_Method()
    {
        // The generated mapping is an extension method in another class: a private method can't be called
        const string source = $$"""
                                #nullable enable
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace;

                                {{MoneyType}}

                                public class InvoiceDto
                                {
                                    public Money Amount { get; set; } = new(0m);
                                }

                                [MapTo(typeof(InvoiceDto))]
                                public partial class Invoice
                                {
                                    public decimal Amount { get; set; }

                                    private static Money ToMoney(decimal amount) => new(amount);
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("ToMoney(", output, StringComparison.Ordinal);
    }
}