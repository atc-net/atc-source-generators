// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
// ReSharper disable UnusedParameter.Local
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable MergeIntoPattern
namespace Atc.SourceGenerators.Generators;

/// <summary>
/// Source generator that extracts DataAnnotation attributes from class/record properties
/// and generates compile-time accessible constants. Supports both Microsoft DataAnnotations
/// and Atc-specific attributes when the Atc assembly is referenced.
/// </summary>
[SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "OK.")]
[Generator]
public class AnnotationConstantsGenerator : IIncrementalGenerator
{
    private const string ConfigKeyPrefix = "build_property.atc_annotation_constants.";
    private const string IncludeUnannotatedKey = ConfigKeyPrefix + "include_unannotated_properties";

    // Supported Microsoft DataAnnotation attribute full names
    private static readonly HashSet<string> SupportedMicrosoftAnnotations = new(StringComparer.Ordinal)
    {
        "System.ComponentModel.DataAnnotations.DisplayAttribute",
        "System.ComponentModel.DataAnnotations.RequiredAttribute",
        "System.ComponentModel.DataAnnotations.StringLengthAttribute",
        "System.ComponentModel.DataAnnotations.RangeAttribute",
        "System.ComponentModel.DataAnnotations.MinLengthAttribute",
        "System.ComponentModel.DataAnnotations.MaxLengthAttribute",
        "System.ComponentModel.DataAnnotations.RegularExpressionAttribute",
        "System.ComponentModel.DataAnnotations.EmailAddressAttribute",
        "System.ComponentModel.DataAnnotations.PhoneAttribute",
        "System.ComponentModel.DataAnnotations.UrlAttribute",
        "System.ComponentModel.DataAnnotations.CreditCardAttribute",
        "System.ComponentModel.DataAnnotations.DataTypeAttribute",
        "System.ComponentModel.DataAnnotations.CompareAttribute",
        "System.ComponentModel.DataAnnotations.KeyAttribute",
        "System.ComponentModel.DataAnnotations.EditableAttribute",
        "System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute",
        "System.ComponentModel.DataAnnotations.TimestampAttribute",
    };

    // Supported Atc attribute full names (from Atc namespace)
    private static readonly HashSet<string> SupportedAtcAnnotations = new(StringComparer.Ordinal)
    {
        "Atc.IgnoreDisplayAttribute",
        "Atc.EnumGuidAttribute",
        "Atc.CasingStyleDescriptionAttribute",
    };

    // Supported Atc validation attribute full names (from System.ComponentModel.DataAnnotations namespace, but in Atc assembly)
    private static readonly HashSet<string> SupportedAtcValidationAnnotations = new(StringComparer.Ordinal)
    {
        "System.ComponentModel.DataAnnotations.IPAddressAttribute",
        "System.ComponentModel.DataAnnotations.IsoCurrencySymbolAttribute",
        "System.ComponentModel.DataAnnotations.StringAttribute",
        "System.ComponentModel.DataAnnotations.KeyStringAttribute",
        "System.ComponentModel.DataAnnotations.UriAttribute",
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Create configuration pipeline from analyzer options
        var configPipeline = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) =>
            {
                var includeUnannotated = options.GlobalOptions.TryGetValue(
                    IncludeUnannotatedKey, out var value) &&
                    value.Equals("true", StringComparison.OrdinalIgnoreCase);

                return new AnnotationGeneratorConfig(includeUnannotated);
            });

        // Step 2: Find types with properties that have DataAnnotation attributes
        var typesWithAnnotations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsTypeWithPotentialAnnotations(node),
                transform: static (ctx, _) => GetTypeWithAnnotatedProperties(ctx))
            .Where(static info => info is not null)
            .Collect();

        // Step 3: Combine with compilation and configuration
        var combined = context.CompilationProvider
            .Combine(typesWithAnnotations)
            .Combine(configPipeline);

        // Step 4: Generate source output
        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var ((compilation, types), config) = source;
            Execute(compilation, types, config, spc);
        });
    }

    private static bool IsTypeWithPotentialAnnotations(SyntaxNode node)
    {
        // Fast check: is this a class or record?
        if (node is not TypeDeclarationSyntax typeDecl)
        {
            return false;
        }

        // Only process classes and records
        if (typeDecl is not (ClassDeclarationSyntax or RecordDeclarationSyntax))
        {
            return false;
        }

        // Check if any member is a property with attributes
        foreach (var member in typeDecl.Members)
        {
            if (member is PropertyDeclarationSyntax { AttributeLists.Count: > 0 })
            {
                return true;
            }
        }

        return false;
    }

    private static TypeAnnotationInfo? GetTypeWithAnnotatedProperties(
        GeneratorSyntaxContext context)
    {
        var typeDecl = (TypeDeclarationSyntax)context.Node;
        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDecl);

        if (typeSymbol is null)
        {
            return null;
        }

        var annotatedProperties = new List<PropertyAnnotationInfo>();

        // Process all properties
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            var propertyInfo = ExtractAllPropertyAnnotations(propertySymbol);
            if (propertyInfo is not null)
            {
                annotatedProperties.Add(propertyInfo);
            }
        }

        if (annotatedProperties.Count == 0)
        {
            return null;
        }

        return new TypeAnnotationInfo(
            typeSymbol.Name,
            typeSymbol.ContainingNamespace.ToDisplayString(),
            typeSymbol.ContainingAssembly.Name,
            [.. annotatedProperties]);
    }

    private static PropertyAnnotationInfo? ExtractAllPropertyAnnotations(
        IPropertySymbol propertySymbol)
    {
        var (display, msValidation, hasMsAnnotation) = ExtractPropertyMicrosoftAnnotations(propertySymbol);
        var (atcAnnotation, hasAtcAnnotation) = ExtractPropertyAtcAnnotations(propertySymbol);

        if (!hasMsAnnotation && !hasAtcAnnotation)
        {
            return null;
        }

        return new PropertyAnnotationInfo(
            propertySymbol.Name,
            display,
            msValidation,
            atcAnnotation);
    }

    [SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK - attribute extraction logic")]
    private static (DisplayAnnotationInfo? Display, ValidationAnnotationInfo? Validation, bool HasAny) ExtractPropertyMicrosoftAnnotations(
        IPropertySymbol propertySymbol)
    {
        DisplayAnnotationInfo? display = null;
        var validation = new ValidationAnnotationBuilder();
        var hasAnyAnnotation = false;

        foreach (var attribute in propertySymbol.GetAttributes())
        {
            var attrFullName = attribute.AttributeClass?.ToDisplayString();
            if (attrFullName is null || !SupportedMicrosoftAnnotations.Contains(attrFullName))
            {
                continue;
            }

            hasAnyAnnotation = true;

            // Extract based on attribute type
            switch (attrFullName)
            {
                case "System.ComponentModel.DataAnnotations.DisplayAttribute":
                    display = ExtractDisplayAttribute(attribute);
                    break;

                case "System.ComponentModel.DataAnnotations.RequiredAttribute":
                    validation.IsRequired = true;
                    validation.AllowEmptyStrings = GetNamedArg<bool?>(attribute, "AllowEmptyStrings");
                    validation.RequiredErrorMessage = GetNamedArg<string>(attribute, "ErrorMessage");
                    break;

                case "System.ComponentModel.DataAnnotations.StringLengthAttribute":
                    // First constructor argument is MaximumLength
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is int maxLen)
                    {
                        validation.MaxLength = maxLen;
                    }

                    validation.MinLength = GetNamedArg<int?>(attribute, "MinimumLength");
                    validation.StringLengthErrorMessage = GetNamedArg<string>(attribute, "ErrorMessage");
                    break;

                case "System.ComponentModel.DataAnnotations.RangeAttribute":
                    ExtractRangeAttribute(attribute, validation);
                    break;

                case "System.ComponentModel.DataAnnotations.MinLengthAttribute":
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is int minLenVal)
                    {
                        validation.MinLength = minLenVal;
                    }

                    break;

                case "System.ComponentModel.DataAnnotations.MaxLengthAttribute":
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is int maxLenVal)
                    {
                        validation.MaxLength = maxLenVal;
                    }

                    break;

                case "System.ComponentModel.DataAnnotations.RegularExpressionAttribute":
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is string pattern)
                    {
                        validation.RegularExpressionPattern = pattern;
                    }

                    validation.RegularExpressionErrorMessage = GetNamedArg<string>(attribute, "ErrorMessage");
                    break;

                case "System.ComponentModel.DataAnnotations.EmailAddressAttribute":
                    validation.IsEmailAddress = true;
                    break;

                case "System.ComponentModel.DataAnnotations.PhoneAttribute":
                    validation.IsPhone = true;
                    break;

                case "System.ComponentModel.DataAnnotations.UrlAttribute":
                    validation.IsUrl = true;
                    break;

                case "System.ComponentModel.DataAnnotations.CreditCardAttribute":
                    validation.IsCreditCard = true;
                    break;

                case "System.ComponentModel.DataAnnotations.DataTypeAttribute":
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is int dataType)
                    {
                        validation.DataType = dataType;
                    }

                    break;

                case "System.ComponentModel.DataAnnotations.CompareAttribute":
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is string otherProp)
                    {
                        validation.CompareOtherProperty = otherProp;
                    }

                    break;

                case "System.ComponentModel.DataAnnotations.KeyAttribute":
                    validation.IsKey = true;
                    break;

                case "System.ComponentModel.DataAnnotations.EditableAttribute":
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is bool editable)
                    {
                        validation.IsEditable = editable;
                    }

                    break;

                case "System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute":
                    if (attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is bool scaffold)
                    {
                        validation.IsScaffoldColumn = scaffold;
                    }

                    break;

                case "System.ComponentModel.DataAnnotations.TimestampAttribute":
                    validation.IsTimestamp = true;
                    break;
            }
        }

        return (display, validation.HasAnyValue ? validation.Build() : null, hasAnyAnnotation);
    }

    [SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK - attribute extraction logic")]
    private static (AtcAnnotationInfo? Annotation, bool HasAny) ExtractPropertyAtcAnnotations(
        IPropertySymbol propertySymbol)
    {
        var atc = new AtcAnnotationBuilder();
        var hasAnyAnnotation = false;

        foreach (var attribute in propertySymbol.GetAttributes())
        {
            var attrFullName = attribute.AttributeClass?.ToDisplayString();
            if (attrFullName is null)
            {
                continue;
            }

            // Check if it's an Atc namespace attribute
            if (SupportedAtcAnnotations.Contains(attrFullName))
            {
                hasAnyAnnotation = true;
                ExtractAtcNamespaceAttribute(attribute, attrFullName, atc);
                continue;
            }

            // Check if it's an Atc validation attribute (in System.ComponentModel.DataAnnotations namespace but from Atc assembly)
            if (SupportedAtcValidationAnnotations.Contains(attrFullName))
            {
                // Verify it's from the Atc assembly, not Microsoft's
                var assemblyName = attribute.AttributeClass?.ContainingAssembly?.Name;
                if (assemblyName is not null && assemblyName.StartsWith("Atc", StringComparison.Ordinal))
                {
                    hasAnyAnnotation = true;
                    ExtractAtcValidationAttribute(attribute, attrFullName, atc);
                }
            }
        }

        return (atc.HasAnyValue ? atc.Build() : null, hasAnyAnnotation);
    }

    private static void ExtractAtcNamespaceAttribute(
        AttributeData attribute,
        string attrFullName,
        AtcAnnotationBuilder atc)
    {
        switch (attrFullName)
        {
            case "Atc.IgnoreDisplayAttribute":
                atc.IsIgnoreDisplay = true;
                break;

            case "Atc.EnumGuidAttribute":
                if (attribute.ConstructorArguments.Length > 0 &&
                    attribute.ConstructorArguments[0].Value is string guidStr)
                {
                    atc.EnumGuid = guidStr;
                }

                break;

            case "Atc.CasingStyleDescriptionAttribute":
                atc.CasingStyleDefault = GetNamedArg<object>(attribute, "Default")?.ToString();
                atc.CasingStylePrefix = GetNamedArg<string>(attribute, "Prefix");
                break;
        }
    }

    [SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK - attribute extraction logic")]
    private static void ExtractAtcValidationAttribute(
        AttributeData attribute,
        string attrFullName,
        AtcAnnotationBuilder atc)
    {
        switch (attrFullName)
        {
            case "System.ComponentModel.DataAnnotations.IPAddressAttribute":
                atc.IsIPAddress = true;
                atc.IPAddressRequired = GetBoolConstructorArgOrNamedArg(attribute, "Required");
                break;

            case "System.ComponentModel.DataAnnotations.IsoCurrencySymbolAttribute":
                atc.IsIsoCurrencySymbol = true;
                atc.IsoCurrencySymbolRequired = GetNamedArg<bool?>(attribute, "Required");
                atc.IsoCurrencySymbols = GetNamedArgStringArray(attribute, "IsoCurrencySymbols");
                break;

            case "System.ComponentModel.DataAnnotations.StringAttribute":
                atc.IsAtcString = true;
                ExtractAtcStringAttributeProperties(attribute, atc);
                break;

            case "System.ComponentModel.DataAnnotations.KeyStringAttribute":
                atc.IsKeyString = true;
                atc.IsAtcString = true;
                ExtractAtcStringAttributeProperties(attribute, atc);
                break;

            case "System.ComponentModel.DataAnnotations.UriAttribute":
                atc.IsAtcUri = true;
                ExtractAtcUriAttributeProperties(attribute, atc);
                break;
        }
    }

    private static void ExtractAtcStringAttributeProperties(
        AttributeData attribute,
        AtcAnnotationBuilder atc)
    {
        var args = attribute.ConstructorArguments;

        // Handle constructor overloads
        if (args.Length >= 3)
        {
            // StringAttribute(bool required, uint minLength, uint maxLength, ...)
            if (args[0].Value is bool req)
            {
                atc.StringRequired = req;
            }

            if (args[1].Value is uint minLen)
            {
                atc.StringMinLength = minLen;
            }

            if (args[2].Value is uint maxLen)
            {
                atc.StringMaxLength = maxLen;
            }

            // Check for additional constructor parameters
            if (args.Length >= 5 && args[3].Kind == TypedConstantKind.Array)
            {
                // StringAttribute(bool, uint, uint, char[], string[])
                atc.StringInvalidCharacters = GetCharArrayFromTypedConstant(args[3]);
                atc.StringInvalidPrefixStrings = GetStringArrayFromTypedConstant(args[4]);
            }
            else if (args.Length >= 4 && args[3].Value is string regEx)
            {
                // StringAttribute(bool, uint, uint, string regularExpression)
                atc.StringRegularExpression = regEx;
            }
        }

        // Override with named arguments if present
        atc.StringRequired ??= GetNamedArg<bool?>(attribute, "Required");
        atc.StringMinLength ??= GetNamedArgUInt(attribute, "MinLength");
        atc.StringMaxLength ??= GetNamedArgUInt(attribute, "MaxLength");
        atc.StringRegularExpression ??= GetNamedArg<string>(attribute, "RegularExpression");
        atc.StringInvalidCharacters ??= GetNamedArgCharArray(attribute, "InvalidCharacters");
        atc.StringInvalidPrefixStrings ??= GetNamedArgStringArray(attribute, "InvalidPrefixStrings");
    }

    private static void ExtractAtcUriAttributeProperties(
        AttributeData attribute,
        AtcAnnotationBuilder atc)
    {
        var args = attribute.ConstructorArguments;

        // Handle constructor: UriAttribute(bool required, bool allowHttp, bool allowHttps, bool allowFtp, bool allowFtps, bool allowFile, bool allowOpcTcp)
        if (args.Length >= 7)
        {
            if (args[0].Value is bool req)
            {
                atc.UriRequired = req;
            }

            if (args[1].Value is bool http)
            {
                atc.UriAllowHttp = http;
            }

            if (args[2].Value is bool https)
            {
                atc.UriAllowHttps = https;
            }

            if (args[3].Value is bool ftp)
            {
                atc.UriAllowFtp = ftp;
            }

            if (args[4].Value is bool ftps)
            {
                atc.UriAllowFtps = ftps;
            }

            if (args[5].Value is bool file)
            {
                atc.UriAllowFile = file;
            }

            if (args[6].Value is bool opcTcp)
            {
                atc.UriAllowOpcTcp = opcTcp;
            }
        }

        // Override with named arguments if present
        atc.UriRequired ??= GetNamedArg<bool?>(attribute, "Required");
        atc.UriAllowHttp ??= GetNamedArg<bool?>(attribute, "AllowHttp");
        atc.UriAllowHttps ??= GetNamedArg<bool?>(attribute, "AllowHttps");
        atc.UriAllowFtp ??= GetNamedArg<bool?>(attribute, "AllowFtp");
        atc.UriAllowFtps ??= GetNamedArg<bool?>(attribute, "AllowFtps");
        atc.UriAllowFile ??= GetNamedArg<bool?>(attribute, "AllowFile");
        atc.UriAllowOpcTcp ??= GetNamedArg<bool?>(attribute, "AllowOpcTcp");
    }

    private static bool? GetBoolConstructorArgOrNamedArg(
        AttributeData attribute,
        string namedArgName)
    {
        // Check constructor arguments first
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is bool boolVal)
        {
            return boolVal;
        }

        return GetNamedArg<bool?>(attribute, namedArgName);
    }

    private static uint? GetNamedArgUInt(
        AttributeData attribute,
        string name)
    {
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key == name && arg.Value.Value is uint value)
            {
                return value;
            }
        }

        return null;
    }

    private static char[]? GetCharArrayFromTypedConstant(TypedConstant constant)
    {
        if (constant.Kind != TypedConstantKind.Array || constant.Values.IsDefault)
        {
            return null;
        }

        var result = new List<char>();
        foreach (var item in constant.Values)
        {
            if (item.Value is char c)
            {
                result.Add(c);
            }
        }

        return result.Count > 0 ? [.. result] : null;
    }

    private static string[]? GetStringArrayFromTypedConstant(
        TypedConstant constant)
    {
        if (constant.Kind != TypedConstantKind.Array || constant.Values.IsDefault)
        {
            return null;
        }

        var result = new List<string>();
        foreach (var item in constant.Values)
        {
            if (item.Value is string s)
            {
                result.Add(s);
            }
        }

        return result.Count > 0 ? [.. result] : null;
    }

    private static char[]? GetNamedArgCharArray(
        AttributeData attribute,
        string name)
        => (from arg
            in attribute.NamedArguments
            where arg.Key == name
            select GetCharArrayFromTypedConstant(arg.Value)).FirstOrDefault();

    private static string[]? GetNamedArgStringArray(
        AttributeData attribute,
        string name)
    {
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key == name)
            {
                return GetStringArrayFromTypedConstant(arg.Value);
            }
        }

        return null;
    }

    private static DisplayAnnotationInfo ExtractDisplayAttribute(
        AttributeData attribute)
        => new(
            Name: GetNamedArg<string>(attribute, "Name"),
            Description: GetNamedArg<string>(attribute, "Description"),
            ShortName: GetNamedArg<string>(attribute, "ShortName"),
            GroupName: GetNamedArg<string>(attribute, "GroupName"),
            Prompt: GetNamedArg<string>(attribute, "Prompt"),
            Order: GetNamedArg<int?>(attribute, "Order"));

    private static void ExtractRangeAttribute(
        AttributeData attribute,
        ValidationAnnotationBuilder validation)
    {
        var args = attribute.ConstructorArguments;

        if (args.Length >= 2)
        {
            // Check for Range(Type, string, string) overload
            if (args[0].Value is INamedTypeSymbol typeSymbol)
            {
                validation.RangeOperandType = typeSymbol.ToDisplayString();
                validation.RangeMinimum = args[1].Value?.ToString();
                if (args.Length > 2)
                {
                    validation.RangeMaximum = args[2].Value?.ToString();
                }
            }
            else
            {
                // Range(int, int) or Range(double, double) overloads
                validation.RangeMinimum = args[0].Value?.ToString();
                validation.RangeMaximum = args[1].Value?.ToString();

                // Infer operand type from the argument type
                if (args[0].Type is { } argType)
                {
                    validation.RangeOperandType = argType.ToDisplayString();
                }
            }
        }

        validation.RangeErrorMessage = GetNamedArg<string>(attribute, "ErrorMessage");
    }

    private static T? GetNamedArg<T>(
        AttributeData attribute,
        string name)
    {
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key != name)
            {
                continue;
            }

            if (arg.Value.Value is T value)
            {
                return value;
            }

            // Handle nullable value types
            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>) &&
                arg.Value.Value is not null)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));
                if (underlyingType is not null &&
                    underlyingType.IsInstanceOfType(arg.Value.Value))
                {
                    return (T)arg.Value.Value;
                }
            }

            break;
        }

        return default;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<TypeAnnotationInfo?> types,
        AnnotationGeneratorConfig config,
        SourceProductionContext context)
    {
        if (types.IsDefaultOrEmpty)
        {
            return;
        }

        // Group types by namespace for generation
        var distinctTypes = types
            .Where(t => t is not null)
            .Cast<TypeAnnotationInfo>()
            .Distinct()
            .ToList();

        foreach (var typeInfo in distinctTypes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            GenerateConstantsForType(typeInfo, config, context);
        }
    }

    private static void GenerateConstantsForType(
        TypeAnnotationInfo typeInfo,
        AnnotationGeneratorConfig config,
        SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLineLf("// <auto-generated/>");
        sb.AppendLineLf("#nullable enable");
        sb.AppendLineLf();
        sb.AppendLineLf($"namespace {typeInfo.Namespace};");
        sb.AppendLineLf();
        sb.AppendLineLf("/// <summary>");
        sb.AppendLineLf($"/// Annotation constants for {typeInfo.TypeName}.");
        sb.AppendLineLf("/// </summary>");
        sb.AppendLineLf("public static partial class AnnotationConstants");
        sb.AppendLineLf("{");

        GenerateTypeConstants(sb, typeInfo);

        sb.AppendLineLf("}");

        var fileName = $"AnnotationConstants.{typeInfo.Namespace}.{typeInfo.TypeName}.g.cs";
        context.AddSource(fileName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateTypeConstants(
        StringBuilder sb,
        TypeAnnotationInfo typeInfo)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Annotation constants for {typeInfo.TypeName} properties.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    [global::System.CodeDom.Compiler.GeneratedCode(\"Atc.SourceGenerators.AnnotationConstants\", \"1.0.0\")]");
        sb.AppendLineLf("    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLineLf("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        sb.AppendLineLf("    [global::System.Diagnostics.DebuggerNonUserCode]");
        sb.AppendLineLf("    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        sb.AppendLineLf($"    public static partial class {typeInfo.TypeName}");
        sb.AppendLineLf("    {");

        for (var i = 0; i < typeInfo.Properties.Length; i++)
        {
            var property = typeInfo.Properties[i];
            GeneratePropertyConstants(sb, property);

            if (i < typeInfo.Properties.Length - 1)
            {
                sb.AppendLineLf();
            }
        }

        sb.AppendLineLf("    }");
    }

    private static void GeneratePropertyConstants(
        StringBuilder sb,
        PropertyAnnotationInfo property)
    {
        sb.AppendLineLf("        /// <summary>");
        sb.AppendLineLf($"        /// Annotation constants for the {property.PropertyName} property.");
        sb.AppendLineLf("        /// </summary>");
        sb.AppendLineLf($"        public static partial class {property.PropertyName}");
        sb.AppendLineLf("        {");

        var hasContent = false;

        // Display constants (Microsoft)
        if (property.Display is not null)
        {
            hasContent = GenerateDisplayConstants(sb, property.Display, property.PropertyName) || hasContent;
        }

        // Microsoft validation constants
        if (property.MicrosoftValidation is not null)
        {
            if (hasContent)
            {
                sb.AppendLineLf();
            }

            GenerateMicrosoftValidationConstants(sb, property.MicrosoftValidation);
            hasContent = true;
        }

        // Atc annotation constants
        if (property.AtcAnnotation is not null)
        {
            if (hasContent)
            {
                sb.AppendLineLf();
            }

            GenerateAtcConstants(sb, property.AtcAnnotation);
        }

        sb.AppendLineLf("        }");
    }

    private static bool GenerateDisplayConstants(
        StringBuilder sb,
        DisplayAnnotationInfo display,
        string propertyName)
    {
        var hasContent = false;

        if (display.Name is not null)
        {
            sb.AppendLineLf($"            public const string DisplayName = \"{EscapeString(display.Name)}\";");
            hasContent = true;
        }

        if (display.Description is not null)
        {
            // Use DisplayDescription to avoid conflict when property is named "Description"
            var constantName = propertyName == "Description" ? "DisplayDescription" : "Description";
            sb.AppendLineLf($"            public const string {constantName} = \"{EscapeString(display.Description)}\";");
            hasContent = true;
        }

        if (display.ShortName is not null)
        {
            // Use DisplayShortName to avoid conflict when property is named "ShortName"
            var constantName = propertyName == "ShortName" ? "DisplayShortName" : "ShortName";
            sb.AppendLineLf($"            public const string {constantName} = \"{EscapeString(display.ShortName)}\";");
            hasContent = true;
        }

        if (display.GroupName is not null)
        {
            // Use DisplayGroupName to avoid conflict when property is named "GroupName"
            var constantName = propertyName == "GroupName" ? "DisplayGroupName" : "GroupName";
            sb.AppendLineLf($"            public const string {constantName} = \"{EscapeString(display.GroupName)}\";");
            hasContent = true;
        }

        if (display.Prompt is not null)
        {
            // Use DisplayPrompt to avoid conflict when property is named "Prompt"
            var constantName = propertyName == "Prompt" ? "DisplayPrompt" : "Prompt";
            sb.AppendLineLf($"            public const string {constantName} = \"{EscapeString(display.Prompt)}\";");
            hasContent = true;
        }

        if (display.Order.HasValue)
        {
            // Use DisplayOrder to avoid conflict when property is named "Order"
            var constantName = propertyName == "Order" ? "DisplayOrder" : "Order";
            sb.AppendLineLf($"            public const int {constantName} = {display.Order.Value};");
            hasContent = true;
        }

        return hasContent;
    }

    [SuppressMessage("Style", "ATC203:Method chains with 2 or more calls should be placed on separate lines", Justification = "OK.")]
    [SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK - validation constant generation")]
    private static void GenerateMicrosoftValidationConstants(
        StringBuilder sb,
        ValidationAnnotationInfo validation)
    {
        // Required
        if (validation.IsRequired)
        {
            sb.AppendLineLf("            public const bool IsRequired = true;");
        }

        if (validation.AllowEmptyStrings.HasValue)
        {
            sb.AppendLineLf($"            public const bool AllowEmptyStrings = {validation.AllowEmptyStrings.Value.ToString().ToLowerInvariant()};");
        }

        if (validation.RequiredErrorMessage is not null)
        {
            sb.AppendLineLf($"            public const string RequiredErrorMessage = \"{EscapeString(validation.RequiredErrorMessage)}\";");
        }

        // Length constraints
        if (validation.MinLength.HasValue)
        {
            sb.AppendLineLf($"            public const int MinimumLength = {validation.MinLength.Value};");
        }

        if (validation.MaxLength.HasValue)
        {
            sb.AppendLineLf($"            public const int MaximumLength = {validation.MaxLength.Value};");
        }

        if (validation.StringLengthErrorMessage is not null)
        {
            sb.AppendLineLf($"            public const string StringLengthErrorMessage = \"{EscapeString(validation.StringLengthErrorMessage)}\";");
        }

        // Range
        if (validation.RangeMinimum is not null)
        {
            sb.AppendLineLf($"            public const string Minimum = \"{EscapeString(validation.RangeMinimum)}\";");
        }

        if (validation.RangeMaximum is not null)
        {
            sb.AppendLineLf($"            public const string Maximum = \"{EscapeString(validation.RangeMaximum)}\";");
        }

        if (validation.RangeOperandType is not null)
        {
            sb.AppendLineLf($"            public static readonly global::System.Type OperandType = typeof({validation.RangeOperandType});");
        }

        if (validation.RangeErrorMessage is not null)
        {
            sb.AppendLineLf($"            public const string RangeErrorMessage = \"{EscapeString(validation.RangeErrorMessage)}\";");
        }

        // RegularExpression
        if (validation.RegularExpressionPattern is not null)
        {
            sb.AppendLineLf($"            public const string Pattern = @\"{EscapeVerbatimString(validation.RegularExpressionPattern)}\";");
        }

        if (validation.RegularExpressionErrorMessage is not null)
        {
            sb.AppendLineLf($"            public const string RegularExpressionErrorMessage = \"{EscapeString(validation.RegularExpressionErrorMessage)}\";");
        }

        // Data type flags
        if (validation.IsEmailAddress)
        {
            sb.AppendLineLf("            public const bool IsEmailAddress = true;");
        }

        if (validation.IsPhone)
        {
            sb.AppendLineLf("            public const bool IsPhone = true;");
        }

        if (validation.IsUrl)
        {
            sb.AppendLineLf("            public const bool IsUrl = true;");
        }

        if (validation.IsCreditCard)
        {
            sb.AppendLineLf("            public const bool IsCreditCard = true;");
        }

        if (validation.DataType.HasValue)
        {
            sb.AppendLineLf($"            public const int DataType = {validation.DataType.Value};");
        }

        // Compare
        if (validation.CompareOtherProperty is not null)
        {
            sb.AppendLineLf($"            public const string CompareProperty = \"{EscapeString(validation.CompareOtherProperty)}\";");
        }

        // Metadata
        if (validation.IsKey)
        {
            sb.AppendLineLf("            public const bool IsKey = true;");
        }

        if (validation.IsEditable.HasValue)
        {
            sb.AppendLineLf($"            public const bool IsEditable = {validation.IsEditable.Value.ToString().ToLowerInvariant()};");
        }

        if (validation.IsScaffoldColumn.HasValue)
        {
            sb.AppendLineLf($"            public const bool IsScaffoldColumn = {validation.IsScaffoldColumn.Value.ToString().ToLowerInvariant()};");
        }

        if (validation.IsTimestamp)
        {
            sb.AppendLineLf("            public const bool IsTimestamp = true;");
        }
    }

    [SuppressMessage("Style", "ATC203:Method chains with 2 or more calls should be placed on separate lines", Justification = "OK.")]
    [SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK - Atc constant generation")]
    private static void GenerateAtcConstants(
        StringBuilder sb,
        AtcAnnotationInfo atc)
    {
        // IgnoreDisplayAttribute
        if (atc.IsIgnoreDisplay)
        {
            sb.AppendLineLf("            public const bool IsIgnoreDisplay = true;");
        }

        // EnumGuidAttribute
        if (atc.EnumGuid is not null)
        {
            sb.AppendLineLf($"            public const string EnumGuid = \"{EscapeString(atc.EnumGuid)}\";");
        }

        // CasingStyleDescriptionAttribute
        if (atc.CasingStyleDefault is not null)
        {
            sb.AppendLineLf($"            public const string CasingStyleDefault = \"{EscapeString(atc.CasingStyleDefault)}\";");
        }

        if (atc.CasingStylePrefix is not null)
        {
            sb.AppendLineLf($"            public const string CasingStylePrefix = \"{EscapeString(atc.CasingStylePrefix)}\";");
        }

        // IPAddressAttribute
        if (atc.IsIPAddress)
        {
            sb.AppendLineLf("            public const bool IsIPAddress = true;");
            if (atc.IPAddressRequired.HasValue)
            {
                sb.AppendLineLf($"            public const bool IPAddressRequired = {atc.IPAddressRequired.Value.ToString().ToLowerInvariant()};");
            }
        }

        // IsoCurrencySymbolAttribute
        if (atc.IsIsoCurrencySymbol)
        {
            sb.AppendLineLf("            public const bool IsIsoCurrencySymbol = true;");
            if (atc.IsoCurrencySymbolRequired.HasValue)
            {
                sb.AppendLineLf($"            public const bool IsoCurrencySymbolRequired = {atc.IsoCurrencySymbolRequired.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.IsoCurrencySymbols is { Length: > 0 })
            {
                var symbols = string.Join("\", \"", atc.IsoCurrencySymbols.Select(EscapeString));
                sb.AppendLineLf($"            public static readonly string[] AllowedIsoCurrencySymbols = new[] {{ \"{symbols}\" }};");
            }
        }

        // StringAttribute / KeyStringAttribute
        if (atc.IsAtcString)
        {
            sb.AppendLineLf("            public const bool IsAtcString = true;");

            if (atc.IsKeyString)
            {
                sb.AppendLineLf("            public const bool IsKeyString = true;");
            }

            if (atc.StringRequired.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcStringRequired = {atc.StringRequired.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.StringMinLength.HasValue)
            {
                sb.AppendLineLf($"            public const uint AtcStringMinLength = {atc.StringMinLength.Value};");
            }

            if (atc.StringMaxLength.HasValue)
            {
                sb.AppendLineLf($"            public const uint AtcStringMaxLength = {atc.StringMaxLength.Value};");
            }

            if (atc.StringRegularExpression is not null)
            {
                sb.AppendLineLf($"            public const string AtcStringRegularExpression = @\"{EscapeVerbatimString(atc.StringRegularExpression)}\";");
            }

            if (atc.StringInvalidCharacters is { Length: > 0 })
            {
                var chars = string.Join("', '", atc.StringInvalidCharacters.Select(c => EscapeChar(c)));
                sb.AppendLineLf($"            public static readonly char[] AtcStringInvalidCharacters = new[] {{ '{chars}' }};");
            }

            if (atc.StringInvalidPrefixStrings is { Length: > 0 })
            {
                var prefixes = string.Join("\", \"", atc.StringInvalidPrefixStrings.Select(EscapeString));
                sb.AppendLineLf($"            public static readonly string[] AtcStringInvalidPrefixStrings = new[] {{ \"{prefixes}\" }};");
            }
        }

        // UriAttribute
        if (atc.IsAtcUri)
        {
            sb.AppendLineLf("            public const bool IsAtcUri = true;");

            if (atc.UriRequired.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcUriRequired = {atc.UriRequired.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.UriAllowHttp.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcUriAllowHttp = {atc.UriAllowHttp.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.UriAllowHttps.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcUriAllowHttps = {atc.UriAllowHttps.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.UriAllowFtp.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcUriAllowFtp = {atc.UriAllowFtp.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.UriAllowFtps.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcUriAllowFtps = {atc.UriAllowFtps.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.UriAllowFile.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcUriAllowFile = {atc.UriAllowFile.Value.ToString().ToLowerInvariant()};");
            }

            if (atc.UriAllowOpcTcp.HasValue)
            {
                sb.AppendLineLf($"            public const bool AtcUriAllowOpcTcp = {atc.UriAllowOpcTcp.Value.ToString().ToLowerInvariant()};");
            }
        }
    }

    private static string EscapeChar(char c)
        => c switch
        {
            '\\' => @"\\",
            '\'' => "\\'",
            '\r' => "\\r",
            '\n' => "\\n",
            '\t' => "\\t",
            _ => c.ToString(),
        };

    private static string EscapeString(string value)
        => value
            .Replace("\\", @"\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");

    // In verbatim strings, only double quotes need escaping (by doubling)
    private static string EscapeVerbatimString(string value)
        => value.Replace("\"", "\"\"");
}