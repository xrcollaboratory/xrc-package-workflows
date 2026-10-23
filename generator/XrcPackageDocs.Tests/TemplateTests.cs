using Xunit;

namespace XrcPackageDocs.Tests;

public sealed class TemplateTests
{
    private static Dictionary<string, object?> Values() => new()
    {
        ["name"] = "edu.cornell.xrc.toolkit.example",
        ["displayName"] = "XRC Example",
        ["description"] = "An example.",
        ["version"] = "1.2.3",
        ["unity"] = "6000.0",
        ["repositoryUrl"] = "https://example.invalid/example.git",
        ["overviewImage"] = "",
        ["dependencies"] = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["name"] = "com.unity.inputsystem", ["version"] = "1.14.1" },
            new Dictionary<string, object?> { ["name"] = "com.unity.xr.interaction.toolkit", ["version"] = "3.2.0" },
        },
    };

    [Fact]
    public void FillsTheOverviewAndRepeatsTheDependencies()
    {
        var page = Template.Render("index.md", Values());

        Assert.StartsWith("# XRC Example\n\nAn example.\n\n## Technical details\n", page);
        Assert.Contains("* Version: 1.2.3\n* [Git URL](https://example.invalid/example.git)\n\n### Requirements\n\n", page);
        Assert.Contains("### Dependencies\n\nThe XRC Example package has the following dependencies which are automatically added to your project when installing:\n\n* com.unity.inputsystem@1.14.1\n* com.unity.xr.interaction.toolkit@3.2.0\n\nIf the package has additional Git dependencies", page);
        Assert.DoesNotContain("{{", page);
    }

    [Fact]
    public void SkippedBlocksLeaveNoBlankLine()
    {
        var values = Values();
        values["dependencies"] = new List<IReadOnlyDictionary<string, object?>>();
        values["repositoryUrl"] = "";
        values["description"] = "";

        var page = Template.Render("index.md", values);

        Assert.StartsWith("# XRC Example\n\n## Technical details\n", page);
        Assert.Contains("* Version: 1.2.3\n\n### Requirements\n\n", page);
        Assert.DoesNotContain("### Dependencies", page);
        Assert.Contains("* 6000.0 and later\n\n### Installation\n", page);
    }

    [Fact]
    public void FailsOnAMissingTemplate()
    {
        Assert.Throws<GeneratorException>(() => Template.Render("missing.md", Values()));
    }
}
