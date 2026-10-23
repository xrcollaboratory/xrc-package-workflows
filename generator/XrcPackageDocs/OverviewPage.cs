using System.Text;

namespace XrcPackageDocs;

/// <summary>The overview page, Documentation~/index.md: the template Templates/index.md filled from package.json.</summary>
internal static class OverviewPage
{
    private static readonly string[] k_ImageExtensions = [".gif", ".png", ".jpg", ".jpeg"];

    public static void Write(PackageManifest package, string docsFolder, Report report)
    {
        var values = new Dictionary<string, object?>
        {
            ["name"] = package.Name,
            ["displayName"] = package.DisplayName,
            ["description"] = package.Description,
            ["version"] = package.Version,
            ["unity"] = package.Unity,
            ["repositoryUrl"] = package.RepositoryUrl,
            ["author"] = package.AuthorName,
            ["overviewImage"] = OverviewImage(package, docsFolder),
            ["dependencies"] = package.Dependencies
                .Select(d => (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?> { ["name"] = d.Key, ["version"] = d.Value })
                .ToList(),
        };

        Directory.CreateDirectory(docsFolder);
        var path = Path.Combine(docsFolder, "index.md");
        File.WriteAllText(path, Template.Render("index.md", values), new UTF8Encoding(false));
        report.Wrote(path);
    }

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
