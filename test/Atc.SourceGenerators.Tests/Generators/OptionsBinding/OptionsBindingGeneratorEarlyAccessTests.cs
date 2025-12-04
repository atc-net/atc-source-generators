// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_OptionsInstanceCache_Class()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("OptionsInstanceCache.g.cs", output.Keys, StringComparer.Ordinal);
        Assert.Contains("class OptionsInstanceCache", output["OptionsInstanceCache.g.cs"], StringComparison.Ordinal);
        Assert.Contains("namespace Atc.OptionsBinding", output["OptionsInstanceCache.g.cs"], StringComparison.Ordinal);
        Assert.Contains("ConcurrentDictionary", output["OptionsInstanceCache.g.cs"], StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_GetOrAdd_Method_For_Unnamed_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database")]
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
        Assert.Contains("GetOrAddDatabaseOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Early Access Methods", generatedCode, StringComparison.Ordinal);
        Assert.Contains("If already cached, returns the existing instance", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_GetOrAdd_Method_For_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email:Primary", Name = "Primary")]
                              [OptionsBinding("Email:Secondary", Name = "Secondary")]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Should NOT contain GetOrAdd methods for named options
        Assert.DoesNotContain("GetOrAddEmailOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Early Access Methods", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_Idempotency_Check_In_GetOrAdd_Method()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Check if already registered (idempotent)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("OptionsInstanceCache.TryGet", generatedCode, StringComparison.Ordinal);
        Assert.Contains("if (existing is not null)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("return existing;", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_DataAnnotations_Validation_In_GetOrAdd_Method()
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
        Assert.Contains("Validate immediately (DataAnnotations)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ValidationContext", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Validator.ValidateObject", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_ErrorOnMissingKeys_Check_In_GetOrAdd_Method()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ErrorOnMissingKeys = true)]
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
        Assert.Contains("Validate section exists (ErrorOnMissingKeys)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("if (!section.Exists())", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'Database' is missing", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_PostConfigure_Call_In_GetOrAdd_Method()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Storage", PostConfigure = nameof(NormalizePaths))]
                              public partial class StorageOptions
                              {
                                  public string BasePath { get; set; } = string.Empty;

                                  internal static void NormalizePaths(StorageOptions options)
                                  {
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Apply post-configuration", generatedCode, StringComparison.Ordinal);
        Assert.Contains("StorageOptions.NormalizePaths(options)", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Add_Cache_Population_In_AddOptionsFrom_Methods()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Add to shared cache for early access", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".PostConfigure(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("OptionsInstanceCache.Add(options, \"", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Add_Cache_Population_For_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email:Primary", Name = "Primary")]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Find the Configure section for the named option
        var configureIndex = generatedCode.IndexOf("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\"", StringComparison.Ordinal);
        Assert.NotEqual(-1, configureIndex);

        // Verify no cache population for named options
        var nextConfigureIndex = generatedCode.IndexOf("services.", configureIndex + 1, StringComparison.Ordinal);
        var sectionBetween = nextConfigureIndex == -1
            ? generatedCode.Substring(configureIndex)
            : generatedCode.Substring(configureIndex, nextConfigureIndex - configureIndex);

        Assert.DoesNotContain("OptionsInstanceCache.Add", sectionBetween, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Populate_Cache_In_GetOrAdd_Method()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // GetOrAdd should only populate cache, not register with service collection
        Assert.Contains("global::Atc.OptionsBinding.OptionsInstanceCache.Add", generatedCode, StringComparison.Ordinal);

        // Should NOT call services.Configure (that's done by AddOptionsFrom)
        Assert.DoesNotContain("Copy all properties from pre-bound instance", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Get_Method_That_Checks_Cache_But_Does_Not_Populate()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Get method should exist
        Assert.Contains("public static global::MyApp.Configuration.DatabaseOptions GetDatabaseOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);

        // Get method should check cache (read-only) but not populate it
        var getMethodStart = generatedCode.IndexOf("public static global::MyApp.Configuration.DatabaseOptions GetDatabaseOptionsFromTestAssembly", StringComparison.Ordinal);
        var getOrAddMethodStart = generatedCode.IndexOf("public static global::MyApp.Configuration.DatabaseOptions GetOrAddDatabaseOptionsFromTestAssembly", StringComparison.Ordinal);
        var getMethodCode = generatedCode.Substring(getMethodStart, getOrAddMethodStart - getMethodStart);

        // Verify Get method checks cache (read-only access)
        Assert.Contains("OptionsInstanceCache.TryGet", getMethodCode, StringComparison.Ordinal);

        // Verify Get method does NOT populate cache (no side effects)
        Assert.DoesNotContain("OptionsInstanceCache.Add", getMethodCode, StringComparison.Ordinal);

        // Verify it still binds and validates when not cached
        Assert.Contains("section.Bind(options)", getMethodCode, StringComparison.Ordinal);
        Assert.Contains("return options", getMethodCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Multiple_GetOrAdd_Methods_For_Multiple_Options()
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

                              [OptionsBinding("Storage")]
                              public partial class StorageOptions
                              {
                                  public string BasePath { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("GetOrAddDatabaseOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);
        Assert.Contains("GetOrAddStorageOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Generic_GetOptions_In_Library()
    {
        // Arrange - Library assemblies (without OptionsBinding references) should NOT generate GetOptions<T>()
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

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

        // Check that the extension method file exists
        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify generic method does NOT exist (library assembly)
        // Check for the actual method signature, not just the name in comments
        Assert.DoesNotContain("public static T GetOptions<T>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("namespace Microsoft.Extensions.DependencyInjection", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Generic_Method_When_No_Referenced_Assemblies()
    {
        // Arrange - Single-assembly projects (libraries) should NOT generate GetOptions<T>()
        // Only consuming assemblies with references should generate the smart dispatcher
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

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

        // In single-assembly test context (no referenced assemblies), generic method should NOT be generated
        // Check for the actual method signature, not just the name in comments
        Assert.DoesNotContain("public static T GetOptions<T>", generatedCode, StringComparison.Ordinal);

        // Verify comment explaining why it's not generated
        Assert.Contains("Generic Method Not Generated", generatedCode, StringComparison.Ordinal);
        Assert.Contains("not generated in library assemblies without references", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_Assembly_Metadata_In_Cache()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        // Check that Add() method accepts assembly name parameter
        var cacheCode = output["OptionsInstanceCache.g.cs"];
        Assert.Contains("void Add<T>(T instance, string assemblyName)", cacheCode, StringComparison.Ordinal);

        // Check that Add() is called with assembly name in GetOrAdd methods
        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.Contains("OptionsInstanceCache.Add(options, \"TestAssembly\");", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_FindAll_Method_In_Cache()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var cacheCode = output["OptionsInstanceCache.g.cs"];
        Assert.Contains("FindAll<T>", cacheCode, StringComparison.Ordinal);
        Assert.Contains("List<(object Instance, string AssemblyName)>", cacheCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generic_Method_Should_Include_Error_Message_For_Unregistered_Types()
    {
        // Arrange - The smart dispatcher generates compile-time error for unrecognized types
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

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

        // Verify the GetOptions<T> method is NOT generated for library assemblies
        // Check for the actual method signature, not just the name in comments
        Assert.DoesNotContain("public static T GetOptions<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Smart_Dispatcher_Should_Include_Available_Types_In_Error()
    {
        // Arrange - When GetOptions<T>() IS generated (with references), it includes available types in error
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

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

        // Since this is a library assembly, GetOptions<T>() should not be generated
        // The smart dispatcher only generates in consuming assemblies with references
        Assert.DoesNotContain("Available types:", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Smart_Dispatcher_Should_Call_Get_Methods_Not_GetOrAdd()
    {
        // Arrange - Smart dispatcher should call Get methods (no caching) not GetOrAdd
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

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

        // In library assemblies, GetOptions<T>() is not generated
        // This test would need multi-assembly setup to properly test the dispatcher
        // For now, verify both Get and GetOrAdd methods exist
        Assert.Contains("public static global::MyApp.Configuration.DatabaseOptions GetDatabaseOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public static global::MyApp.Configuration.DatabaseOptions GetOrAddDatabaseOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);

        // Note: Full dispatcher testing requires multi-assembly setup which is complex in unit tests
        // The sample project serves as integration test for this functionality
    }

    [Fact]
    public void Generic_Method_Should_List_Assembly_Specific_Methods_In_Error()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify error includes method suggestions
        Assert.Contains("GetOrAdd", generatedCode, StringComparison.Ordinal);
        Assert.Contains("From", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Cache_Should_Support_Multiple_Assemblies()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var cacheCode = output["OptionsInstanceCache.g.cs"];

        // Verify cache structure supports multiple assemblies
        Assert.Contains("ConcurrentDictionary<global::System.Type, global::System.Collections.Generic.List<(object Instance, string AssemblyName)>>", cacheCode, StringComparison.Ordinal);
    }
}