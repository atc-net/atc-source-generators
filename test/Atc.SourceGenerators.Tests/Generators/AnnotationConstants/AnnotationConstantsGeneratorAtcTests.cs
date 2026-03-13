// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.AnnotationConstants;

public partial class AnnotationConstantsGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_IsIgnoreDisplay_For_IgnoreDisplayAttribute()
    {
        const string source = """
                              namespace Atc
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class IgnoreDisplayAttribute : System.Attribute { }
                              }

                              namespace TestNamespace
                              {
                                  public class Product
                                  {
                                      [Atc.IgnoreDisplay]
                                      public string InternalCode { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Product", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class InternalCode", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsIgnoreDisplay = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_EnumGuid_For_EnumGuidAttribute()
    {
        const string source = """
                              namespace Atc
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class EnumGuidAttribute : System.Attribute
                                  {
                                      public EnumGuidAttribute(string guid) { }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Product
                                  {
                                      [Atc.EnumGuid("d290f1ee-6c54-4b01-90e6-d701748f0851")]
                                      public int CategoryId { get; set; }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class CategoryId", output, StringComparison.Ordinal);
        Assert.Contains("public const string EnumGuid = \"d290f1ee-6c54-4b01-90e6-d701748f0851\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_CasingStyle_For_CasingStyleDescriptionAttribute()
    {
        const string source = """
                              namespace Atc
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class CasingStyleDescriptionAttribute : System.Attribute
                                  {
                                      public object Default { get; set; }
                                      public string Prefix { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Product
                                  {
                                      [Atc.CasingStyleDescription(Default = "PascalCase", Prefix = "pre_")]
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Name", output, StringComparison.Ordinal);
        Assert.Contains("public const string CasingStyleDefault = \"PascalCase\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string CasingStylePrefix = \"pre_\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsIPAddress_For_IPAddressAttribute()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class IPAddressAttribute : System.Attribute
                                  {
                                      public IPAddressAttribute() { }
                                      public IPAddressAttribute(bool required) { Required = required; }
                                      public bool Required { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Server
                                  {
                                      [System.ComponentModel.DataAnnotations.IPAddress(true)]
                                      public string HostAddress { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Server", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class HostAddress", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsIPAddress = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IPAddressRequired = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsIPAddress_Without_Required()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class IPAddressAttribute : System.Attribute
                                  {
                                      public IPAddressAttribute() { }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Server
                                  {
                                      [System.ComponentModel.DataAnnotations.IPAddress]
                                      public string HostAddress { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsIPAddress = true;", output, StringComparison.Ordinal);
        Assert.DoesNotContain("IPAddressRequired", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsAtcUri_For_UriAttribute()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class UriAttribute : System.Attribute
                                  {
                                      public UriAttribute() { }
                                      public UriAttribute(bool required, bool allowHttp, bool allowHttps, bool allowFtp, bool allowFtps, bool allowFile, bool allowOpcTcp)
                                      {
                                          Required = required;
                                          AllowHttp = allowHttp;
                                          AllowHttps = allowHttps;
                                          AllowFtp = allowFtp;
                                          AllowFtps = allowFtps;
                                          AllowFile = allowFile;
                                          AllowOpcTcp = allowOpcTcp;
                                      }
                                      public bool Required { get; set; }
                                      public bool AllowHttp { get; set; }
                                      public bool AllowHttps { get; set; }
                                      public bool AllowFtp { get; set; }
                                      public bool AllowFtps { get; set; }
                                      public bool AllowFile { get; set; }
                                      public bool AllowOpcTcp { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class ServiceConfig
                                  {
                                      [System.ComponentModel.DataAnnotations.Uri(true, true, true, false, false, false, false)]
                                      public string Endpoint { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class ServiceConfig", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Endpoint", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsAtcUri = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriAllowHttp = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriAllowHttps = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriAllowFtp = false;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriAllowFtps = false;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriAllowFile = false;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriAllowOpcTcp = false;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsAtcString_For_StringAttribute()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class StringAttribute : System.Attribute
                                  {
                                      public StringAttribute() { }
                                      public StringAttribute(bool required, uint minLength, uint maxLength)
                                      {
                                          Required = required;
                                          MinLength = minLength;
                                          MaxLength = maxLength;
                                      }
                                      public bool Required { get; set; }
                                      public uint MinLength { get; set; }
                                      public uint MaxLength { get; set; }
                                      public string RegularExpression { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Customer
                                  {
                                      [System.ComponentModel.DataAnnotations.String(true, 2, 100)]
                                      public string FullName { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Customer", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class FullName", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsAtcString = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcStringRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const uint AtcStringMinLength = 2;", output, StringComparison.Ordinal);
        Assert.Contains("public const uint AtcStringMaxLength = 100;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsAtcString_With_RegularExpression()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class StringAttribute : System.Attribute
                                  {
                                      public StringAttribute() { }
                                      public StringAttribute(bool required, uint minLength, uint maxLength, string regularExpression)
                                      {
                                          Required = required;
                                          MinLength = minLength;
                                          MaxLength = maxLength;
                                          RegularExpression = regularExpression;
                                      }
                                      public bool Required { get; set; }
                                      public uint MinLength { get; set; }
                                      public uint MaxLength { get; set; }
                                      public string RegularExpression { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Customer
                                  {
                                      [System.ComponentModel.DataAnnotations.String(true, 1, 50, @"^[a-zA-Z]+$")]
                                      public string Code { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsAtcString = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcStringRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const uint AtcStringMinLength = 1;", output, StringComparison.Ordinal);
        Assert.Contains("public const uint AtcStringMaxLength = 50;", output, StringComparison.Ordinal);
        Assert.Contains("AtcStringRegularExpression", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsKeyString_For_KeyStringAttribute()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class KeyStringAttribute : System.Attribute
                                  {
                                      public KeyStringAttribute() { }
                                      public KeyStringAttribute(bool required, uint minLength, uint maxLength)
                                      {
                                          Required = required;
                                          MinLength = minLength;
                                          MaxLength = maxLength;
                                      }
                                      public bool Required { get; set; }
                                      public uint MinLength { get; set; }
                                      public uint MaxLength { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Order
                                  {
                                      [System.ComponentModel.DataAnnotations.KeyString(true, 5, 20)]
                                      public string OrderKey { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Order", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class OrderKey", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsAtcString = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsKeyString = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcStringRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const uint AtcStringMinLength = 5;", output, StringComparison.Ordinal);
        Assert.Contains("public const uint AtcStringMaxLength = 20;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsIsoCurrencySymbol_For_IsoCurrencySymbolAttribute()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class IsoCurrencySymbolAttribute : System.Attribute
                                  {
                                      public IsoCurrencySymbolAttribute() { }
                                      public bool Required { get; set; }
                                      public string[] IsoCurrencySymbols { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Payment
                                  {
                                      [System.ComponentModel.DataAnnotations.IsoCurrencySymbol(Required = true)]
                                      public string CurrencyCode { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Payment", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class CurrencyCode", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsIsoCurrencySymbol = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsoCurrencySymbolRequired = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Atc_Validation_Attributes_From_Non_Atc_Assembly()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class IPAddressAttribute : System.Attribute
                                  {
                                      public IPAddressAttribute() { }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Server
                                  {
                                      [System.ComponentModel.DataAnnotations.IPAddress]
                                      public string HostAddress { get; set; } = string.Empty;
                                  }
                              }
                              """;

        // Use default GetGeneratedOutput which has assembly name "TestAssembly" (not starting with "Atc")
        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("IsIPAddress", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Multiple_Atc_Attributes_On_Same_Property()
    {
        const string source = """
                              namespace Atc
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class IgnoreDisplayAttribute : System.Attribute { }

                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class EnumGuidAttribute : System.Attribute
                                  {
                                      public EnumGuidAttribute(string guid) { }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class Product
                                  {
                                      [Atc.IgnoreDisplay]
                                      [Atc.EnumGuid("aaaabbbb-cccc-dddd-eeee-ffffffffffff")]
                                      public int Status { get; set; }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsIgnoreDisplay = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const string EnumGuid = \"aaaabbbb-cccc-dddd-eeee-ffffffffffff\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IsAtcUri_With_Named_Args()
    {
        const string source = """
                              namespace System.ComponentModel.DataAnnotations
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Property)]
                                  public class UriAttribute : System.Attribute
                                  {
                                      public UriAttribute() { }
                                      public bool Required { get; set; }
                                      public bool AllowHttp { get; set; }
                                      public bool AllowHttps { get; set; }
                                      public bool AllowFtp { get; set; }
                                      public bool AllowFtps { get; set; }
                                      public bool AllowFile { get; set; }
                                      public bool AllowOpcTcp { get; set; }
                                  }
                              }

                              namespace TestNamespace
                              {
                                  public class ServiceConfig
                                  {
                                      [System.ComponentModel.DataAnnotations.Uri(Required = true, AllowHttps = true)]
                                      public string Endpoint { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutputWithAtcAssemblyName(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsAtcUri = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AtcUriAllowHttps = true;", output, StringComparison.Ordinal);
    }

    /// <summary>
    /// Helper that creates a compilation with assembly name starting with "Atc"
    /// so the generator recognizes Atc validation attributes (IPAddress, Uri, String, etc.)
    /// that live in System.ComponentModel.DataAnnotations namespace but require an Atc assembly.
    /// </summary>
    [SuppressMessage("", "S1854:Remove this useless assignment to local variable 'driver'", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutputWithAtcAssemblyName(
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        var dataAnnotationsAssembly = typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly;
        if (!string.IsNullOrWhiteSpace(dataAnnotationsAssembly.Location))
        {
            references.Add(MetadataReference.CreateFromFile(dataAnnotationsAssembly.Location));
        }

        var compilation = CSharpCompilation.Create(
            "AtcTestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AnnotationConstantsGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = outputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCANN", StringComparison.Ordinal))
            .ToImmutableArray();

        var output = string.Join(
            Constants.LineFeed,
            outputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, output);
    }
}