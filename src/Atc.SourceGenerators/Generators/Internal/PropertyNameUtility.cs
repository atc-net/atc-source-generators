namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Utility class for converting property names between different casing strategies.
/// </summary>
internal static class PropertyNameUtility
{
    /// <summary>
    /// Converts a property name according to the specified naming strategy.
    /// </summary>
    /// <param name="propertyName">The original property name (typically PascalCase).</param>
    /// <param name="strategy">The target naming strategy.</param>
    /// <returns>The converted property name.</returns>
    public static string ConvertPropertyName(
        string propertyName,
        PropertyNameStrategy strategy)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        return strategy switch
        {
            PropertyNameStrategy.PascalCase => propertyName,
            PropertyNameStrategy.CamelCase => ToCamelCase(propertyName),
            PropertyNameStrategy.SnakeCase => ToSnakeCase(propertyName),
            PropertyNameStrategy.KebabCase => ToKebabCase(propertyName),
            _ => propertyName,
        };
    }

    /// <summary>
    /// Converts a PascalCase property name to camelCase.
    /// Example: "FirstName" → "firstName"
    /// </summary>
    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
        {
            return input;
        }

        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }

    /// <summary>
    /// Converts a PascalCase property name to snake_case.
    /// Example: "FirstName" → "first_name"
    /// </summary>
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var builder = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            if (i > 0 && char.IsUpper(currentChar))
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(currentChar));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts a PascalCase property name to kebab-case.
    /// Example: "FirstName" → "first-name"
    /// </summary>
    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var builder = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            if (i > 0 && char.IsUpper(currentChar))
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(currentChar));
        }

        return builder.ToString();
    }
}

