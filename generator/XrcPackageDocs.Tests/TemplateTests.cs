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
            ["author"] = "XR Collaboratory",
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

        Assert.StartsWith("# XRC Example\n\nAn example.\n\n* Name: `edu.cornell.xrc.toolkit.example`\n* Display name: XRC Example\n* Version: 1.2.3 ([changelog](changelog/CHANGELOG.md))\n* [Git URL](https://example.invalid/example.git)\n* [License](license/LICENSE.md)\n\nThe package's components are described on the [Components](manual/components.md) page, and its samples on the [Samples](manual/samples.md) page.\n\n## Dependencies\n\nThis version of the package was developed and tested with Unity 6000.0, with the following package dependencies.\n\n### XRC Toolkit packages\n\n", page);
        Assert.Contains("* [edu.cornell.xrc.toolkit.core@0.4.4](https://xrcollaboratory.github.io/edu.cornell.xrc.toolkit.core/) ([Git URL](https://github.com/xrcollaboratory/edu.cornell.xrc.toolkit.core.git))\n\n### Unity packages\n\nThe Package Manager installs these automatically.\n\n* [com.unity.inputsystem@1.14.1](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/)\n* com.example.registry@2.0.0\n\n## Installation\n\nAdd the XRC Toolkit packages listed under [Dependencies](#dependencies) first. Then, in the Package Manager, choose **Add package from git URL** and enter the package's [Git URL](https://example.invalid/example.git).\n\nSee Unity's manual:", page);
        Assert.EndsWith("> [!NOTE]\n> This documentation is public, but the package is not. Its repository is private to XR Collaboratory. Installing the package requires a GitHub account that XR Collaboratory has given access to the repository; without it, the Package Manager cannot download the package.\n", page);
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
        values["dependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["unityDependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["xrcDependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["components"] = new List<IReadOnlyDictionary<string, object?>>();
        values["samples"] = new List<IReadOnlyDictionary<string, object?>>();
        values["description"] = "";
        values["unity"] = "";
        values["private"] = false;

        var page = Template.Render("index.md", values);

        Assert.StartsWith("# XRC Example\n\n* Name: `edu.cornell.xrc.toolkit.example`\n", page);
        Assert.Contains("* [License](license/LICENSE.md)\n\nThe package's components are described on the [Components](manual/components.md) page, and its samples on the [Samples](manual/samples.md) page.\n\n## Dependencies\n\n## Installation\n\nIn the Package Manager, choose", page);
        Assert.EndsWith("The samples are under the Samples tab of the package.\n\n", page);
        Assert.DoesNotContain("### ", page);
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
