using System.Text;

namespace XrcPackageDocs;

/// <summary>Unity's ObjectNames.NicifyVariableName: the label the Inspector shows for a field or class name.</summary>
internal static class Names
{
    public static string Nicify(string name)
    {
        if (name.StartsWith("m_", StringComparison.Ordinal))
            name = name[2..];
        else if (name.StartsWith('_'))
            name = name[1..];
        else if (name.Length > 1 && name[0] == 'k' && char.IsUpper(name[1]))
            name = name[1..];
        if (name.Length == 0)
            return name;

        var result = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c))
            {
                // A new word starts at an upper-case letter after a lower-case letter or digit ("logEntries"), and at
                // the last upper-case letter of a run that is followed by a lower-case one ("UIPress" -> "UI Press").
                var previous = name[i - 1];
                var startsWord = char.IsLower(previous) || char.IsDigit(previous)
                                 || (char.IsUpper(previous) && i + 1 < name.Length && char.IsLower(name[i + 1]));
                if (startsWord)
                    result.Append(' ');
            }
            result.Append(i == 0 ? char.ToUpperInvariant(c) : c);
        }
        return result.ToString();
    }
}
