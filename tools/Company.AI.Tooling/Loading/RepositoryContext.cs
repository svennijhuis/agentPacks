using Company.AI.Tooling.Io;
using Company.AI.Tooling.Validation;

namespace Company.AI.Tooling.Loading;

/// <summary>Thrown only for conditions that make the run impossible, never for a finding.</summary>
internal sealed class FatalToolingException(string message) : Exception(message);

internal sealed class RepositoryContext
{
    public required string Root { get; init; }

    public string PluginsRoot => Path.Combine(Root, "plugins");

    public string SchemasRoot => Path.Combine(Root, "schemas");

    public string ExternalSourcesPath => Path.Combine(Root, "external", "sources.json");

    public string MarketplaceRelativePath => Path.Combine(".claude-plugin", "marketplace.json");

    public DiagnosticCollector Diagnostics { get; } = new();

    public string Relative(string path) => PathUtils.Relative(Root, path);
}

internal static class RepositoryLocator
{
    /// <summary>Walks up from the working directory looking for the repository markers.</summary>
    public static RepositoryContext Locate(string? startDirectory = null)
    {
        var current = new DirectoryInfo(startDirectory ?? Directory.GetCurrentDirectory());

        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "plugins")) &&
                Directory.Exists(Path.Combine(current.FullName, "tools")))
            {
                return new RepositoryContext { Root = current.FullName };
            }

            current = current.Parent;
        }

        throw new FatalToolingException(
            "Could not find a repository root containing both plugins/ and tools/.");
    }
}
