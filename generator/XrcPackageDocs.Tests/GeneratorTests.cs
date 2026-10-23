using Xunit;

namespace XrcPackageDocs.Tests;

/// <summary>
/// The fixture is the XRC Documentation Test package, whose components cover the hard cases: a sealed class, an
/// inherited field, a property named differently from its field, a field documented only on itself, a field without
/// any description and a hidden field. The expected pages were checked against the Inspector in the Unity editor.
/// </summary>
public sealed class GeneratorTests : IDisposable
{
    private readonly string m_Package = Path.Combine(Path.GetTempPath(), "xrc-package-docs-tests", Guid.NewGuid().ToString("N"));

    public GeneratorTests()
    {
        CopyDirectory(Fixture("documentationtest"), m_Package);
    }

    public void Dispose()
    {
        Directory.Delete(m_Package, true);
    }

    [Fact]
    public void WritesTheSamePagesAsTheUnityEditorScripts()
    {
        var output = new StringWriter();
        var code = Generator.Run([m_Package, "--annotations"], output, output);

        Assert.Equal(0, code);
        AssertSamePage("index.md");
        AssertSamePage("manual/samples.md");
        AssertSamePage("manual/components.md");
    }

    [Fact]
    public void ReportsTheFieldWithoutADescriptionOnItsSourceLine()
    {
        var output = new StringWriter();
        Generator.Run([m_Package, "--annotations"], output, output);
        var lines = output.ToString().Split('\n').Select(l => l.TrimEnd()).ToList();

        Assert.Contains("MISSING DocumentationTestLogic.m_LogEntries (no public property reads it)", lines);
        Assert.Contains("::warning file=Runtime/DocumentationTestLogic.cs,line=61::DocumentationTestLogic.m_LogEntries (no public property reads it)", lines);
        Assert.Contains("2 components, 6 fields described, 1 missing", lines);
        Assert.DoesNotContain(lines, l => l.StartsWith("WARNING", StringComparison.Ordinal));
    }

    [Fact]
    public void WritesToAnotherFolderWhenAsked()
    {
        var elsewhere = Path.Combine(m_Package, "elsewhere");
        var output = new StringWriter();
        var code = Generator.Run([m_Package, "--out", elsewhere], output, output);

        Assert.Equal(0, code);
        Assert.True(File.Exists(Path.Combine(elsewhere, "index.md")));
        Assert.True(File.Exists(Path.Combine(elsewhere, "manual", "components.md")));
        Assert.False(File.Exists(Path.Combine(m_Package, "Documentation~", "index.md")));
    }

    [Fact]
    public void FailsWithoutAPackageManifest()
    {
        File.Delete(Path.Combine(m_Package, "package.json"));
        var output = new StringWriter();
        var error = new StringWriter();

        Assert.Equal(1, Generator.Run([m_Package], output, error));
        Assert.StartsWith("ERROR: no package.json", error.ToString());
    }

    [Fact]
    public void RejectsBadArguments()
    {
        var error = new StringWriter();
        Assert.Equal(2, Generator.Run([], new StringWriter(), error));
        Assert.StartsWith("Usage:", error.ToString());
    }

    [Fact]
    public void WarnsWhenAPackageHasNoComponents()
    {
        Directory.Delete(Path.Combine(m_Package, "Runtime"), true);
        var output = new StringWriter();
        var code = Generator.Run([m_Package], output, output);

        Assert.Equal(0, code);
        Assert.Contains("WARNING no components found under Runtime/", output.ToString());
        Assert.Contains("This package has no MonoBehaviour components.", File.ReadAllText(Path.Combine(m_Package, "Documentation~", "manual", "components.md")));
    }

    [Fact]
    public void WarnsWhenDocfxSettingsDisagreeWithTheManifest()
    {
        var docfx = Path.Combine(m_Package, "Documentation~", "docfx.json");
        File.WriteAllText(docfx, File.ReadAllText(docfx).Replace("\"_appTitle\": \"XRC Documentation Test\"", "\"_appTitle\": \"Old Title\""));
        var output = new StringWriter();
        Generator.Run([m_Package], output, output);

        Assert.Contains("WARNING docfx.json _appTitle is \"Old Title\"; package.json says \"XRC Documentation Test\"", output.ToString());
    }

    private void AssertSamePage(string page)
    {
        var expected = File.ReadAllText(Path.Combine(Fixture("documentationtest-expected"), page)).ReplaceLineEndings("\n");
        var actual = File.ReadAllText(Path.Combine(m_Package, "Documentation~", page)).ReplaceLineEndings("\n");
        Assert.Equal(expected, actual);
    }

    private static string Fixture(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        foreach (var folder in Directory.GetDirectories(source))
            CopyDirectory(folder, Path.Combine(destination, Path.GetFileName(folder)));
    }
}
