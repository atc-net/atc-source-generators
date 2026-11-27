namespace Atc.SourceGenerators.Tests.Integration;

/// <summary>
/// Checks the packed nupkg and builds a consumer project against it, the way users install the package.
/// </summary>
[Trait("Category", "Package")]
public sealed class PackageTests(PackageFixture fixture) : IClassFixture<PackageFixture>
{
    [Fact]
    public void Package_Should_Contain_The_Generator_Only_As_An_Analyzer()
    {
        var entries = GetPackageEntries();

        Assert.Contains("analyzers/dotnet/cs/Atc.SourceGenerators.dll", entries, StringComparer.Ordinal);
        Assert.DoesNotContain(entries, e => e.StartsWith("lib/", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.Contains("Microsoft.CodeAnalysis", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Consumer_Should_Build_And_Run_Against_The_Package()
    {
        var consumerDirectory = await CreateConsumerProjectAsync();

        var run = await DotnetCli.RunAsync(
            "run -c Release",
            consumerDirectory,
            TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);

        DotnetCli.AssertSucceeded(run, "Building and running the consumer project");
        Assert.Contains("7|Ada|Active|50", run.Stdout, StringComparison.Ordinal);

        // The generator is a build-time analyzer: it must not be copied into the consumer's output
        var outputFiles = Directory.GetFiles(Path.Combine(consumerDirectory, "bin"), "*.dll", SearchOption.AllDirectories);
        Assert.DoesNotContain(outputFiles, f => Path.GetFileName(f).StartsWith("Atc.SourceGenerators", StringComparison.Ordinal));
    }

    private List<string> GetPackageEntries()
    {
        using var archive = ZipFile.OpenRead(fixture.PackagePath);
        return archive.Entries.Select(e => e.FullName).ToList();
    }

    private async Task<string> CreateConsumerProjectAsync()
    {
        var directory = Path.Combine(fixture.WorkDirectory, "consumer");
        Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(
            Path.Combine(directory, "nuget.config"),
            $"""
             <?xml version="1.0" encoding="utf-8"?>
             <configuration>
               <packageSources>
                 <clear />
                 <add key="local" value="{fixture.PackageDirectory}" />
               </packageSources>
             </configuration>
             """,
            TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(directory, "Consumer.csproj"),
            $"""
             <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                 <OutputType>Exe</OutputType>
                 <TargetFramework>net10.0</TargetFramework>
                 <Nullable>enable</Nullable>
                 <ImplicitUsings>enable</ImplicitUsings>
                 <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
                 <RestorePackagesPath>{Path.Combine(fixture.WorkDirectory, "nuget-cache")}</RestorePackagesPath>
               </PropertyGroup>
               <ItemGroup>
                 <PackageReference Include="Atc.SourceGenerators" Version="{PackageFixture.PackageVersion}" />
               </ItemGroup>
             </Project>
             """,
            TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(directory, "Models.cs"),
            """
            using System.ComponentModel.DataAnnotations;
            using Atc.SourceGenerators.Annotations;

            namespace SmokeTest;

            [MapTo(typeof(UserStatusDto))]
            public enum UserStatus
            {
                None,
                Active,
            }

            public enum UserStatusDto
            {
                Unknown,
                Active,
            }

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public int Id { get; set; }

                [StringLength(50)]
                public string Name { get; set; } = string.Empty;

                public UserStatus Status { get; set; }
            }

            public class UserDto
            {
                public int Id { get; set; }

                public string Name { get; set; } = string.Empty;

                public UserStatusDto Status { get; set; }
            }
            """,
            TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(directory, "Program.cs"),
            """
            using Atc.Mapping;
            using SmokeTest;

            var user = new User { Id = 7, Name = "Ada", Status = UserStatus.Active };
            var dto = user.MapToUserDto();

            Console.WriteLine($"{dto.Id}|{dto.Name}|{dto.Status}|{AnnotationConstants.User.Name.MaximumLength}");
            """,
            TestContext.Current.CancellationToken);

        return directory;
    }
}