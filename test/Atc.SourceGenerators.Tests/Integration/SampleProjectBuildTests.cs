namespace Atc.SourceGenerators.Tests.Integration;

[Trait("Category", "Integration")]
public class SampleProjectBuildTests
{
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
        var projectPath = Path.Combine(DotnetCli.SolutionRoot, relativePath);
        Assert.True(File.Exists(projectPath), $"Project file not found: {projectPath}");

        var result = await DotnetCli.RunAsync(
            $"build \"{projectPath}\" -c Release --no-restore -v q",
            DotnetCli.SolutionRoot,
            TimeSpan.FromSeconds(120),
            TestContext.Current.CancellationToken);

        DotnetCli.AssertSucceeded(result, $"Build of '{relativePath}'");
    }
}