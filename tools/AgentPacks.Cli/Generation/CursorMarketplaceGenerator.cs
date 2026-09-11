using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Builds the repo-root catalog Cursor Team Marketplace import and public submit read at
/// .cursor-plugin/marketplace.json. Official Cursor catalogs are a thin name/source/description
/// list; component routing belongs on each plugin's .cursor-plugin/plugin.json.
/// </summary>
internal sealed class CursorMarketplaceGenerator(RepositoryContext context)
{
    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var entries = new JsonArray();

        foreach (var plugin in plugins.Where(p => p.Manifest is not null))
        {
            entries.Add(BuildEntry(plugin));
        }

        var marketplace = new JsonObject
        {
            ["name"] = ClaudeCompatGenerator.MarketplaceName,
            ["owner"] = new JsonObject { ["name"] = "agentPacks Maintainers" },
            ["metadata"] = new JsonObject
            {
                ["description"] = "agentPacks plugins generated for Cursor Team Marketplace import."
            },
            ["plugins"] = entries
        };

        return [GeneratedFile.FromJson(context.CursorMarketplaceRelativePath, marketplace)];
    }

    private static JsonObject BuildEntry(PluginPackage plugin)
    {
        var manifest = plugin.Manifest!;
        var name = plugin.Name ?? plugin.DirectoryName;

        var entry = new JsonObject
        {
            ["name"] = name,
            // Official Cursor catalogs write repo-relative paths without "./"
            // (cursor/plugins uses "teaching" and "third_party/gmail").
            ["source"] = $"plugins/{plugin.DirectoryName}",
            ["description"] = manifest["description"]?.GetValue<string>() ?? name
        };

        foreach (var field in (string[])["author", "homepage", "repository", "license", "keywords"])
        {
            if (manifest[field] is { } value)
            {
                entry[field] = value.DeepClone();
            }
        }

        return entry;
    }
}
