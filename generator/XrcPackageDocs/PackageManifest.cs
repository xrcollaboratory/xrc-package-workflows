using System.Text.Json;
using System.Text.Json.Nodes;

namespace XrcPackageDocs;

/// <summary>The fields of package.json the pages are generated from.</summary>
internal sealed class PackageManifest
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public string Description { get; init; } = "";
    public string Version { get; init; } = "";
    public string Unity { get; init; } = "";
    public string DocumentationUrl { get; init; } = "";
    public string RepositoryUrl { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public IReadOnlyList<KeyValuePair<string, string>> Dependencies { get; init; } = [];
    public IReadOnlyList<Sample>? Samples { get; init; }

    public sealed record Sample(string DisplayName, string Description, string Path);

    public static PackageManifest Load(string packageRoot)
    {
        var path = Path.Combine(packageRoot, "package.json");
        if (!File.Exists(path))
            throw new GeneratorException("no package.json in " + packageRoot);

        JsonObject json;
        try
        {
            json = JsonNode.Parse(File.ReadAllText(path), documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject
                   ?? throw new GeneratorException("package.json is not a JSON object");
        }
        catch (JsonException e)
        {
            throw new GeneratorException("package.json does not parse: " + e.Message);
        }

        var name = Text(json["name"]);
        if (name.Length == 0)
            throw new GeneratorException("package.json has no name");

        var repository = json["repository"];
        var author = json["author"];
        var dependencies = json["dependencies"] as JsonObject;
        var samples = json["samples"] as JsonArray;

        return new PackageManifest
        {
            Name = name,
            DisplayName = Text(json["displayName"]).Length > 0 ? Text(json["displayName"]) : name,
            Description = Text(json["description"]),
            Version = Text(json["version"]),
            Unity = Text(json["unity"]),
            DocumentationUrl = Text(json["documentationUrl"]),
            RepositoryUrl = repository is JsonValue ? Text(repository) : Text(repository?["url"]),
            AuthorName = author is JsonValue ? Text(author) : Text(author?["name"]),
            Dependencies = dependencies == null
                ? []
                : dependencies.Select(d => new KeyValuePair<string, string>(d.Key, Text(d.Value))).ToList(),
            Samples = samples?.Select(s => new Sample(Text(s?["displayName"]), Text(s?["description"]), Text(s?["path"]))).ToList(),
        };
    }

    private static string Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : "";
}
