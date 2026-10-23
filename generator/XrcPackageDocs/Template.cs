using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XrcPackageDocs;

/// <summary>
/// Renders the Markdown templates in <c>Templates/</c>: <c>{{name}}</c> inserts a value, the block between
/// <c>{{#if name}}</c> and <c>{{/if}}</c> is kept only when the value is set, and the block between
/// <c>{{#each name}}</c> and <c>{{/each}}</c> is repeated for each item, with the item's values in scope and the
/// enclosing values behind them. A block tag alone on its line takes the whole line with it, so a skipped block
/// leaves no blank line behind.
/// </summary>
internal static class Template
{
    private static readonly Regex k_StandaloneTag = new(@"^[ \t]*(\{\{[#/](?:if|each)\b[^}]*\}\})[ \t]*(?:\r?\n|$)", RegexOptions.Multiline);

    public static string Render(string templateName, IReadOnlyDictionary<string, object?> values)
    {
        var template = k_StandaloneTag.Replace(Load(templateName), "$1");
        return Render(template, new Scope(values, null));
    }

    private static string Load(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Templates/" + name)
                           ?? throw new GeneratorException("template " + name + " is missing from the generator");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }

    private static string Render(string text, Scope scope)
    {
        var result = new StringBuilder();
        var position = 0;
        while (true)
        {
            var start = text.IndexOf("{{", position, StringComparison.Ordinal);
            if (start < 0)
                break;
            var end = text.IndexOf("}}", start, StringComparison.Ordinal);
            if (end < 0)
                throw new GeneratorException("template tag is not closed: " + text[start..]);
            result.Append(text, position, start - position);

            var tag = text[(start + 2)..end].Trim();
            var afterTag = end + 2;
            if (tag.StartsWith("#if ", StringComparison.Ordinal) || tag.StartsWith("#each ", StringComparison.Ordinal))
            {
                var name = tag[(tag.IndexOf(' ') + 1)..].Trim();
                var (closeStart, closeEnd) = FindClose(text, afterTag, tag);
                var body = text[afterTag..closeStart];
                var value = scope.Lookup(name);
                if (tag.StartsWith("#if", StringComparison.Ordinal))
                {
                    if (IsSet(value))
                        result.Append(Render(body, scope));
                }
                else if (value is IEnumerable<IReadOnlyDictionary<string, object?>> items)
                {
                    foreach (var item in items)
                        result.Append(Render(body, new Scope(item, scope)));
                }
                else if (value != null)
                {
                    throw new GeneratorException("template value " + name + " cannot be repeated with #each");
                }
                position = closeEnd;
            }
            else if (tag.StartsWith('/'))
            {
                throw new GeneratorException("template has a closing tag without an opening one: {{" + tag + "}}");
            }
            else
            {
                result.Append(scope.Lookup(tag) switch
                {
                    string s => s,
                    null => "",
                    var other => other.ToString(),
                });
                position = afterTag;
            }
        }
        result.Append(text, position, text.Length - position);
        return result.ToString();
    }

    // The matching close tag of a block that opened just before `from`, allowing nested blocks.
    private static (int start, int end) FindClose(string text, int from, string openTag)
    {
        var depth = 1;
        var position = from;
        while (depth > 0)
        {
            var start = text.IndexOf("{{", position, StringComparison.Ordinal);
            if (start < 0)
                throw new GeneratorException("template block is not closed: {{" + openTag + "}}");
            var end = text.IndexOf("}}", start, StringComparison.Ordinal) + 2;
            var tag = text[(start + 2)..(end - 2)].Trim();
            if (tag.StartsWith('#'))
                depth++;
            else if (tag.StartsWith('/'))
                depth--;
            if (depth == 0)
                return (start, end);
            position = end;
        }
        throw new InvalidOperationException();
    }

    private static bool IsSet(object? value) => value switch
    {
        null => false,
        string s => s.Length > 0,
        bool b => b,
        IEnumerable<IReadOnlyDictionary<string, object?>> items => items.Any(),
        _ => true,
    };

    private sealed class Scope(IReadOnlyDictionary<string, object?> values, Scope? parent)
    {
        public object? Lookup(string name)
        {
            if (values.TryGetValue(name, out var value))
                return value;
            return parent?.Lookup(name);
        }
    }
}
