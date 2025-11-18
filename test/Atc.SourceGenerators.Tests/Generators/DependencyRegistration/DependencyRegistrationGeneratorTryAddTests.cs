// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_TryAdd_Registration_For_Singleton()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ILogger
                              {
                                  void Log(string message);
                              }

                              [Registration(As = typeof(ILogger), TryAdd = true)]
                              public class DefaultLogger : ILogger
                              {
                                  public void Log(string message) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddSingleton<TestNamespace.ILogger, TestNamespace.DefaultLogger>();", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_TryAdd_Registration_For_Scoped()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService
                              {
                                  void CreateUser(string name);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IUserService), TryAdd = true)]
                              public class DefaultUserService : IUserService
                              {
                                  public void CreateUser(string name) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddScoped<TestNamespace.IUserService, TestNamespace.DefaultUserService>();", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_TryAdd_Registration_For_Transient()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailService
                              {
                                  void Send(string to, string message);
                              }

                              [Registration(Lifetime.Transient, As = typeof(IEmailService), TryAdd = true)]
                              public class DefaultEmailService : IEmailService
                              {
                                  public void Send(string to, string message) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddTransient<TestNamespace.IEmailService, TestNamespace.DefaultEmailService>();", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_TryAdd_Registration_With_Factory()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache
                              {
                                  object Get(string key);
                              }

                              [Registration(Lifetime.Singleton, As = typeof(ICache), TryAdd = true, Factory = nameof(CreateCache))]
                              public class DefaultCache : ICache
                              {
                                  public object Get(string key) => null;

                                  public static ICache CreateCache(IServiceProvider sp)
                                  {
                                      return new DefaultCache();
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddSingleton<TestNamespace.ICache>(sp => TestNamespace.DefaultCache.CreateCache(sp));", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_TryAdd_Registration_With_Generic_Types()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T> where T : class
                              {
                                  T? GetById(int id);
                              }

                              [Registration(Lifetime.Scoped, TryAdd = true)]
                              public class DefaultRepository<T> : IRepository<T> where T : class
                              {
                                  public T? GetById(int id) => default;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddScoped(typeof(TestNamespace.IRepository<>), typeof(TestNamespace.DefaultRepository<>));", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_TryAdd_Registration_With_Multiple_Interfaces()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService1
                              {
                                  void Method1();
                              }

                              public interface IService2
                              {
                                  void Method2();
                              }

                              [Registration(Lifetime.Scoped, TryAdd = true)]
                              public class DefaultService : IService1, IService2
                              {
                                  public void Method1() { }
                                  public void Method2() { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddScoped<TestNamespace.IService1, TestNamespace.DefaultService>();", output, StringComparison.Ordinal);
        Assert.Contains("services.TryAddScoped<TestNamespace.IService2, TestNamespace.DefaultService>();", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_TryAdd_Registration_With_AsSelf()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ILogger
                              {
                                  void Log(string message);
                              }

                              [Registration(As = typeof(ILogger), AsSelf = true, TryAdd = true)]
                              public class DefaultLogger : ILogger
                              {
                                  public void Log(string message) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddSingleton<TestNamespace.ILogger, TestNamespace.DefaultLogger>();", output, StringComparison.Ordinal);
        Assert.Contains("services.TryAddSingleton<TestNamespace.DefaultLogger>();", output, StringComparison.Ordinal);
    }
}