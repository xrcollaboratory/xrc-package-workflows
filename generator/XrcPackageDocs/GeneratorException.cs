namespace XrcPackageDocs;

/// <summary>An error that stops the generation, reported as one line without a stack trace.</summary>
internal sealed class GeneratorException(string message) : Exception(message);
