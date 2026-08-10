using Company.AI.Tooling.Io;
using Company.AI.Tooling.Loading;

namespace Company.AI.Tooling.Vendoring;

/// <summary>
/// Copies reviewed external skills into a plugin's skills/ directory at their pinned commit.
/// <para>
/// Agent Plugins v1 has no import mechanism: skills/ is a fixed location, each skill must be a real
/// directory containing a regular SKILL.md, and symlinks may not escape the package. Vendoring is
/// therefore the only way to share an external skill with every client rather than just the ones
/// whose marketplaces support git sources.
/// </para>
/// </summary>
internal sealed class Vendorer(RepositoryContext context)
{
    /// <summary>Fetches every source and rewrites its vendored directory.</summary>
    public void Vendor(IReadOnlyList<ExternalSourceEntry> sources)
    {
        foreach (var source in sources)
        {
            if (ResolveTarget(source) is not { } target)
            {
                continue;
            }

            var fetched = GitFetcher.Fetch(source.Repository, source.Path, source.Commit);

            if (!fetched.Success)
            {
                context.Diagnostics.Policy(
                    context.Relative(context.ExternalSourcesPath),
                    $"source '{source.Name}' could not be fetched: {fetched.Error}");
                continue;
            }

            try
            {
                Replace(fetched.Directory!, target);
                WriteManifest(source, target);
                Console.WriteLine($"Vendored {source.Name} from {source.Repository} at {source.Commit[..7]}.");
            }
            finally
            {
                GitFetcher.Discard(fetched.Directory);
            }
        }

        ReportOrphans(sources);
    }

    /// <summary>
    /// Verifies vendored content offline: every source is present at the pinned commit, its files
    /// still hash to what was written, and nothing vendored is left behind after a source is removed.
    /// </summary>
    public void Check(IReadOnlyList<ExternalSourceEntry> sources)
    {
        foreach (var source in sources)
        {
            if (ResolveTarget(source) is not { } target)
            {
                continue;
            }

            var relative = context.Relative(target);
            var markerPath = Path.Combine(target, ExternalSources.MarkerFileName);

            if (!Directory.Exists(target) || !File.Exists(markerPath))
            {
                context.Diagnostics.Policy(
                    relative, $"external source '{source.Name}' has not been vendored. Run 'vendor'.");
                continue;
            }

            var manifest = VendorManifest.FromJson(JsonFile.TryRead(markerPath, out _));

            if (manifest is null)
            {
                context.Diagnostics.Policy(relative, $"{ExternalSources.MarkerFileName} is unreadable. Run 'vendor'.");
                continue;
            }

            if (manifest.Commit != source.Commit)
            {
                context.Diagnostics.Policy(
                    relative,
                    $"is vendored from commit {manifest.Commit[..7]} but external/sources.json pins " +
                    $"{source.Commit[..7]}. Run 'vendor'.");
                continue;
            }

            if (VendorManifest.HashDirectory(target) != manifest.ContentHash)
            {
                context.Diagnostics.Policy(
                    relative,
                    "has been modified since it was vendored. Vendored skills are generated content: " +
                    "change them upstream and bump the pinned commit instead.");
            }
        }

        ReportOrphans(sources);
    }

    /// <summary>Vendored directories with no matching entry in external/sources.json.</summary>
    private void ReportOrphans(IReadOnlyList<ExternalSourceEntry> sources)
    {
        var expected = sources
            .Select(ResolveTarget)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        foreach (var pluginDirectory in EnumeratePluginDirectories())
        {
            var skillsRoot = Path.Combine(pluginDirectory, "skills");

            if (!Directory.Exists(skillsRoot))
            {
                continue;
            }

            foreach (var skill in Directory.GetDirectories(skillsRoot).OrderBy(d => d, StringComparer.Ordinal))
            {
                if (!File.Exists(Path.Combine(skill, ExternalSources.MarkerFileName)) || expected.Contains(skill))
                {
                    continue;
                }

                context.Diagnostics.Policy(
                    context.Relative(skill),
                    "is vendored but has no entry in external/sources.json. Restore the entry or delete the directory.");
            }
        }
    }

    private string? ResolveTarget(ExternalSourceEntry source)
    {
        var plugins = EnumeratePluginDirectories();

        if (source.Plugin is { } named)
        {
            var match = plugins.FirstOrDefault(p => Path.GetFileName(p) == named);

            if (match is null)
            {
                context.Diagnostics.Policy(
                    context.Relative(context.ExternalSourcesPath),
                    $"source '{source.Name}' targets plugin '{named}', which does not exist.");

                return null;
            }

            return Path.Combine(match, "skills", source.Name);
        }

        if (plugins.Count == 1)
        {
            return Path.Combine(plugins[0], "skills", source.Name);
        }

        context.Diagnostics.Policy(
            context.Relative(context.ExternalSourcesPath),
            $"source '{source.Name}' must set 'plugin' because the repository contains more than one plugin.");

        return null;
    }

    private List<string> EnumeratePluginDirectories() =>
        Directory.Exists(context.PluginsRoot)
            ? Directory.GetDirectories(context.PluginsRoot).OrderBy(d => d, StringComparer.Ordinal).ToList()
            : [];

    private static void Replace(string source, string target)
    {
        if (Directory.Exists(target))
        {
            Directory.Delete(target, recursive: true);
        }

        Directory.CreateDirectory(target);

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = PathUtils.Relative(source, file);

            // .git metadata belongs to the fetch, not to the skill.
            if (relative.StartsWith(".git/", StringComparison.Ordinal) || relative == ".git")
            {
                continue;
            }

            var destination = Path.Combine(target, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private static void WriteManifest(ExternalSourceEntry source, string target)
    {
        var manifest = new VendorManifest(
            source.Name,
            source.Repository,
            source.Path,
            source.Commit,
            VendorManifest.HashDirectory(target));

        JsonFile.Write(Path.Combine(target, ExternalSources.MarkerFileName), manifest.ToJson());
    }
}
