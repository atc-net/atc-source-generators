namespace Atc.SourceGenerators.Tests.Snapshots;

/// <summary>
/// Compares the complete output of a generator run (every generated file and the generator diagnostics) with the
/// snapshot files in <c>Snapshots/Verified/{snapshot name}/</c>.
/// </summary>
/// <remarks>
/// On a difference the test fails and the new output is written next to the snapshot as <c>*.received.txt</c>
/// (git-ignored). To accept intended changes, run the tests with the environment variable
/// <c>ATC_SG_ACCEPT_SNAPSHOTS=1</c>, which overwrites the snapshots, and review the diff before committing.
/// A missing snapshot is written and the test fails, so new output is always reviewed.
/// </remarks>
internal static class SnapshotAssert
{
    private const string VerifiedSuffix = ".verified.txt";
    private const string ReceivedSuffix = ".received.txt";
    private const string DiagnosticsFileName = "_diagnostics";

    public static void Matches(
        string snapshotDirectory,
        GeneratorDriverRunResult runResult,
        Func<string, bool> includeHintName)
    {
        var actual = GetActualFiles(runResult, includeHintName);
        var accept = string.Equals(Environment.GetEnvironmentVariable("ATC_SG_ACCEPT_SNAPSHOTS"), "1", StringComparison.Ordinal);
        var isNew = !Directory.Exists(snapshotDirectory);

        Directory.CreateDirectory(snapshotDirectory);
        foreach (var received in Directory.GetFiles(snapshotDirectory, "*" + ReceivedSuffix))
        {
            File.Delete(received);
        }

        var expected = Directory
            .GetFiles(snapshotDirectory, "*" + VerifiedSuffix)
            .ToDictionary(
                path => Path.GetFileName(path)[..^VerifiedSuffix.Length],
                path => Normalize(File.ReadAllText(path)),
                StringComparer.Ordinal);

        var differences = new List<string>();
        foreach (var (name, content) in actual)
        {
            if (!expected.TryGetValue(name, out var expectedContent))
            {
                differences.Add($"New file '{name}'");
            }
            else if (!string.Equals(expectedContent, content, StringComparison.Ordinal))
            {
                differences.Add($"'{name}' differs: {DescribeFirstDifference(expectedContent, content)}");
            }
            else
            {
                continue;
            }

            File.WriteAllText(Path.Combine(snapshotDirectory, name + (accept || isNew ? VerifiedSuffix : ReceivedSuffix)), content);
        }

        foreach (var name in expected.Keys.Where(name => !actual.ContainsKey(name)))
        {
            differences.Add($"File '{name}' is no longer generated");
            if (accept)
            {
                File.Delete(Path.Combine(snapshotDirectory, name + VerifiedSuffix));
            }
        }

        if (differences.Count == 0 || accept)
        {
            return;
        }

        Assert.Fail(
            (isNew
                ? $"New snapshot written to {snapshotDirectory}; review and commit it."
                : $"Generated output differs from the snapshot in {snapshotDirectory} (see *.received.txt). " +
                  "If the change is intended, rerun with ATC_SG_ACCEPT_SNAPSHOTS=1.") +
            Environment.NewLine +
            string.Join(Environment.NewLine, differences));
    }

    private static SortedDictionary<string, string> GetActualFiles(
        GeneratorDriverRunResult runResult,
        Func<string, bool> includeHintName)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var source in runResult.Results.SelectMany(r => r.GeneratedSources).Where(s => includeHintName(s.HintName)))
        {
            files[source.HintName] = Normalize(source.SourceText.ToString());
        }

        var diagnostics = runResult.Diagnostics
            .Select(d => $"{d.Id} {d.Severity} {d.Location.GetLineSpan().Span}: {d.GetMessage(CultureInfo.InvariantCulture)}")
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToList();
        files[DiagnosticsFileName] = diagnostics.Count == 0
            ? "(none)\n"
            : string.Join("\n", diagnostics) + "\n";

        return files;
    }

    // Line endings differ with the checkout (raw string literals in the generators), so compare with LF only
    private static string Normalize(string text)
        => text.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string DescribeFirstDifference(
        string expected,
        string actual)
    {
        var expectedLines = expected.Split('\n');
        var actualLines = actual.Split('\n');
        for (var i = 0; i < Math.Max(expectedLines.Length, actualLines.Length); i++)
        {
            var expectedLine = i < expectedLines.Length ? expectedLines[i] : "<end of file>";
            var actualLine = i < actualLines.Length ? actualLines[i] : "<end of file>";
            if (!string.Equals(expectedLine, actualLine, StringComparison.Ordinal))
            {
                return $"line {i + 1}: expected '{expectedLine.Trim()}' but was '{actualLine.Trim()}'";
            }
        }

        return "line endings or whitespace";
    }
}