namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    private const string ServiceDescriptorType = "global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor";

    [Fact]
    public void Generator_Should_Generate_Replace_Registration()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache
                              {
                              }

                              [Registration(Lifetime.Scoped, As = typeof(ICache), Replace = true)]
                              public class RedisCache : ICache
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains($"services.Replace({ServiceDescriptorType}.Scoped<TestNamespace.ICache, TestNamespace.RedisCache>());", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Replace_Registration_As_Self()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration(Lifetime.Transient, Replace = true)]
                              public class Clock
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains($"services.Replace({ServiceDescriptorType}.Transient<TestNamespace.Clock, TestNamespace.Clock>());", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Replace_Registration_With_Factory()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailSender
                              {
                              }

                              [Registration(As = typeof(IEmailSender), Factory = nameof(Create), Replace = true)]
                              public class EmailSender : IEmailSender
                              {
                                  public static IEmailSender Create(IServiceProvider serviceProvider) => new EmailSender();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains($"services.Replace({ServiceDescriptorType}.Singleton<TestNamespace.IEmailSender>(sp => TestNamespace.EmailSender.Create(sp)));", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Replace_Registration_With_Instance()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ISettings
                              {
                              }

                              [Registration(As = typeof(ISettings), Instance = nameof(Default), Replace = true)]
                              public class Settings : ISettings
                              {
                                  public static readonly Settings Default = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains($"services.Replace({ServiceDescriptorType}.Singleton<TestNamespace.ISettings>(TestNamespace.Settings.Default));", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Replace_Registration_For_Open_Generic()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T>
                              {
                              }

                              [Registration(Lifetime.Scoped, Replace = true)]
                              public class Repository<T> : IRepository<T>
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains($"services.Replace({ServiceDescriptorType}.Describe(typeof(TestNamespace.IRepository<>), typeof(TestNamespace.Repository<>), global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped));", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("TryAdd = true", "TryAdd")]
    [InlineData("Key = \"Redis\"", "Key")]
    [InlineData("Decorator = true", "Decorator")]
    public void Generator_Should_Report_Replace_Combined_With_Unsupported_Option(
        string option,
        string optionName)
    {
        var source = $$"""
                       using Atc.DependencyInjection;

                       namespace TestNamespace;

                       public interface ICache
                       {
                       }

                       [Registration(As = typeof(ICache), Replace = true, {{option}})]
                       public class RedisCache : ICache
                       {
                       }
                       """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCDIR011");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains(optionName, diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}