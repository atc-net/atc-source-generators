// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators;

public class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Attribute_Definition()
    {
        // Arrange
        const string source = """
                              namespace TestNamespace;

                              public class TestClass
                              {
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("OptionsBindingAttribute.g.cs", output.Keys, StringComparer.Ordinal);
        Assert.Contains("class OptionsBindingAttribute", output["OptionsBindingAttribute.g.cs"], StringComparison.Ordinal);
        Assert.Contains("enum OptionsLifetime", output["OptionsBindingAttribute.g.cs"], StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Extension_Method_With_Inferred_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                                  public int MaxRetries { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("AddOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);
        Assert.Contains("AddOptions<global::MyApp.Configuration.DatabaseOptions>()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"DatabaseOptions\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Extension_Method_With_Explicit_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("App:Database:Settings")]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"App:Database:Settings\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Add_ValidateDataAnnotations_When_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateDataAnnotations = true)]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Add_ValidateOnStart_When_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateOnStart = true)]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Add_Both_Validation_Methods_When_Both_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Class_Is_Not_Partial()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database")]
                              public class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT001", diagnostic.Id);
        Assert.Contains("must be declared as partial", diagnostic.GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Infer_Section_Name_From_Class_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"ApiOptions\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Full_Class_Name_For_Inference()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class LoggingSettings
                              {
                                  public string Level { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"LoggingSettings\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Multiple_Options_Classes()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database")]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }

                              [OptionsBinding("Api")]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;
                              }

                              [OptionsBinding("Logging")]
                              public partial class LoggingOptions
                              {
                                  public string Level { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure DatabaseOptions", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configure ApiOptions", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configure LoggingOptions", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Database\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Api\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Logging\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_When_No_OptionsBinding_Attribute()
    {
        // Arrange
        const string source = """
                              namespace MyApp.Configuration;

                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.Null(generatedCode);
    }

    [Fact]
    public void Generator_Should_Use_Atc_DependencyInjection_Namespace_For_Extension_Method()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyCompany.MyApp.Configuration;

                              [OptionsBinding("Database")]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("namespace Atc.DependencyInjection", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_SectionName_For_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string SectionName = "CustomDatabaseSection";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"CustomDatabaseSection\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_Name_For_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string Name = "MyDatabaseConfig";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"MyDatabaseConfig\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_NameTitle_For_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public const string NameTitle = "CustomApiSection";
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"CustomApiSection\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_SectionName_Over_NameTitle()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string SectionName = "X1";
                                  public const string NameTitle = "X2";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"X1\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X2\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_SectionName_Over_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public const string SectionName = "X1";
                                  public const string Name = "X3";
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"X1\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X3\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_NameTitle_Over_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class LoggingOptions
                              {
                                  public const string NameTitle = "AppLogging";
                                  public const string Name = "Logging";
                                  public string Level { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"AppLogging\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"Logging\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Full_Priority_Order()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class LoggingOptions
                              {
                                  public const string SectionName = "X1";
                                  public const string NameTitle = "X2";
                                  public const string Name = "X3";
                                  public string Level { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"X1\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X2\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X3\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_Explicit_SectionName_Over_Const_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("ExplicitSection")]
                              public partial class CacheOptions
                              {
                                  public const string Name = "Cache";
                                  public int Size { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"ExplicitSection\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"Cache\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_Const_Name_Over_Auto_Inference()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class EmailOptions
                              {
                                  public const string Name = "EmailConfig";
                                  public string SmtpServer { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"EmailConfig\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"EmailOptions\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Const_SectionName_Is_Empty()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class CacheOptions
                              {
                                  public const string SectionName = "";
                                  public int Size { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT003", diagnostic.Id);
        Assert.Contains("CacheOptions", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("SectionName", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Const_Name_Is_Empty()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string Name = "";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT003", diagnostic.Id);
        Assert.Contains("DatabaseOptions", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("Name", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Const_NameTitle_Is_Empty()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public const string NameTitle = "";
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT003", diagnostic.Id);
        Assert.Contains("ApiOptions", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("NameTitle", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Named_Parameters_Without_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding(ValidateDataAnnotations = true)]
                              public partial class DatabaseOptions
                              {
                                  public const string Name = "MyDatabase";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"MyDatabase\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Comment_For_Singleton_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", Lifetime = OptionsLifetime.Singleton)]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure DatabaseOptions - Inject using IOptions<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Comment_For_Scoped_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Request", Lifetime = OptionsLifetime.Scoped)]
                              public partial class RequestOptions
                              {
                                  public string ClientId { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure RequestOptions - Inject using IOptionsSnapshot<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Comment_For_Monitor_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor)]
                              public partial class FeatureOptions
                              {
                                  public bool EnableNewFeature { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure FeatureOptions - Inject using IOptionsMonitor<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Default_To_Singleton_When_Lifetime_Not_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Cache")]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure CacheOptions - Inject using IOptions<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_All_Four_Overloads_For_Transitive_Registration()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Options;

                              [OptionsBinding("App")]
                              public partial class AppOptions
                              {
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Check that all 4 overloads exist
        var overload1Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration\s*\)", RegexOptions.Multiline).Count;
        var overload2Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration,\s*bool includeReferencedAssemblies\s*\)", RegexOptions.Multiline).Count;
        var overload3Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration,\s*string referencedAssemblyName\s*\)", RegexOptions.Multiline).Count;
        var overload4Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration,\s*params string\[\] referencedAssemblyNames\s*\)", RegexOptions.Multiline).Count;

        Assert.Equal(1, overload1Count);
        Assert.Equal(1, overload2Count);
        Assert.Equal(1, overload3Count);
        Assert.Equal(1, overload4Count);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Empty_If_Statement_When_No_Referenced_Assemblies()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestApp.Options;

                              [OptionsBinding("TestSection")]
                              public partial class TestOptions
                              {
                                  public string Value { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify that the overload with includeReferencedAssemblies parameter exists
        Assert.Contains("bool includeReferencedAssemblies", generatedCode, StringComparison.Ordinal);

        // Verify that there is NO empty if-statement in the generated code
        // The pattern we're looking for is an if-statement with only whitespace between the braces
        var emptyIfPattern = new Regex(@"if\s*\(\s*includeReferencedAssemblies\s*\)\s*\{\s*\}", RegexOptions.Multiline);
        Assert.False(emptyIfPattern.IsMatch(generatedCode), "Generated code should not contain an empty if-statement when there are no referenced assemblies");
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, Dictionary<string, string> GeneratedSources) GetGeneratedOutput(
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new OptionsBindingGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);

        var runResult = driver.GetRunResult();

        var generatedSources = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var result in runResult.Results)
        {
            foreach (var generatedSource in result.GeneratedSources)
            {
                generatedSources[generatedSource.HintName] = generatedSource.SourceText.ToString();
            }
        }

        return (runResult.Diagnostics, generatedSources);
    }

    private static string? GetGeneratedExtensionMethod(
        Dictionary<string, string> output)
    {
        var extensionMethodFile = output.Keys.FirstOrDefault(k => k.StartsWith("OptionsBindingExtensions.", StringComparison.Ordinal));
        return extensionMethodFile != null ? output[extensionMethodFile] : null;
    }
}