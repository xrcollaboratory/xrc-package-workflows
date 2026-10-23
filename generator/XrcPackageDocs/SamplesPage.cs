using System.Text;

namespace XrcPackageDocs;

/// <summary>The samples page, Documentation~/manual/samples.md, from package.json.</summary>
internal static class SamplesPage
{
    private static readonly string[] k_ImageExtensions = [".gif", ".png", ".jpg", ".jpeg"];

    public static void Write(PackageManifest package, string docsFolder, Report report)
    {
        if (package.Samples == null)
        {
            report.Info("No samples array found in package.json");
            return;
        }

        var sections = new List<string>();
        foreach (var sample in package.Samples)
        {
            if (sample.Description.Length == 0)
                report.Warning("Sample \"" + sample.DisplayName + "\" has no description.", "package.json");

            var lines = new List<string> { "## " + sample.DisplayName, "", sample.Description, "" };
            foreach (var image in SampleImages(docsFolder, sample.DisplayName))
            {
                lines.Add("![" + sample.DisplayName + "](" + image + ")");
                lines.Add("");
            }
            sections.Add(string.Join("\n", lines));
        }

        var markdown = new List<string>
        {
            "# Samples",
            "",
            "This document lists all available samples in the package.",
            "",
        };
        markdown.AddRange(sections);

        var manual = Path.Combine(docsFolder, "manual");
        Directory.CreateDirectory(manual);
        var path = Path.Combine(manual, "samples.md");
        File.WriteAllText(path, string.Join("\n", markdown), new UTF8Encoding(false));
        report.Wrote(path);
    }

    private static IEnumerable<string> SampleImages(string docsFolder, string sampleName)
    {
        var folder = Path.Combine(docsFolder, "images", "samples", sampleName);
        if (!Directory.Exists(folder))
            return [];
        return Directory.GetFiles(folder)
            .Select(file => Path.GetFileName(file))
            .Where(file => k_ImageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
            .OrderBy(file => file, StringComparer.Ordinal)
            .Select(file => "../images/samples/" + UriComponent.Encode(sampleName) + "/" + UriComponent.Encode(file));
    }
}

/// <summary>JavaScript's encodeURIComponent, which is how the image links are encoded.</summary>
internal static class UriComponent
{
    private const string k_Unreserved = "-_.!~*'()";

    public static string Encode(string text)
    {
        var result = new StringBuilder();
        foreach (var b in Encoding.UTF8.GetBytes(text))
        {
            var c = (char)b;
            if (b < 128 && (char.IsAsciiLetterOrDigit(c) || k_Unreserved.Contains(c)))
                result.Append(c);
            else
                result.Append('%').Append(b.ToString("X2"));
        }
        return result.ToString();
    }
}
