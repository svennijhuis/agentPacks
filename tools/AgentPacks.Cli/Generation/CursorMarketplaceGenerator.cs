using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Builds the repo marketplace Cursor discovers at .cursor-plugin/marketplace.json. Cursor
/// Plugins load components from each plugin directory (and from that plugin's
/// .cursor-plugin/plugin.json); this catalog only lists the plugins so Team Marketplace
/// "Import from Repo" and the public submit flow see the full set.
/// </summary>
internal sealed class CursorMarketplaceGenerator(RepositoryContext context)
{
    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var entries = new JsonArray();

        foreach (var plugin in plugins.Where(p => p.Manifest is not null))
        {
            var manifest = plugin.Manifest!;
            var name = plugin.Name ?? plugin.DirectoryName;

            entries.Add(new JsonObject
            {
                ["name"] = name,
                ["source"] = $"./plugins/{plugin.DirectoryName}",
                ["description"] = manifest["description"]?.GetValue<string>() ?? name
            });
        }

        var marketplace = new JsonObject
        {
            ["name"] = ClaudeCompatGenerator.MarketplaceName,
            ["owner"] = new JsonObject { ["name"] = "agentPacks Maintainers" },
            ["metadata"] = new JsonObject
            {
                ["description"] = "agentPacks plugins generated for Cursor."
            },
            ["plugins"] = entries
        };

        return [GeneratedFile.FromJson(context.CursorMarketplaceRelativePath, marketplace)];
    }
}
