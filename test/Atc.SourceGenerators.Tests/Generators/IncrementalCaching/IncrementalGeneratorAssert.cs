namespace Atc.SourceGenerators.Tests.Generators.IncrementalCaching;

/// <summary>
/// Asserts that an incremental generator step is reused (cached) when an unrelated file changes,
/// which is what keeps a generator cheap while typing in the IDE.
/// </summary>
internal static class IncrementalGeneratorAssert
{
    /// <summary>
    /// Runs the generators, adds an unrelated syntax tree, runs them again on the same driver and asserts
    /// that every output of the named step is <see cref="IncrementalStepRunReason.Cached"/> or
    /// <see cref="IncrementalStepRunReason.Unchanged"/>.
    /// </summary>
    /// <param name="generators">The generators to run (as they run together in a project).</param>
    /// <param name="source">The source code of the project.</param>
    /// <param name="trackingName">The tracking name of the step (set with <c>WithTrackingName</c>).</param>
    public static void StepIsCachedAfterUnrelatedEdit(
        IIncrementalGenerator[] generators,
        string source,
        string trackingName)
    {
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators.Select(GeneratorExtensions.AsSourceGenerator),
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var unrelatedTree = CSharpSyntaxTree.ParseText(
            "namespace Unrelated { public class Other { public int Value { get; set; } } }",
            cancellationToken: TestContext.Current.CancellationToken);
        driver = driver.RunGenerators(compilation.AddSyntaxTrees(unrelatedTree), TestContext.Current.CancellationToken);

        var steps = driver
            .GetRunResult()
            .Results
            .SelectMany(r => r.TrackedSteps.TryGetValue(trackingName, out var tracked) ? tracked : [])
            .ToList();

        Assert.NotEmpty(steps);
        Assert.All(
            steps.SelectMany(step => step.Outputs),
            output => Assert.True(
                output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                $"Step '{trackingName}' was re-run ({output.Reason}) after an unrelated edit."));
    }
}