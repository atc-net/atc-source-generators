// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Register_Decorator_With_Scoped_Lifetime()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IOrderService
                              {
                                  Task PlaceOrderAsync(string orderId);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IOrderService))]
                              public class OrderService : IOrderService
                              {
                                  public Task PlaceOrderAsync(string orderId) => Task.CompletedTask;
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
                              public class LoggingOrderServiceDecorator : IOrderService
                              {
                                  private readonly IOrderService inner;

                                  public LoggingOrderServiceDecorator(IOrderService inner)
                                  {
                                      this.inner = inner;
                                  }

                                  public Task PlaceOrderAsync(string orderId) => inner.PlaceOrderAsync(orderId);
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify base service is registered first
        Assert.Contains("services.AddScoped<TestNamespace.IOrderService, TestNamespace.OrderService>()", output, StringComparison.Ordinal);

        // Verify decorator uses Decorate method
        Assert.Contains("services.Decorate<TestNamespace.IOrderService>((provider, inner) =>", output, StringComparison.Ordinal);
        Assert.Contains("return ActivatorUtilities.CreateInstance<TestNamespace.LoggingOrderServiceDecorator>(provider, inner);", output, StringComparison.Ordinal);

        // Verify Decorate helper method is generated
        Assert.Contains("private static IServiceCollection Decorate<TService>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Decorator_With_Singleton_Lifetime()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICacheService
                              {
                                  void Set(string key, string value);
                              }

                              [Registration(Lifetime.Singleton, As = typeof(ICacheService))]
                              public class CacheService : ICacheService
                              {
                                  public void Set(string key, string value) { }
                              }

                              [Registration(Lifetime.Singleton, As = typeof(ICacheService), Decorator = true)]
                              public class CachingDecorator : ICacheService
                              {
                                  private readonly ICacheService inner;

                                  public CachingDecorator(ICacheService inner)
                                  {
                                      this.inner = inner;
                                  }

                                  public void Set(string key, string value) => inner.Set(key, value);
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.ICacheService, TestNamespace.CacheService>()", output, StringComparison.Ordinal);
        Assert.Contains("services.Decorate<TestNamespace.ICacheService>((provider, inner) =>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Skip_Decorator_Without_Explicit_As_Parameter()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration(Decorator = true)]
                              public class InvalidDecorator
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // No errors - decorator is just skipped
        Assert.Empty(diagnostics);

        // Verify decorator is not registered (no Decorate call)
        Assert.DoesNotContain("InvalidDecorator", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Multiple_Decorators_In_Order()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService
                              {
                                  void Execute();
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IService))]
                              public class BaseService : IService
                              {
                                  public void Execute() { }
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IService), Decorator = true)]
                              public class LoggingDecorator : IService
                              {
                                  private readonly IService inner;
                                  public LoggingDecorator(IService inner) => this.inner = inner;
                                  public void Execute() => inner.Execute();
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IService), Decorator = true)]
                              public class ValidationDecorator : IService
                              {
                                  private readonly IService inner;
                                  public ValidationDecorator(IService inner) => this.inner = inner;
                                  public void Execute() => inner.Execute();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify base service is registered
        Assert.Contains("services.AddScoped<TestNamespace.IService, TestNamespace.BaseService>()", output, StringComparison.Ordinal);

        // Verify both decorators are registered
        Assert.Contains("TestNamespace.LoggingDecorator", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.ValidationDecorator", output, StringComparison.Ordinal);

        // Verify both decorator registrations are present
        Assert.Contains("ActivatorUtilities.CreateInstance<TestNamespace.LoggingDecorator>", output, StringComparison.Ordinal);
        Assert.Contains("ActivatorUtilities.CreateInstance<TestNamespace.ValidationDecorator>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Decorate_Helper_Methods()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration(As = typeof(IService))]
                              public class Service : IService { }

                              [Registration(As = typeof(IService), Decorator = true)]
                              public class Decorator : IService
                              {
                                  public Decorator(IService inner) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify generic Decorate method exists
        Assert.Contains("private static IServiceCollection Decorate<TService>", output, StringComparison.Ordinal);
        Assert.Contains("where TService : class", output, StringComparison.Ordinal);
        Assert.Contains("this IServiceCollection services,", output, StringComparison.Ordinal);
        Assert.Contains("global::System.Func<global::System.IServiceProvider, TService, TService> decorator", output, StringComparison.Ordinal);

        // Verify non-generic Decorate method exists for open generics
        Assert.Contains("private static IServiceCollection Decorate(", output, StringComparison.Ordinal);
        Assert.Contains("global::System.Type serviceType,", output, StringComparison.Ordinal);

        // Verify error handling in Decorate method
        Assert.Contains("throw new global::System.InvalidOperationException", output, StringComparison.Ordinal);
        Assert.Contains("Decorators must be registered after the base service", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Separate_Base_Services_And_Decorators()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IServiceA { }
                              public interface IServiceB { }

                              [Registration(As = typeof(IServiceA))]
                              public class ServiceA : IServiceA { }

                              [Registration(As = typeof(IServiceB))]
                              public class ServiceB : IServiceB { }

                              [Registration(As = typeof(IServiceA), Decorator = true)]
                              public class DecoratorA : IServiceA
                              {
                                  public DecoratorA(IServiceA inner) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Find positions in the output
        var serviceAIndex = output.IndexOf("services.AddSingleton<TestNamespace.IServiceA, TestNamespace.ServiceA>()", StringComparison.Ordinal);
        var serviceBIndex = output.IndexOf("services.AddSingleton<TestNamespace.IServiceB, TestNamespace.ServiceB>()", StringComparison.Ordinal);
        var decoratorAIndex = output.IndexOf("services.Decorate<TestNamespace.IServiceA>", StringComparison.Ordinal);

        // Verify base services are registered before decorators
        Assert.True(serviceAIndex > 0, "ServiceA should be registered");
        Assert.True(serviceBIndex > 0, "ServiceB should be registered");
        Assert.True(decoratorAIndex > 0, "DecoratorA should be registered");
        Assert.True(serviceAIndex < decoratorAIndex, "Base service should be registered before decorator");
        Assert.True(serviceBIndex < decoratorAIndex, "Other base services should be registered before decorators");
    }
}