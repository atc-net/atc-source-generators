// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Instance_Registration_With_Field()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IConfiguration
                              {
                                  string GetSetting(string key);
                              }

                              [Registration(As = typeof(IConfiguration), Instance = nameof(DefaultInstance))]
                              public class AppConfiguration : IConfiguration
                              {
                                  public static readonly AppConfiguration DefaultInstance = new AppConfiguration();

                                  private AppConfiguration() { }

                                  public string GetSetting(string key) => "default";
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IConfiguration>(TestNamespace.AppConfiguration.DefaultInstance);", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Instance_Registration_With_Property()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ISettings
                              {
                                  int MaxRetries { get; }
                              }

                              [Registration(As = typeof(ISettings), Instance = nameof(Default))]
                              public class AppSettings : ISettings
                              {
                                  public static AppSettings Default { get; } = new AppSettings();

                                  private AppSettings() { }

                                  public int MaxRetries => 3;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.ISettings>(TestNamespace.AppSettings.Default);", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Instance_Registration_With_Method()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICache
                              {
                                  void Set(string key, string value);
                              }

                              [Registration(As = typeof(ICache), Instance = nameof(GetInstance))]
                              public class MemoryCache : ICache
                              {
                                  private static readonly MemoryCache _instance = new MemoryCache();

                                  private MemoryCache() { }

                                  public static MemoryCache GetInstance() => _instance;

                                  public void Set(string key, string value) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.ICache>(TestNamespace.MemoryCache.GetInstance());", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Instance_Member_Not_Found()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration(As = typeof(IService), Instance = "NonExistentMember")]
                              public class Service : IService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR007", diagnostics[0].Id);
        Assert.Contains("Instance member 'NonExistentMember' not found", diagnostics[0].GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Instance_Member_Not_Static()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration(As = typeof(IService), Instance = nameof(InstanceField))]
                              public class Service : IService
                              {
                                  public readonly Service InstanceField = new Service();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR008", diagnostics[0].Id);
        Assert.Contains("Instance member 'InstanceField' must be static", diagnostics[0].GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Instance_And_Factory_Both_Specified()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration(As = typeof(IService), Instance = nameof(DefaultInstance), Factory = nameof(Create))]
                              public class Service : IService
                              {
                                  public static readonly Service DefaultInstance = new Service();

                                  public static IService Create(IServiceProvider sp) => new Service();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR009", diagnostics[0].Id);
        Assert.Contains("Cannot use both Instance and Factory", diagnostics[0].GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Instance_With_Scoped_Lifetime()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration(Lifetime.Scoped, As = typeof(IService), Instance = nameof(DefaultInstance))]
                              public class Service : IService
                              {
                                  public static readonly Service DefaultInstance = new Service();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR010", diagnostics[0].Id);
        Assert.Contains("Instance registration can only be used with Singleton lifetime", diagnostics[0].GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Instance_With_Transient_Lifetime()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration(Lifetime.Transient, As = typeof(IService), Instance = nameof(DefaultInstance))]
                              public class Service : IService
                              {
                                  public static readonly Service DefaultInstance = new Service();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR010", diagnostics[0].Id);
        Assert.Contains("Instance registration can only be used with Singleton lifetime", diagnostics[0].GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Instance_Registration_With_TryAdd()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ILogger { }

                              [Registration(As = typeof(ILogger), Instance = nameof(Default), TryAdd = true)]
                              public class DefaultLogger : ILogger
                              {
                                  public static readonly DefaultLogger Default = new DefaultLogger();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.TryAddSingleton<TestNamespace.ILogger>(TestNamespace.DefaultLogger.Default);", output, StringComparison.Ordinal);
    }
}