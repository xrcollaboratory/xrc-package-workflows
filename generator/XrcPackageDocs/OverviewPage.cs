using System.Text;

namespace XrcPackageDocs;

/// <summary>The overview page, Documentation~/index.md: the template templates/index.md filled from package.json.</summary>
internal static class OverviewPage
{
    private const string k_XrcPrefix = "edu.cornell.xrc.";
    private const string k_UnityPrefix = "com.unity.";
    private static readonly string[] k_ImageExtensions = [".gif", ".png", ".jpg", ".jpeg"];

    /// <param name="components">The components page's entries: <c>label</c>, <c>anchor</c> and <c>summary</c>.</param>
    /// <param name="isPrivate">Whether the package repository is private.</param>
    public static void Write(PackageManifest package, string docsFolder, IReadOnlyList<IReadOnlyDictionary<string, object?>> components, bool isPrivate, Report report)
    {
        var dependencies = package.Dependencies.Select(Dependency).ToList();
        var samples = (package.Samples ?? []).Select(s => (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?>
        {
            ["displayName"] = s.DisplayName,
            ["anchor"] = Anchor(s.DisplayName),
            ["description"] = s.Description,
        }).ToList();
        var values = new Dictionary<string, object?>
        {
            ["name"] = package.Name,
            ["displayName"] = package.DisplayName,
            ["description"] = package.Description,
            ["version"] = package.Version,
            ["unity"] = package.Unity,
            ["repositoryUrl"] = package.RepositoryUrl,
            ["documentationUrl"] = package.DocumentationUrl,
            ["author"] = package.AuthorName,
            ["private"] = isPrivate,
            ["overviewImage"] = OverviewImage(package, docsFolder),
            ["components"] = components,
            ["samples"] = samples,
            ["dependencies"] = dependencies,
            // XRC packages come from Git, which the Package Manager does not fetch; everything else from a registry.
            ["xrcDependencies"] = dependencies.Where(d => IsXrc(d)).ToList(),
            ["unityDependencies"] = dependencies.Where(d => !IsXrc(d)).ToList(),
        };

        Directory.CreateDirectory(docsFolder);
        var path = Path.Combine(docsFolder, "index.md");
        File.WriteAllText(path, Template.Render("index.md", values), new UTF8Encoding(false));
        report.Wrote(path);
    }

    // A dependency with the links that follow from its name: XRC packages live on GitHub under xrcollaboratory and
    // publish their documentation on GitHub Pages; Unity packages have a manual per minor version.
    private static IReadOnlyDictionary<string, object?> Dependency(KeyValuePair<string, string> dependency)
    {
        var (name, version) = dependency;
        var gitUrl = "";
        var documentationUrl = "";
        if (name.StartsWith(k_XrcPrefix, StringComparison.Ordinal))
        {
            gitUrl = "https://github.com/xrcollaboratory/" + name + ".git";
            documentationUrl = "https://xrcollaboratory.github.io/" + name + "/";
        }
        else if (name.StartsWith(k_UnityPrefix, StringComparison.Ordinal))
        {
            var parts = version.Split('.');
            if (parts.Length >= 2)
                documentationUrl = "https://docs.unity3d.com/Packages/" + name + "@" + parts[0] + "." + parts[1] + "/";
        }
        return new Dictionary<string, object?>
        {
            ["name"] = name,
            ["version"] = version,
            ["gitUrl"] = gitUrl,
            ["documentationUrl"] = documentationUrl,
        };
    }

    /// <summary>The id DocFX gives a heading, for a link to it: lower case, punctuation dropped, spaces as hyphens.</summary>
    public static string Anchor(string heading)
    {
        var id = new StringBuilder();
        foreach (var c in heading.ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_')
                id.Append(c);
            else if (c == ' ')
                id.Append('-');
        }
        return id.ToString();
    }

    private static bool IsXrc(IReadOnlyDictionary<string, object?> dependency) =>
        ((string)dependency["name"]!).StartsWith(k_XrcPrefix, StringComparison.Ordinal);

    private static string OverviewImage(PackageManifest package, string docsFolder)
    {
        foreach (var extension in k_ImageExtensions)
        {
            var imageName = package.Name + extension;
            if (File.Exists(Path.Combine(docsFolder, "images", "overview", imageName)))
                return "images/overview/" + imageName;
        }
        return "";
    }
}
