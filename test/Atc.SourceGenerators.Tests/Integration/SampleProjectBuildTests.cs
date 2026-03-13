namespace Atc.SourceGenerators.Tests.Integration;

[Trait("Category", "Integration")]
public class SampleProjectBuildTests
{
    private static readonly string SolutionRoot = FindSolutionRoot();

    [Fact]
    public void DependencyRegistration_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.DependencyRegistration/Atc.SourceGenerators.DependencyRegistration.csproj");
    }

    [Fact]
    public void DependencyRegistration_Domain_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.DependencyRegistration.Domain/Atc.SourceGenerators.DependencyRegistration.Domain.csproj");
    }

    [Fact]
    public void OptionsBinding_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.OptionsBinding/Atc.SourceGenerators.OptionsBinding.csproj");
    }

    [Fact]
    public void OptionsBinding_Domain_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.OptionsBinding.Domain/Atc.SourceGenerators.OptionsBinding.Domain.csproj");
    }

    [Fact]
    public void Mapping_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.Mapping/Atc.SourceGenerators.Mapping.csproj");
    }

    [Fact]
    public void Mapping_Domain_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.Mapping.Domain/Atc.SourceGenerators.Mapping.Domain.csproj");
    }

    [Fact]
    public void Mapping_DataAccess_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.Mapping.DataAccess/Atc.SourceGenerators.Mapping.DataAccess.csproj");
    }

    [Fact]
    public void Mapping_Contract_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.Mapping.Contract/Atc.SourceGenerators.Mapping.Contract.csproj");
    }

    [Fact]
    public void EnumMapping_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.EnumMapping/Atc.SourceGenerators.EnumMapping.csproj");
    }

    [Fact]
    public void MappingOnlyConfiguration_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingOnlyConfiguration/Atc.SourceGenerators.MappingOnlyConfiguration.csproj");
    }

    [Fact]
    public void MappingOnlyConfiguration_Domain_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingOnlyConfiguration.Domain/Atc.SourceGenerators.MappingOnlyConfiguration.Domain.csproj");
    }

    [Fact]
    public void MappingOnlyConfiguration_ExternalCrm_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingOnlyConfiguration.ExternalCrm/Atc.SourceGenerators.MappingOnlyConfiguration.ExternalCrm.csproj");
    }

    [Fact]
    public void MappingCombinedConfiguration_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingCombinedConfiguration/Atc.SourceGenerators.MappingCombinedConfiguration.csproj");
    }

    [Fact]
    public void MappingCombinedConfiguration_Domain_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingCombinedConfiguration.Domain/Atc.SourceGenerators.MappingCombinedConfiguration.Domain.csproj");
    }

    [Fact]
    public void MappingCombinedConfiguration_Contract_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingCombinedConfiguration.Contract/Atc.SourceGenerators.MappingCombinedConfiguration.Contract.csproj");
    }

    [Fact]
    public void MappingCombinedConfiguration_ExternalAnalytics_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalAnalytics/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalAnalytics.csproj");
    }

    [Fact]
    public void MappingCombinedConfiguration_ExternalNotifications_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalNotifications/Atc.SourceGenerators.MappingCombinedConfiguration.ExternalNotifications.csproj");
    }

    [Fact]
    public void AnnotationConstants_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/Atc.SourceGenerators.AnnotationConstants/Atc.SourceGenerators.AnnotationConstants.csproj");
    }

    [Fact]
    public void PetStore_Api_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/PetStore.Api/PetStore.Api.csproj");
    }

    [Fact]
    public void PetStore_Domain_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/PetStore.Domain/PetStore.Domain.csproj");
    }

    [Fact]
    public void PetStore_DataAccess_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/PetStore.DataAccess/PetStore.DataAccess.csproj");
    }

    [Fact]
    public void PetStore_ApiContract_Sample_Should_Build()
    {
        AssertProjectBuilds("sample/PetStore.Api.Contract/PetStore.Api.Contract.csproj");
    }

    private static void AssertProjectBuilds(string relativePath)
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

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        var exited = process.WaitForExit(120_000);

        Assert.True(exited, $"Build timed out after 120 seconds for: {relativePath}");

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