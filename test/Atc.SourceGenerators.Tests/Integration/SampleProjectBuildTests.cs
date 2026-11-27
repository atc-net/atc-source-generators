namespace Atc.SourceGenerators.Tests.Integration;

[Trait("Category", "Integration")]
public class SampleProjectBuildTests
{
    private static readonly string SolutionRoot = FindSolutionRoot();

    [Fact]
    public Task DependencyRegistration_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.DependencyRegistration/Atc.SourceGenerators.DependencyRegistration.csproj");

    [Fact]
    public Task DependencyRegistration_Domain_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.DependencyRegistration.Domain/Atc.SourceGenerators.DependencyRegistration.Domain.csproj");

    [Fact]
    public Task OptionsBinding_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.OptionsBinding/Atc.SourceGenerators.OptionsBinding.csproj");

    [Fact]
    public Task OptionsBinding_Domain_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.OptionsBinding.Domain/Atc.SourceGenerators.OptionsBinding.Domain.csproj");

    [Fact]
    public Task Mapping_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.Mapping/Atc.SourceGenerators.Mapping.csproj");

    [Fact]
    public Task Mapping_Domain_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.Mapping.Domain/Atc.SourceGenerators.Mapping.Domain.csproj");

    [Fact]
    public Task Mapping_DataAccess_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.Mapping.DataAccess/Atc.SourceGenerators.Mapping.DataAccess.csproj");

    [Fact]
    public Task Mapping_Contract_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.Mapping.Contract/Atc.SourceGenerators.Mapping.Contract.csproj");

    [Fact]
    public Task EnumMapping_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.EnumMapping/Atc.SourceGenerators.EnumMapping.csproj");

    [Fact]
    public Task MappingOnlyConfiguration_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingOnlyConfiguration/Atc.SourceGenerators.MappingOnlyConfiguration.csproj");

    [Fact]
    public Task MappingOnlyConfiguration_Domain_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingOnlyConfiguration.Domain/Atc.SourceGenerators.MappingOnlyConfiguration.Domain.csproj");

    [Fact]
    public Task MappingOnlyConfiguration_ExternalCrm_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingOnlyConfiguration.ExternalCrm/Atc.SourceGenerators.MappingOnlyConfiguration.ExternalCrm.csproj");

    [Fact]
    public Task MappingCombinedConfiguration_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingCombinedConfiguration/Atc.SourceGenerators.MappingCombinedConfiguration.csproj");

    [Fact]
    public Task MappingCombinedConfiguration_Domain_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingCombinedConfiguration.Domain/Atc.SourceGenerators.MappingCombinedConfiguration.Domain.csproj");

    [Fact]
    public Task MappingCombinedConfiguration_Contract_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingCombinedConfiguration.Contract/Atc.SourceGenerators.MappingCombinedConfiguration.Contract.csproj");

    [Fact]
    public Task MappingCombinedConfiguration_ExternalAnalytics_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalAnalytics/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalAnalytics.csproj");

    [Fact]
    public Task MappingCombinedConfiguration_ExternalNotifications_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalNotifications/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalNotifications.csproj");

    [Fact]
    public Task AnnotationConstants_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/Atc.SourceGenerators.AnnotationConstants/Atc.SourceGenerators.AnnotationConstants.csproj");

    [Fact]
    public Task PetStore_Api_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/PetStore.Api/PetStore.Api.csproj");

    [Fact]
    public Task PetStore_Domain_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/PetStore.Domain/PetStore.Domain.csproj");

    [Fact]
    public Task PetStore_DataAccess_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/PetStore.DataAccess/PetStore.DataAccess.csproj");

    [Fact]
    public Task PetStore_ApiContract_Sample_Should_Build()
        => AssertProjectBuildsAsync("sample/PetStore.Api.Contract/PetStore.Api.Contract.csproj");

    private static async Task AssertProjectBuildsAsync(string relativePath)
    {
        var projectPath = Path.Combine(SolutionRoot, relativePath);
        Assert.True(File.Exists(projectPath), $"Project file not found: {projectPath}");

        using var process = new System.Diagnostics.Process();
        process.StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\" -c Release --no-restore -v q",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = SolutionRoot,
        };

        process.Start();

        // Read both streams concurrently (reading one to the end first can deadlock when the other fills its
        // buffer) and enforce the timeout while waiting, killing the build when it is exceeded
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(120));

        var stdoutTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderrTask = process.StandardError.ReadToEndAsync(timeout.Token);

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"Build timed out after 120 seconds for: {relativePath}");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        var message = $"Build failed for '{relativePath}' with exit code {process.ExitCode}.{Environment.NewLine}" +
            $"--- stdout ---{Environment.NewLine}{stdout}{Environment.NewLine}" +
            $"--- stderr ---{Environment.NewLine}{stderr}";
        Assert.True(process.ExitCode == 0, message);
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !dir.GetFiles("*.slnx").Any())
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException(
                "Could not find solution root (no .slnx file found above " + AppContext.BaseDirectory + ")");
    }
}