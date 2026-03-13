// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Register_Decorator_With_Conditional_Check()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IMyService
                              {
                                  void Execute();
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IMyService))]
                              public class MyService : IMyService
                              {
                                  public void Execute() { }
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IMyService), Decorator = true, Condition = "Features:EnableLogging")]
                              public class LoggingMyServiceDecorator : IMyService
                              {
                                  private readonly IMyService inner;

                                  public LoggingMyServiceDecorator(IMyService inner)
                                  {
                                      this.inner = inner;
                                  }

                                  public void Execute() => inner.Execute();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify base service is registered
        Assert.Contains("services.AddScoped<TestNamespace.IMyService, TestNamespace.MyService>()", output, StringComparison.Ordinal);

        // Verify conditional check wraps the decorator registration
        Assert.Contains("if (configuration.GetValue<bool>(\"Features:EnableLogging\"))", output, StringComparison.Ordinal);

        // Verify decorator uses Decorate method
        Assert.Contains("services.Decorate<TestNamespace.IMyService>((provider, inner) =>", output, StringComparison.Ordinal);
        Assert.Contains("ActivatorUtilities.CreateInstance<TestNamespace.LoggingMyServiceDecorator>(provider, inner)", output, StringComparison.Ordinal);

        // Verify IConfiguration parameter is present in method signature
        Assert.Contains("IConfiguration configuration", output, StringComparison.Ordinal);
    }
}