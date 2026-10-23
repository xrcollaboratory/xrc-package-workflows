using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace XrcPackageDocs;

/// <summary>
/// Turns the XML of a <c>&lt;summary&gt;</c> into Markdown:
/// <c>see</c>, <c>c</c>, <c>b</c>, <c>i</c>, <c>para</c>, <c>list</c> and <c>code</c>, with lists and paragraph
/// breaks written as HTML inside a table cell, where Markdown has no way to write them.
/// </summary>
internal static class DocComments
{
    public static string ToMarkdown(XmlNode node, bool inTable)
    {
        var text = new StringBuilder();
        Append(node, text, inTable);
        // Collapse the source's line breaks and indentation; keep the paragraph breaks <para> and lists made.
        var paragraphs = text.ToString().Split("\n\n")
            .Select(p => string.Join("\n", p.Split('\n').Select(l => Regex.Replace(l, @"[ \t\r]+", " ").Trim())
                .Where(l => l.Length > 0)
                .Aggregate(new List<string>(), (acc, l) =>
                {
                    if (acc.Count > 0 && !l.StartsWith("* ", StringComparison.Ordinal) && !Regex.IsMatch(l, @"^\d+\. "))
                        acc[^1] += " " + l;
                    else
                        acc.Add(l);
                    return acc;
                })))
            .Where(p => p.Length > 0);
        return string.Join(inTable ? "<br><br>" : "\n\n", paragraphs).Replace("\n", inTable ? "<br>" : "\n");
    }

    private static void Append(XmlNode node, StringBuilder text, bool inTable)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    text.Append(inTable ? child.Value!.Replace("|", "\\|") : child.Value);
                    continue;
                case XmlNodeType.Element:
                    break;
                default:
                    continue;
            }

            switch (child.Name)
            {
                case "see":
                case "seealso":
                    var cref = child.Attributes?["cref"]?.Value;
                    var langword = child.Attributes?["langword"]?.Value;
                    var href = child.Attributes?["href"]?.Value;
                    if (child.InnerText.Trim().Length > 0)
                        text.Append(href != null ? "[" + child.InnerText.Trim() + "](" + href + ")" : "`" + child.InnerText.Trim() + "`");
                    else if (cref != null)
                        text.Append("`" + ShortCref(cref) + "`");
                    else if (langword != null)
                        text.Append("`" + langword + "`");
                    else if (href != null)
                        text.Append(href);
                    break;
                case "paramref":
                case "typeparamref":
                    text.Append("`" + child.Attributes?["name"]?.Value + "`");
                    break;
                case "c":
                    text.Append("`" + child.InnerText.Trim() + "`");
                    break;
                case "b":
                    text.Append("**");
                    Append(child, text, inTable);
                    text.Append("**");
                    break;
                case "i":
                    text.Append("*");
                    Append(child, text, inTable);
                    text.Append("*");
                    break;
                case "para":
                    text.Append("\n\n");
                    Append(child, text, inTable);
                    text.Append("\n\n");
                    break;
                case "list":
                    var numbered = child.Attributes?["type"]?.Value == "number";
                    var index = 1;
                    // Markdown lists cannot sit in a table cell, so a list there is written as HTML on one line.
                    text.Append(inTable ? (numbered ? "<ol>" : "<ul>") : "\n");
                    foreach (XmlNode item in child.SelectNodes("item")!)
                    {
                        var itemText = new StringBuilder();
                        var term = item.SelectSingleNode("term");
                        var description = item.SelectSingleNode("description");
                        if (term != null)
                        {
                            itemText.Append("**");
                            Append(term, itemText, inTable);
                            itemText.Append("**");
                            if (description != null)
                                itemText.Append(": ");
                        }
                        Append(description ?? item, itemText, inTable);
                        var itemLine = Regex.Replace(itemText.ToString(), @"\s+", " ").Trim();
                        if (inTable)
                            text.Append("<li>" + itemLine + "</li>");
                        else
                            text.Append((numbered ? index++ + ". " : "* ") + itemLine + "\n");
                    }
                    text.Append(inTable ? (numbered ? "</ol>" : "</ul>") : "\n");
                    break;
                case "code":
                    text.Append(inTable ? "`" + child.InnerText.Trim() + "`" : "\n\n```\n" + child.InnerText.Trim() + "\n```\n\n");
                    break;
                default:
                    Append(child, text, inTable);
                    break;
            }
        }
    }

    // "P:XRC.Toolkit.SphereSelect.SphereSelectLogic.radius" -> "SphereSelectLogic.radius"; a type -> its name;
    // an unresolved reference ("!:name") -> the name as written.
    private static string ShortCref(string cref)
    {
        var kind = cref.Length > 2 && cref[1] == ':' ? cref[0] : 'T';
        var name = cref.Length > 2 && cref[1] == ':' ? cref[2..] : cref;
        if (kind == '!')
            return name;
        var paren = name.IndexOf('(');
        if (paren >= 0)
            name = name[..paren];
        var parts = name.Split('.');
        return kind == 'T' || parts.Length < 2 ? parts[^1] : parts[^2] + "." + parts[^1];
    }
}
