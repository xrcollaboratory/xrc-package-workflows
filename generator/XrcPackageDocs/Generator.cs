namespace XrcPackageDocs;

/// <summary>
/// Generates a package's documentation pages from its sources: the overview (index.md) and the samples page from
/// package.json, and the components page from the C# in Runtime/. Nothing is read from a Unity editor.
/// </summary>
internal static class Generator
{
    public const string Usage = "Usage: xrc-package-docs <package root> [--out <Documentation~ folder>] [--annotations]";

    /// <summary>
    /// The command line: the package root, <c>--out</c> for a Documentation~ folder other than the package's own,
    /// and <c>--annotations</c> for GitHub Actions warnings on the source line of each missing description.
    /// </summary>
    /// <returns>0 when the pages were written (missing descriptions are warnings), 1 on an error, 2 on bad arguments.</returns>
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        var arguments = args.ToList();
        var annotations = arguments.Remove("--annotations");
        string? outFolder = null;
        var outIndex = arguments.IndexOf("--out");
        if (outIndex >= 0 && outIndex + 1 < arguments.Count)
        {
            outFolder = arguments[outIndex + 1];
            arguments.RemoveRange(outIndex, 2);
        }
        if (arguments.Count != 1)
        {
            error.WriteLine(Usage);
            return 2;
        }

        var packageRoot = Path.GetFullPath(arguments[0]);
        var docsFolder = Path.GetFullPath(outFolder ?? Path.Combine(packageRoot, "Documentation~"));
        try
        {
            Generate(packageRoot, docsFolder, new Report(annotations, packageRoot, output));
            return 0;
        }
        catch (GeneratorException e)
        {
            error.WriteLine("ERROR: " + e.Message);
            return 1;
        }
    }

    public static void Generate(string packageRoot, string docsFolder, Report report)
    {
        var manifest = PackageManifest.Load(packageRoot);
        OverviewPage.Write(manifest, docsFolder, report);
        SamplesPage.Write(manifest, docsFolder, report);
        ComponentsPage.Write(packageRoot, docsFolder, report);
        DocfxSettings.Check(manifest, docsFolder, report);
    }
}
