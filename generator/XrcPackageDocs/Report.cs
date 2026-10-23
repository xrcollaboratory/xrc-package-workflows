namespace XrcPackageDocs;

/// <summary>
/// Prints the result lines: <c>wrote</c>, <c>MISSING</c> and <c>WARNING</c>, and the same as GitHub Actions
/// annotations on the source line when asked, so a missing description shows up on the file in the workflow run.
/// </summary>
internal sealed class Report(bool annotations, string packageRoot, TextWriter output)
{
    public void Wrote(string path)
    {
        output.WriteLine("wrote " + Relative(path));
    }

    public void Missing(string message, string? file = null, int line = 0)
    {
        output.WriteLine("MISSING " + message);
        Annotate(message, file, line);
    }

    public void Warning(string message, string? file = null, int line = 0)
    {
        output.WriteLine("WARNING " + message);
        Annotate(message, file, line);
    }

    public void Info(string message)
    {
        output.WriteLine(message);
    }

    private void Annotate(string message, string? file, int line)
    {
        if (!annotations)
            return;
        var location = file == null ? "" : " file=" + Relative(file) + (line > 0 ? ",line=" + line : "");
        output.WriteLine("::warning" + location + "::" + message);
    }

    // Paths inside the package are printed relative to it, with forward slashes, as GitHub annotations need them.
    private string Relative(string path)
    {
        var full = Path.GetFullPath(path);
        var relative = full.StartsWith(packageRoot, StringComparison.OrdinalIgnoreCase) ? Path.GetRelativePath(packageRoot, full) : path;
        return relative.Replace(Path.DirectorySeparatorChar, '/');
    }
}
