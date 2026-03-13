// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Update_Target_Method()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class UserDto
                              {
                                  public Guid Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(UserDto), UpdateTarget = true)]
                              public partial class User
                              {
                                  public Guid Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        // Should generate standard method
        Assert.Contains("public static TestNamespace.UserDto MapToUserDto(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.User source)", output, StringComparison.Ordinal);

        // Should also generate update target overload
        Assert.Contains("public static void MapToUserDto(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.User source,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.UserDto target)", output, StringComparison.Ordinal);

        // Update method should have property assignments
        Assert.Contains("target.Id = source.Id;", output, StringComparison.Ordinal);
        Assert.Contains("target.Name = source.Name;", output, StringComparison.Ordinal);
        Assert.Contains("target.Email = source.Email;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Update_Target_Method_When_False()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class UserDto
                              {
                                  public Guid Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(UserDto), UpdateTarget = false)]
                              public partial class User
                              {
                                  public Guid Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        // Should generate standard method
        Assert.Contains("public static TestNamespace.UserDto MapToUserDto(", output, StringComparison.Ordinal);

        // Should NOT generate update target overload when UpdateTarget = false
        Assert.DoesNotContain("public static void MapToUserDto(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_Hooks_In_Update_Target_Method()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class OrderDto
                              {
                                  public Guid Id { get; set; }
                                  public decimal Total { get; set; }
                              }

                              [MapTo(typeof(OrderDto), UpdateTarget = true, BeforeMap = nameof(ValidateOrder), AfterMap = nameof(EnrichOrder))]
                              public partial class Order
                              {
                                  public Guid Id { get; set; }
                                  public decimal Total { get; set; }

                                  internal static void ValidateOrder(Order source)
                                  {
                                      if (source.Total < 0)
                                          throw new ArgumentException("Total cannot be negative");
                                  }

                                  internal static void EnrichOrder(Order source, OrderDto target)
                                  {
                                      // Custom enrichment logic
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        // Should generate update target overload with hooks
        Assert.Contains("public static void MapToOrderDto(", output, StringComparison.Ordinal);

        // Both hooks should be present in the generated code
        Assert.Contains("Order.ValidateOrder(source);", output, StringComparison.Ordinal);
        Assert.Contains("Order.EnrichOrder(source, target);", output, StringComparison.Ordinal);

        // Property assignment should be present in update method
        Assert.Contains("target.Id = source.Id;", output, StringComparison.Ordinal);
        Assert.Contains("target.Total = source.Total;", output, StringComparison.Ordinal);
    }
}