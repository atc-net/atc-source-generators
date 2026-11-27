namespace Atc.SourceGenerators.Tests.Generators;

public class DiagnosticDescriptorTests
{
    private const string DiagnosticsReferenceUrl = "https://github.com/atc-net/atc-source-generators/wiki/Diagnostics-Reference";

    public static TheoryData<string> DiagnosticIds
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var descriptor in GetAllDescriptors())
            {
                data.Add(descriptor.Id);
            }

            return data;
        }
    }

    [Fact]
    public void Descriptors_Should_Have_Unique_Ids()
    {
        var descriptors = GetAllDescriptors();

        Assert.NotEmpty(descriptors);
        Assert.Equal(
            descriptors.Count,
            descriptors.Select(d => d.Id).Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [MemberData(nameof(DiagnosticIds))]
    public void Descriptor_Should_Link_To_Its_Section_In_The_Diagnostics_Reference(
        string id)
    {
        var descriptor = GetAllDescriptors().Single(d => d.Id == id);

        Assert.Equal(
            $"{DiagnosticsReferenceUrl}#{descriptor.Id.ToLowerInvariant()}",
            descriptor.HelpLinkUri);
    }

    private static List<DiagnosticDescriptor> GetAllDescriptors()
        => typeof(ObjectMappingGenerator).Assembly
            .GetTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .Select(f => (DiagnosticDescriptor)f.GetValue(null))
            .ToList();
}