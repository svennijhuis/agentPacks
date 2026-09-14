using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Copies canonical standards into the references directory of each consuming skill. The catalog
/// is the authored interface; duplicated marketplace files are implementation detail. Shared
/// documents under <c>shared/standards/</c> expand into the language packs and win when a skill
/// reference is generated.
/// </summary>
internal sealed class StandardsGenerator(RepositoryContext? context = null)
{
    private static readonly string[] LanguagePacks = ["dotnet", "rust", "typescript"];

    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var files = new List<GeneratedFile>();
        files.AddRange(EmitSharedIntoLanguagePacks(plugins));

        foreach (var plugin in plugins.Where(p => p.Standards is not null))
        {
            Generate(plugin, files);
        }

        return files;
    }

    private IEnumerable<GeneratedFile> EmitSharedIntoLanguagePacks(IReadOnlyList<PluginPackage> plugins)
    {
        if (context is null)
        {
            yield break;
        }

        var sharedDirectory = Path.Combine(context.Root, "shared", "standards");

        if (!Directory.Exists(sharedDirectory))
        {
            yield break;
        }

        var present = plugins
            .Select(plugin => plugin.DirectoryName)
            .ToHashSet(StringComparer.Ordinal);

        var sources = Directory.GetFiles(sharedDirectory, "*.md")
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        foreach (var pack in LanguagePacks)
        {
            if (!present.Contains(pack))
            {
                continue;
            }

            foreach (var source in sources)
            {
                var destination = Path.Combine("plugins", pack, "standards", Path.GetFileName(source));
                yield return new GeneratedFile(destination, TextFile.ReadNormalized(source));
            }
        }
    }

    private void Generate(PluginPackage plugin, List<GeneratedFile> files)
    {
        var catalog = plugin.Standards!.Document;

        if (catalog["documents"] is not JsonObject documents ||
            catalog["consumers"] is not JsonObject consumers)
        {
            return;
        }

        foreach (var (consumer, node) in consumers.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (node is not JsonArray ids)
            {
                continue;
            }

            foreach (var idNode in ids)
            {
                if (idNode is not JsonValue idValue ||
                    !idValue.TryGetValue<string>(out var id) ||
                    documents[id] is not JsonValue pathValue ||
                    !pathValue.TryGetValue<string>(out var sourceRelative))
                {
                    continue;
                }

                var (source, canonicalRelative) = ResolveCanonical(plugin, sourceRelative);

                if (source is null || canonicalRelative is null)
                {
                    continue;
                }

                var destination = Path.Combine(
                    "plugins",
                    plugin.DirectoryName,
                    "skills",
                    consumer,
                    "references",
                    "standards",
                    $"{id}.md");

                var header =
                    $"<!-- Generated from {canonicalRelative} via {PluginLoader.StandardsFileName}. " +
                    "Edit the canonical document, not this copy. -->\n\n";

                files.Add(new GeneratedFile(destination, header + TextFile.ReadNormalized(source), false));
            }
        }
    }

    private (string? Source, string? CanonicalRelative) ResolveCanonical(
        PluginPackage plugin,
        string sourceRelative)
    {
        var fileName = Path.GetFileName(sourceRelative);

        if (context is not null)
        {
            var shared = Path.Combine(context.Root, "shared", "standards", fileName);

            if (File.Exists(shared))
            {
                return (shared, context.Relative(shared));
            }
        }

        var source = Path.Combine(plugin.Directory, sourceRelative);

        if (!File.Exists(source))
        {
            return (null, null);
        }

        return (source, PluginRelative(plugin, source));
    }

    private static string PluginRelative(PluginPackage plugin, string path) =>
        Path.GetRelativePath(plugin.Directory, path).Replace('\\', '/');
}
