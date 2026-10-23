using System.Text.Json;
using System.Text.Json.Nodes;

namespace XrcPackageDocs;

/// <summary>
/// Checks that the committed docfx.json carries the package's own title, footer and site address, so a copied
/// file that still names another package is noticed.
/// </summary>
internal static class DocfxSettings
{
    public static void Check(PackageManifest package, string docsFolder, Report report)
    {
        var path = Path.Combine(docsFolder, "docfx.json");
        if (!File.Exists(path))
        {
            report.Warning("no docfx.json in " + docsFolder, path);
            return;
        }

        JsonNode? json;
        try
        {
            json = JsonNode.Parse(File.ReadAllText(path), documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        }
        catch (JsonException e)
        {
            report.Warning("docfx.json does not parse: " + e.Message, path);
            return;
        }

        var metadata = json?["build"]?["globalMetadata"];
        Compare("_appTitle", metadata?["_appTitle"], package.DisplayName);
        Compare("_appFooter", metadata?["_appFooter"], package.AuthorName);
        Compare("sitemap.baseUrl", json?["build"]?["sitemap"]?["baseUrl"], package.DocumentationUrl);

        void Compare(string setting, JsonNode? actual, string expected)
        {
            var text = actual is JsonValue value && value.TryGetValue<string>(out var s) ? s : "";
            if (expected.Length > 0 && text != expected)
                report.Warning("docfx.json " + setting + " is \"" + text + "\"; package.json says \"" + expected + "\"", path);
        }
    }
}
