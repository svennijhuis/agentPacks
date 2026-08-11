using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Importing;

/// <summary>
/// Materializes URL-backed skills into the portable plugin. GitHub Actions owns the output;
/// contributors edit only the owning plugin's external-skills.json.
/// </summary>
internal sealed class ExternalSourceMaterializer(RepositoryContext context)
{
    public void Materialize(IReadOnlyList<ExternalSourceEntry> sources)
    {
        foreach (var source in sources)
        {
            if (ResolveTarget(source) is not { } target)
            {
                continue;
            }

            if (Directory.Exists(target) && !IsGeneratedDirectory(target))
            {
                context.Diagnostics.Policy(
                    context.Relative(target),
                    $"cannot import external source '{source.Name}' over an authored skill.");
                continue;
            }

            var fetched = GitSourceFetcher.Fetch(source.Repository, source.Path, source.Commit);
            if (!fetched.Success)
            {
                context.Diagnostics.Policy(
                    context.Relative(source.SourceFile),
                    $"source '{source.Name}' could not be fetched: {fetched.Error}");
                continue;
            }

            try
            {
                if (!File.Exists(Path.Combine(fetched.Directory!, "SKILL.md")))
                {
                    context.Diagnostics.Policy(
                        context.Relative(source.SourceFile),
                        $"source '{source.Name}' has no SKILL.md at '{source.Path}'.");
                    continue;
                }

                if (FindSymlink(fetched.Directory!) is { } symlink)
                {
                    context.Diagnostics.Policy(
                        context.Relative(source.SourceFile),
                        $"source '{source.Name}' contains unsupported symlink '{symlink}'.");
                    continue;
                }

                Replace(fetched.Directory!, target);
                WriteMarker(source, target);
                Console.WriteLine($"Materialized {source.Name} at {source.Commit[..7]}.");
            }
            finally
            {
                GitSourceFetcher.Discard(fetched.Directory);
            }
        }

        RemoveStaleGeneratedDirectories(sources);
    }

    public void Check(IReadOnlyList<ExternalSourceEntry> sources)
    {
        foreach (var source in sources)
        {
            if (ResolveTarget(source) is not { } target)
            {
                continue;
            }

            var markerPath = Path.Combine(target, ExternalSourceMarker.FileName);
            var marker = File.Exists(markerPath)
                ? ExternalSourceMarker.FromJson(JsonFile.TryRead(markerPath, out _))
                : null;

            if (marker is null)
            {
                context.Diagnostics.Policy(
                    context.Relative(target),
                    "is not materialized. The GitHub publication workflow must regenerate it.");
                continue;
            }

            if (marker.Repository != source.Repository || marker.Path != source.Path || marker.Commit != source.Commit)
            {
                context.Diagnostics.Policy(context.Relative(target), "does not match its plugin's external-skills.json.");
                continue;
            }

            if (marker.ContentHash != ExternalSourceMarker.HashDirectory(target))
            {
                context.Diagnostics.Policy(context.Relative(target), "was edited after it was generated.");
            }
        }

        ReportStaleGeneratedDirectories(sources, remove: false);
    }

    private string? ResolveTarget(ExternalSourceEntry source)
    {
        if (source.Name.Length > AgentPluginSpec.SkillNameMaxLength ||
            !AgentPluginSpec.SkillName.IsMatch(source.Name) ||
            Path.IsPathRooted(source.Path) ||
            PathUtils.HasTraversalSegment(source.Path))
        {
            context.Diagnostics.Policy(
                context.Relative(source.SourceFile),
                $"source '{source.Name}' has an unsafe name or repository path.");
            return null;
        }

        return Path.Combine(source.PluginDirectory, "skills", source.Name);
    }

    private void RemoveStaleGeneratedDirectories(IReadOnlyList<ExternalSourceEntry> sources) =>
        ReportStaleGeneratedDirectories(sources, remove: true);

    private void ReportStaleGeneratedDirectories(IReadOnlyList<ExternalSourceEntry> sources, bool remove)
    {
        var expected = sources.Select(ResolveTarget).OfType<string>().ToHashSet(StringComparer.Ordinal);

        foreach (var skillsRoot in Directory.Exists(context.PluginsRoot)
                     ? Directory.GetDirectories(context.PluginsRoot).Select(p => Path.Combine(p, "skills"))
                     : [])
        {
            if (!Directory.Exists(skillsRoot)) continue;

            foreach (var skill in Directory.GetDirectories(skillsRoot).OrderBy(p => p, StringComparer.Ordinal))
            {
                if (!IsGeneratedDirectory(skill) || expected.Contains(skill))
                {
                    continue;
                }

                if (remove)
                {
                    Directory.Delete(skill, recursive: true);
                    Console.WriteLine($"Removed stale generated skill {context.Relative(skill)}.");
                }
                else
                {
                    context.Diagnostics.Policy(
                        context.Relative(skill),
                        "has no entry in its plugin's external-skills.json.");
                }
            }
        }
    }

    private static void Replace(string source, string target)
    {
        if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        Directory.CreateDirectory(target);

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = PathUtils.Relative(source, file);
            if (relative == ".git" || relative.StartsWith(".git/", StringComparison.Ordinal)) continue;

            var destination = Path.Combine(target, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination);
        }
    }

    private static bool IsGeneratedDirectory(string directory) =>
        Directory.Exists(directory) &&
        !File.GetAttributes(directory).HasFlag(FileAttributes.ReparsePoint) &&
        File.Exists(Path.Combine(directory, ExternalSourceMarker.FileName));

    /// <summary>Walk without following directory symlinks; imported packages must be self-contained.</summary>
    private static string? FindSymlink(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.TryPop(out var directory))
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                var attributes = File.GetAttributes(entry);
                if (attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    return PathUtils.Relative(root, entry);
                }

                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    pending.Push(entry);
                }
            }
        }

        return null;
    }

    private static void WriteMarker(ExternalSourceEntry source, string target)
    {
        var marker = new ExternalSourceMarker(
            source.Name,
            source.Repository,
            source.Path,
            source.Commit,
            ExternalSourceMarker.HashDirectory(target));
        JsonFile.Write(Path.Combine(target, ExternalSourceMarker.FileName), marker.ToJson());
    }
}
