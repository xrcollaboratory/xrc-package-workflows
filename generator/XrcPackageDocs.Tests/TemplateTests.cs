using Xunit;

namespace XrcPackageDocs.Tests;

public sealed class TemplateTests
{
    private static Dictionary<string, object?> Dependency(string name, string version, string gitUrl, string documentationUrl) => new()
    {
        ["name"] = name,
        ["version"] = version,
        ["gitUrl"] = gitUrl,
        ["documentationUrl"] = documentationUrl,
    };

    private static Dictionary<string, object?> Values()
    {
        var unity = new List<IReadOnlyDictionary<string, object?>>
        {
            Dependency("com.unity.inputsystem", "1.14.1", "", "https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/"),
            Dependency("com.example.registry", "2.0.0", "", ""),
        };
        var xrc = new List<IReadOnlyDictionary<string, object?>>
        {
            Dependency("edu.cornell.xrc.toolkit.core", "0.4.4", "https://github.com/xrcollaboratory/edu.cornell.xrc.toolkit.core.git", "https://xrcollaboratory.github.io/edu.cornell.xrc.toolkit.core/"),
        };
        return new Dictionary<string, object?>
        {
            ["name"] = "edu.cornell.xrc.toolkit.example",
            ["displayName"] = "XRC Example",
            ["description"] = "An example.",
            ["version"] = "1.2.3",
            ["unity"] = "6000.0",
            ["repositoryUrl"] = "https://example.invalid/example.git",
            ["documentationUrl"] = "https://xrcollaboratory.github.io/edu.cornell.xrc.toolkit.example/",
            ["overviewImage"] = "",
            ["dependencies"] = unity.Concat(xrc).ToList(),
            ["unityDependencies"] = unity,
            ["xrcDependencies"] = xrc,
        };
    }

    [Fact]
    public void FillsTheOverviewAndListsBothKindsOfDependencies()
    {
        var page = Template.Render("index.md", Values());

        Assert.StartsWith("# XRC Example\n\nAn example.\n\n## Technical details\n", page);
        Assert.Contains("* Version: 1.2.3\n* [Git URL](https://example.invalid/example.git)\n\n### Requirements\n\n", page);
        Assert.Contains("* [com.unity.inputsystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/) 1.14.1\n* com.example.registry 2.0.0\n", page);
        Assert.Contains("* [edu.cornell.xrc.toolkit.core](https://xrcollaboratory.github.io/edu.cornell.xrc.toolkit.core/) 0.4.4 or later: `https://github.com/xrcollaboratory/edu.cornell.xrc.toolkit.core.git`\n", page);
        Assert.DoesNotContain("{{", page);
    }

    [Fact]
    public void SkippedBlocksLeaveNoBlankLine()
    {
        var values = Values();
        values["unityDependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["xrcDependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["repositoryUrl"] = "";
        values["description"] = "";

        var page = Template.Render("index.md", values);

        Assert.StartsWith("# XRC Example\n\n## Technical details\n", page);
        Assert.Contains("* Version: 1.2.3\n\n### Requirements\n\n", page);
        Assert.DoesNotContain("ependencies", page);
        Assert.Contains("* 6000.0 and later\n\n### Installation\n", page);
    }

    [Fact]
    public void FailsOnAMissingTemplate()
    {
        Assert.Throws<GeneratorException>(() => Template.Render("missing.md", Values()));
    }

    [Fact]
    public void PrefersATemplateFolderOverTheBuiltInCopy()
    {
        var folder = Path.Combine(Path.GetTempPath(), "xrc-package-docs-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "index.md"), "# {{displayName}} from the folder\n{{#unless overviewImage}}no image{{/unless}}\n");
        try
        {
            Template.Folder = folder;
            Assert.Equal("# XRC Example from the folder\nno image\n", Template.Render("index.md", Values()));
            Assert.Throws<GeneratorException>(() => Template.Render("other.md", Values()));
        }
        finally
        {
            Template.Folder = null;
            Directory.Delete(folder, true);
        }
    }
}
