namespace Atc.SourceGenerators.Helpers;

/// <summary>
/// Formats values as C# literals for generated code: escaped strings and characters,
/// culture-invariant numbers with their type suffix, and enum values by member name.
/// </summary>
internal static class CSharpLiteral
{
    /// <summary>
    /// Formats a string as a quoted, escaped C# string literal.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The C# literal, e.g. <c>"a\"b"</c>.</returns>
    public static string FromString(string value)
        => SymbolDisplay.FormatLiteral(value, quote: true);

    /// <summary>
    /// Formats an attribute argument as a C# expression that evaluates to the same value and type.
    /// </summary>
    /// <param name="constant">The attribute argument.</param>
    /// <returns>The C# expression, or <c>null</c> when the argument is null.</returns>
    public static string FromTypedConstant(TypedConstant constant)
    {
        if (constant.IsNull)
        {
            return "null";
        }

        return constant.Kind switch
        {
            TypedConstantKind.Type when constant.Value is ITypeSymbol typeSymbol
                => $"typeof({typeSymbol.ToDisplayString()})",
            TypedConstantKind.Enum when constant.Type is not null
                => FromEnum(constant.Type, constant.Value),
            _ => FromPrimitive(constant.Value),
        };
    }

    /// <summary>
    /// Formats a primitive value (string, char, bool or number) as a C# literal of the same type.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The C# literal.</returns>
    public static string FromPrimitive(object? value)
        => value switch
        {
            null => "null",
            string s => FromString(s),
            char c => SymbolDisplay.FormatLiteral(c, quote: true),
            bool b => b ? "true" : "false",
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture) + "L",
            uint ui => ui.ToString(CultureInfo.InvariantCulture) + "U",
            ulong ul => ul.ToString(CultureInfo.InvariantCulture) + "UL",
            float f => f.ToString("R", CultureInfo.InvariantCulture) + "F",
            double d => d.ToString("R", CultureInfo.InvariantCulture) + "D",
            decimal m => m.ToString(CultureInfo.InvariantCulture) + "M",
            short s16 => $"(short){s16.ToString(CultureInfo.InvariantCulture)}",
            ushort u16 => $"(ushort){u16.ToString(CultureInfo.InvariantCulture)}",
            byte b8 => $"(byte){b8.ToString(CultureInfo.InvariantCulture)}",
            sbyte s8 => $"(sbyte){s8.ToString(CultureInfo.InvariantCulture)}",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "null",
        };

    private static string FromEnum(
        ITypeSymbol enumType,
        object? value)
    {
        var enumTypeName = enumType.ToGlobalDisplayString();

        // Prefer the member name; fall back to a cast for values without a member (e.g. combined flags)
        var member = enumType
            .GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, value));

        return member is not null
            ? $"{enumTypeName}.{member.Name}"
            : $"({enumTypeName}){FromPrimitive(value)}";
    }
}