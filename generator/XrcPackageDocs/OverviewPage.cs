using System.Text;

namespace XrcPackageDocs;

/// <summary>The overview page, Documentation~/index.md, from package.json.</summary>
internal static class OverviewPage
{
    private static readonly string[] k_ImageExtensions = [".gif", ".png", ".jpg", ".jpeg"];

    public static void Write(PackageManifest package, string docsFolder, Report report)
    {
        var markdown = new StringBuilder();
        markdown.Append("# " + package.DisplayName + "\n\n");

        foreach (var extension in k_ImageExtensions)
        {
            var imageName = package.Name + extension;
            if (File.Exists(Path.Combine(docsFolder, "images", "overview", imageName)))
            {
                markdown.Append("![" + package.Name + "](images/overview/" + imageName + ")\n\n");
                break;
            }
        }

        if (package.Description.Length > 0)
            markdown.Append(package.Description + "\n\n");

        markdown.Append("## Technical details\n\n");
        markdown.Append("### Package\n\n");
        markdown.Append("* Name: " + package.Name + "\n");
        markdown.Append("* Display name: " + package.DisplayName + "\n");
        markdown.Append("* Version: " + package.Version + "\n");
        if (package.RepositoryUrl.Length > 0)
            markdown.Append("* [Git URL](" + package.RepositoryUrl + ")\n");
        markdown.Append("\n");

        markdown.Append("### Requirements\n\n");
        if (package.Unity.Length > 0)
        {
            markdown.Append("This version of the " + package.DisplayName + " package is compatible with the following versions of the Unity Editor:\n\n");
            markdown.Append("* " + package.Unity + " and later\n\n");
        }

        if (package.Dependencies.Count > 0)
        {
            markdown.Append("### Dependencies\n\n");
            markdown.Append("The " + package.DisplayName + " package has the following dependencies which are automatically added to your project when installing:\n\n");
            foreach (var (dependency, version) in package.Dependencies)
                markdown.Append("* " + dependency + "@" + version + "\n");
            markdown.Append("\nIf the package has additional Git dependencies listed in the package description, such as XRC Toolkit packages, then they need to be installed manually via the Unity Package Manager.\n\n");
        }

        markdown.Append("### Installation\n\n");
        markdown.Append("To install the package, follow these steps:\n");
        markdown.Append("1. In the Unity Editor, click on **Window > Package Manager**\n");
        markdown.Append("2. Click the + button and choose **Add package from git URL** option\n");
        markdown.Append("3. Enter the package's Git URL. Make sure the URL has \".git\" ending.\n");
        markdown.Append("4. If the repository is private, you will be asked to authenticate via your GitHub account. If you haven't been granted access to the repository you will not be able to install the package.\n");
        markdown.Append("5. The package should be installed into your project.\n");
        markdown.Append("6. You can download the package samples from under the Samples tab in the Package Manager\n\n");
        markdown.Append("From Unity: [Install a UPM package from a Git URL](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html)\n\n");
        markdown.Append("> [!NOTE]\n");
        markdown.Append("> Even though the package documentation is public, the package itself and its repository might be private and not accessible. If you haven't been granted access to the repository you will not be able to install the package.\n\n");

        Directory.CreateDirectory(docsFolder);
        var path = Path.Combine(docsFolder, "index.md");
        File.WriteAllText(path, markdown.ToString(), new UTF8Encoding(false));
        report.Wrote(path);
    }
}
