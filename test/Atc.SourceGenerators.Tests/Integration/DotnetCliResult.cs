namespace Atc.SourceGenerators.Tests.Integration;

/// <summary>
/// The exit code and output of a dotnet CLI command.
/// </summary>
internal sealed record DotnetCliResult(
    int ExitCode,
    string Stdout,
    string Stderr);