using System.Text;

namespace XrcPackageDocs;

/// <summary>The overview page, Documentation~/index.md: the template templates/index.md filled from package.json.</summary>
internal static class OverviewPage
{
    private const string k_XrcPrefix = "edu.cornell.xrc.";
    private const string k_UnityPrefix = "com.unity.";
    private static readonly string[] k_ImageExtensions = [".gif", ".png", ".jpg", ".jpeg"];

    public static void Write(PackageManifest package, string docsFolder, Report report)
    {
        var dependencies = package.Dependencies.Select(Dependency).ToList();
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
            ["overviewImage"] = OverviewImage(package, docsFolder),
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
