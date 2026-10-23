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
            ["private"] = true,
            ["components"] = new List<IReadOnlyDictionary<string, object?>>
            {
                new Dictionary<string, object?> { ["label"] = "Example Logic", ["anchor"] = "example-logic", ["summary"] = "Does the example." },
                new Dictionary<string, object?> { ["label"] = "Example Input", ["anchor"] = "example-input", ["summary"] = "" },
            },
            ["samples"] = new List<IReadOnlyDictionary<string, object?>>
            {
                new Dictionary<string, object?> { ["displayName"] = "XRC Example", ["anchor"] = "xrc-example", ["description"] = "Shows the example." },
            },
            ["dependencies"] = unity.Concat(xrc).ToList(),
            ["unityDependencies"] = unity,
            ["xrcDependencies"] = xrc,
        };
    }

    [Fact]
    public void FillsTheOverviewAndListsBothKindsOfDependencies()
    {
        var page = Template.Render("index.md", Values());

        Assert.StartsWith("# XRC Example\n\nAn example.\n\n* Name: `edu.cornell.xrc.toolkit.example`\n* Display name: XRC Example\n* Version: 1.2.3 ([changelog](changelog/CHANGELOG.md))\n* [Repository](https://example.invalid/example.git)\n* [License](license/LICENSE.md)\n\nThe package's components are described on the [Components](manual/components.md) page, and its samples on the [Samples](manual/samples.md) page.\n\n## Installation\n\nThis version of the package was developed and tested with Unity 6000.0.\n\n", page);
        Assert.Contains("* [edu.cornell.xrc.toolkit.core](https://xrcollaboratory.github.io/edu.cornell.xrc.toolkit.core/) 0.4.4 or later: [https://github.com/xrcollaboratory/edu.cornell.xrc.toolkit.core.git](https://github.com/xrcollaboratory/edu.cornell.xrc.toolkit.core.git)\n", page);
        Assert.Contains("* [com.unity.inputsystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/) 1.14.1\n* com.example.registry 2.0.0\n", page);
        Assert.Contains("and enter [https://example.invalid/example.git](https://example.invalid/example.git).\n", page);
        Assert.Contains("> [!NOTE]\n> This documentation is public, but the package is not.", page);
        Assert.EndsWith("the Package Manager cannot download the package.\n\n", page);
        Assert.DoesNotContain("{{", page);
    }

    [Fact]
    public void LeavesOutThePrivateNoteForAPublicPackage()
    {
        var values = Values();
        values["private"] = false;

        var page = Template.Render("index.md", values);

        Assert.DoesNotContain("[!NOTE]", page);
        Assert.EndsWith("The samples are under the Samples tab of the package.\n\n", page);
    }

    [Fact]
    public void SkippedBlocksLeaveNoBlankLine()
    {
        var values = Values();
        values["unityDependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["xrcDependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["components"] = new List<IReadOnlyDictionary<string, object?>>();
        values["samples"] = new List<IReadOnlyDictionary<string, object?>>();
        values["description"] = "";
        values["unity"] = "";

        var page = Template.Render("index.md", values);

        Assert.StartsWith("# XRC Example\n\n* Name: `edu.cornell.xrc.toolkit.example`\n", page);
        Assert.Contains("* [License](license/LICENSE.md)\n\nThe package's components are described on the [Components](manual/components.md) page, and its samples on the [Samples](manual/samples.md) page.\n\n## Installation\n\nIn the Package Manager, choose", page);
        Assert.DoesNotContain("installed automatically", page);
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
