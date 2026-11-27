namespace Atc.SourceGenerators.Tests.Generators.IncrementalCaching;

/// <summary>
/// Incremental caching: steps whose models are equatable must be reused when an unrelated file changes.
/// </summary>
public class IncrementalCachingTests
{
    [Fact]
    public void MappingConfiguration_AttributeMappedPairs_Should_Be_Cached_After_Unrelated_Edit()
        => IncrementalGeneratorAssert.StepIsCachedAfterUnrelatedEdit(
            [new MappingConfigurationGenerator(), new ObjectMappingGenerator(), new EnumMappingGenerator()],
            """
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            public class TargetDto
            {
                public int Id { get; set; }
            }

            [MapTo(typeof(TargetDto))]
            public partial class Source
            {
                public int Id { get; set; }
            }
            """,
            "MappingConfiguration.AttributeMappedPairs");

    [Fact]
    public void AnnotationConstants_Should_Be_Fully_Cached_After_Unrelated_Edit()
        => IncrementalGeneratorAssert.StepIsCachedAfterUnrelatedEdit(
            [new AnnotationConstantsGenerator()],
            """
            using System.ComponentModel.DataAnnotations;

            namespace TestNamespace;

            public class Product
            {
                [Required]
                [StringLength(100)]
                public string Name { get; set; } = string.Empty;

                [Range(0.5, 9.5)]
                public double Weight { get; set; }
            }
            """,
            "AnnotationConstants.Types",
            outputStepsMustBeCached: true);
}