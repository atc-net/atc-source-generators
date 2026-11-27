namespace Atc.SourceGenerators.Tests.Integration;

/// <summary>
/// Packs Atc.SourceGenerators once into a temporary folder, isolated from the solution's own build output.
/// </summary>
public sealed class PackageFixture : IAsyncLifetime
{
    public const string PackageVersion = "0.0.0-smoke";

    public string WorkDirectory { get; } = Path.Combine(
        Path.GetTempPath(),
        "atc-sg-package-" + Guid.NewGuid().ToString("N"));

    public string PackageDirectory => Path.Combine(WorkDirectory, "packages");

    public string PackagePath
        => Path.Combine(PackageDirectory, $"Atc.SourceGenerators.{PackageVersion}.nupkg");

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(WorkDirectory);

        // Stop MSBuild from importing Directory.Build.* files above the temporary folder
        await File.WriteAllTextAsync(Path.Combine(WorkDirectory, "Directory.Build.props"), "<Project />");
        await File.WriteAllTextAsync(Path.Combine(WorkDirectory, "Directory.Build.targets"), "<Project />");

        var projectPath = Path.Combine(DotnetCli.SolutionRoot, "src", "Atc.SourceGenerators", "Atc.SourceGenerators.csproj");
        var result = await DotnetCli.RunAsync(
            $"pack \"{projectPath}\" -c Release -o \"{PackageDirectory}\" --artifacts-path \"{Path.Combine(WorkDirectory, "artifacts")}\" -p:Version={PackageVersion}",
            DotnetCli.SolutionRoot,
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        DotnetCli.AssertSucceeded(result, "dotnet pack");
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            Directory.Delete(WorkDirectory, recursive: true);
        }
        catch (IOException)
        {
            // A process may still hold a file; the folder is in the temp directory
        }
        catch (UnauthorizedAccessException)
        {
            // Same as above
        }

        return ValueTask.CompletedTask;
    }
}