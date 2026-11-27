namespace Atc.SourceGenerators.Tests.Integration;

/// <summary>
/// Runs the dotnet CLI for integration tests.
/// </summary>
internal static class DotnetCli
{
    public static string SolutionRoot { get; } = FindSolutionRoot();

    /// <summary>
    /// Runs <c>dotnet</c> with the given arguments, reading stdout and stderr concurrently (reading one to the end
    /// first can deadlock when the other fills its buffer) and killing the process when the timeout is exceeded.
    /// </summary>
    public static async Task<DotnetCliResult> RunAsync(
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var process = new System.Diagnostics.Process();
        process.StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
        };

        process.Start();

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        var stdoutTask = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        var stderrTask = process.StandardError.ReadToEndAsync(timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"'dotnet {arguments}' timed out after {timeout.TotalSeconds} seconds");
        }

        return new DotnetCliResult(process.ExitCode, await stdoutTask, await stderrTask);
    }

    /// <summary>
    /// Asserts that the command succeeded, showing its output when it didn't.
    /// </summary>
    public static void AssertSucceeded(
        DotnetCliResult result,
        string description)
    {
        var message = $"{description} failed with exit code {result.ExitCode}.{Environment.NewLine}" +
                      $"--- stdout ---{Environment.NewLine}{result.Stdout}{Environment.NewLine}" +
                      $"--- stderr ---{Environment.NewLine}{result.Stderr}";
        Assert.True(result.ExitCode == 0, message);
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