namespace AgentPacks.Cli.Loading;

/// <summary>
/// Cross-language standards live once under <c>shared/standards/</c> at the repository root. A
/// pack catalog references one by its repository-relative path (<c>shared/standards/http-api.md</c>)
/// instead of keeping a copy, so the only copies that ever exist are the generated
/// <c>references/standards/</c> files inside consuming skills.
/// </summary>
internal static class SharedStandards
{
    public const string DirectoryRelative = "shared/standards";

    private const string Prefix = DirectoryRelative + "/";

    /// <summary>True when a catalog document path points at the shared directory.</summary>
    public static bool IsSharedPath(string catalogPath) =>
        catalogPath.Replace('\\', '/').StartsWith(Prefix, StringComparison.Ordinal);

    public static string Directory(RepositoryContext context) =>
        Path.Combine(context.Root, "shared", "standards");

    /// <summary>
    /// Resolves a catalog document path to an absolute file: shared paths from the repository
    /// root, everything else from the plugin directory.
    /// </summary>
    public static string Resolve(RepositoryContext context, PluginPackage plugin, string catalogPath) =>
        IsSharedPath(catalogPath)
            ? Path.GetFullPath(Path.Combine(context.Root, catalogPath))
            : Path.GetFullPath(Path.Combine(plugin.Directory, catalogPath));

    /// <summary>Filenames authored under <c>shared/standards/</c>; empty when the directory is absent.</summary>
    public static IReadOnlySet<string> FileNames(RepositoryContext context)
    {
        var directory = Directory(context);

        if (!System.IO.Directory.Exists(directory))
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return System.IO.Directory.GetFiles(directory, "*.md")
            .Select(path => Path.GetFileName(path)!)
            .ToHashSet(StringComparer.Ordinal);
    }
}
