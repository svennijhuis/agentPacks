using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Builds the repo-root catalog Cursor Team Marketplace import and public submit read at
/// .cursor-plugin/marketplace.json. Official Cursor catalogs are a thin name/source/description
/// list (schemas/marketplace.schema.json, additionalProperties: false). Component routing belongs
/// on each plugin's .cursor-plugin/plugin.json, not on this catalog.
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

        // Official cursor/plugins catalogs write repo-relative paths without "./"
        // ("teaching", "third_party/gmail"). Extra entry fields (author, keywords, version)
        // fail the published schema.
        return new JsonObject
        {
            ["name"] = name,
            ["source"] = $"plugins/{plugin.DirectoryName}",
            ["description"] = manifest["description"]?.GetValue<string>() ?? name
        };
    }
}
