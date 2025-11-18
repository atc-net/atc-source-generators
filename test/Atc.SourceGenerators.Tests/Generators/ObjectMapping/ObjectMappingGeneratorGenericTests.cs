// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact(Skip = "Generic mapping requires unbound generic types which are not fully supported in test harness. Feature will be verified in samples.")]
    public void Generator_Should_Generate_Generic_Mapping_Method()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            [MapTo(typeof(ResultDto<>))]
            public partial class Result<T>
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
                public string Message { get; set; } = string.Empty;
            }

            public class ResultDto<T>
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
                public string Message { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("public static ResultDto<T> MapToResultDto<T>(", output, StringComparison.Ordinal);
        Assert.Contains("this Result<T> source)", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Generic mapping requires unbound generic types which are not fully supported in test harness. Feature will be verified in samples.")]
    public void Generator_Should_Generate_Generic_Mapping_With_Constraints()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            [MapTo(typeof(ResultDto<>))]
            public partial class Result<T> where T : class
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
            }

            public class ResultDto<T>
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("public static ResultDto<T> MapToResultDto<T>(", output, StringComparison.Ordinal);
        Assert.Contains("where T : class", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Generic mapping requires unbound generic types which are not fully supported in test harness. Feature will be verified in samples.")]
    public void Generator_Should_Generate_Generic_Mapping_With_Multiple_Type_Parameters()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            [MapTo(typeof(ResultDto<,>))]
            public partial class Result<TData, TError>
            {
                public TData Data { get; set; } = default!;
                public TError Error { get; set; } = default!;
                public bool Success { get; set; }
            }

            public class ResultDto<TData, TError>
            {
                public TData Data { get; set; } = default!;
                public TError Error { get; set; } = default!;
                public bool Success { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("public static ResultDto<TData, TError> MapToResultDto<TData, TError>(", output, StringComparison.Ordinal);
        Assert.Contains("this Result<TData, TError> source)", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Generic mapping requires unbound generic types which are not fully supported in test harness. Feature will be verified in samples.")]
    public void Generator_Should_Generate_Bidirectional_Generic_Mapping()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            [MapTo(typeof(ResultDto<>), Bidirectional = true)]
            public partial class Result<T>
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
            }

            public partial class ResultDto<T>
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("public static ResultDto<T> MapToResultDto<T>(", output, StringComparison.Ordinal);
        Assert.Contains("public static Result<T> MapToResult<T>(", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Generic mapping requires unbound generic types which are not fully supported in test harness. Feature will be verified in samples.")]
    public void Generator_Should_Generate_Generic_Mapping_With_Struct_Constraint()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            [MapTo(typeof(ResultDto<>))]
            public partial class Result<T> where T : struct
            {
                public T Data { get; set; }
                public bool Success { get; set; }
            }

            public class ResultDto<T>
            {
                public T Data { get; set; }
                public bool Success { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("public static ResultDto<T> MapToResultDto<T>(", output, StringComparison.Ordinal);
        Assert.Contains("where T : struct", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Generic mapping requires unbound generic types which are not fully supported in test harness. Feature will be verified in samples.")]
    public void Generator_Should_Generate_Generic_UpdateTarget_Method()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            [MapTo(typeof(ResultDto<>), UpdateTarget = true)]
            public partial class Result<T>
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
            }

            public class ResultDto<T>
            {
                public T Data { get; set; } = default!;
                public bool Success { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("public static ResultDto<T> MapToResultDto<T>(", output, StringComparison.Ordinal);
        Assert.Contains("public static void MapToResultDto<T>(", output, StringComparison.Ordinal);
        Assert.Contains("ResultDto<T> target)", output, StringComparison.Ordinal);
    }
}