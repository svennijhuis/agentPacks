using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Copies canonical standards into the references directory of each consuming skill. The catalog
/// is the authored interface; duplicated marketplace files are implementation detail.
/// </summary>
internal sealed class StandardsGenerator
{
    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var files = new List<GeneratedFile>();

        foreach (var plugin in plugins.Where(p => p.Standards is not null))
        {
            Generate(plugin, files);
        }

        return files;
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

                var source = Path.Combine(plugin.Directory, sourceRelative);

                if (!File.Exists(source))
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
                    $"<!-- Generated from {PluginRelative(plugin, source)} via {PluginLoader.StandardsFileName}. " +
                    "Edit the canonical document, not this copy. -->\n\n";

                files.Add(new GeneratedFile(destination, header + TextFile.ReadNormalized(source), false));
            }
        }
    }

    private static string PluginRelative(PluginPackage plugin, string path) =>
        Path.GetRelativePath(plugin.Directory, path).Replace('\\', '/');
}
