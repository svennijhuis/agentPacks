namespace AgentPacks.Cli.Io;

internal static class PathUtils
{
    /// <summary>Repo-relative, forward-slashed path for diagnostics.</summary>
    public static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    /// <summary>
    /// True when <paramref name="candidate"/> resolves inside <paramref name="root"/>.
    /// Resolves symlinks, so a link pointing outside the package is caught — the spec requires
    /// packaged files to resolve within the plugin root.
    /// </summary>
    public static bool ResolvesWithin(string root, string candidate)
    {
        var resolvedRoot = ResolveRealPath(root);
        var resolvedCandidate = ResolveRealPath(candidate);

        if (resolvedRoot is null || resolvedCandidate is null)
        {
            return false;
        }

        var prefix = resolvedRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        return string.Equals(resolvedCandidate, resolvedRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.Ordinal)
            || resolvedCandidate.StartsWith(prefix, StringComparison.Ordinal);
    }

    /// <summary>Fully resolved path with symlinks followed, or null when it cannot be resolved.</summary>
    public static string? ResolveRealPath(string path)
    {
        try
        {
            var full = Path.GetFullPath(path);
            var info = Directory.Exists(full) ? new DirectoryInfo(full) : (FileSystemInfo)new FileInfo(full);
            var target = info.ResolveLinkTarget(returnFinalTarget: true);

            return Path.GetFullPath(target?.FullName ?? full);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return null;
        }
    }

    /// <summary>True when any segment of a plugin-relative path is <c>..</c>.</summary>
    public static bool HasTraversalSegment(string value)
    {
        var withoutPlaceholder = value
            .Replace("${PLUGIN_ROOT}", string.Empty, StringComparison.Ordinal)
            .Replace("${PLUGIN_DATA}", string.Empty, StringComparison.Ordinal);

        return withoutPlaceholder
            .Split('/', '\\')
            .Any(segment => segment == "..");
    }
}
