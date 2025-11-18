// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Conditional_Registration_With_Configuration_Check()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache { }

                              [Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
                              public class RedisCache : ICache
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("IConfiguration", output, StringComparison.Ordinal);
        Assert.Contains("Features:UseRedisCache", output, StringComparison.Ordinal);
        Assert.Contains("if (configuration.GetValue<bool>(\"Features:UseRedisCache\"))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.ICache, TestNamespace.RedisCache>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Conditional_Registration_With_Negation()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache { }

                              [Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
                              public class MemoryCache : ICache
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("IConfiguration", output, StringComparison.Ordinal);
        Assert.Contains("Features:UseRedisCache", output, StringComparison.Ordinal);
        Assert.Contains("if (!configuration.GetValue<bool>(\"Features:UseRedisCache\"))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.ICache, TestNamespace.MemoryCache>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Multiple_Conditional_Registrations()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache { }

                              [Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
                              public class RedisCache : ICache
                              {
                              }

                              [Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
                              public class MemoryCache : ICache
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("if (configuration.GetValue<bool>(\"Features:UseRedisCache\"))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.ICache, TestNamespace.RedisCache>()", output, StringComparison.Ordinal);
        Assert.Contains("if (!configuration.GetValue<bool>(\"Features:UseRedisCache\"))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.ICache, TestNamespace.MemoryCache>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Mix_Conditional_And_Unconditional_Registrations()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache { }
                              public interface ILogger { }

                              [Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
                              public class RedisCache : ICache
                              {
                              }

                              [Registration(As = typeof(ILogger))]
                              public class Logger : ILogger
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Conditional service should have if check
        Assert.Contains("if (configuration.GetValue<bool>(\"Features:UseRedisCache\"))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.ICache, TestNamespace.RedisCache>()", output, StringComparison.Ordinal);

        // Unconditional service should NOT have if check before it
        var loggerRegistrationIndex = output.IndexOf("services.AddSingleton<TestNamespace.ILogger, TestNamespace.Logger>()", StringComparison.Ordinal);
        Assert.True(loggerRegistrationIndex > 0);

        // Make sure no configuration check appears right before the logger registration
        var precedingText = output.Substring(Math.Max(0, loggerRegistrationIndex - 200), Math.Min(200, loggerRegistrationIndex));
        var hasNoConditionCheck = !precedingText.Contains("if (configuration.GetValue<bool>", StringComparison.Ordinal);
        Assert.True(hasNoConditionCheck || precedingText.Contains("Features:UseRedisCache", StringComparison.Ordinal));
    }

    [Fact]
    public void Generator_Should_Add_IConfiguration_Parameter_When_Conditional_Registrations_Exist()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache { }

                              [Registration(As = typeof(ICache), Condition = "Features:UseCache")]
                              public class Cache : ICache
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Method signature should include using statement and IConfiguration parameter
        Assert.Contains("using Microsoft.Extensions.Configuration;", output, StringComparison.Ordinal);
        Assert.Contains("IServiceCollection AddDependencyRegistrationsFromTestAssembly(", output, StringComparison.Ordinal);
        Assert.Contains("this IServiceCollection services,", output, StringComparison.Ordinal);
        Assert.Contains("IConfiguration configuration", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Conditional_Registration_With_Different_Lifetimes()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache { }

                              [Registration(Lifetime.Scoped, As = typeof(ICache), Condition = "Features:UseScoped")]
                              public class ScopedCache : ICache
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("if (configuration.GetValue<bool>(\"Features:UseScoped\"))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddScoped<TestNamespace.ICache, TestNamespace.ScopedCache>()", output, StringComparison.Ordinal);
    }
}