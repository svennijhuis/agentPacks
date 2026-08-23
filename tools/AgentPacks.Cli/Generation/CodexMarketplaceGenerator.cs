using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Builds the repo marketplace Codex discovers at .agents/plugins/marketplace.json. Component
/// routing belongs to each plugin's .codex-plugin/plugin.json; this catalog only controls discovery,
/// ordering and installation policy.
/// </summary>
internal sealed class CodexMarketplaceGenerator(RepositoryContext context)
{
    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var entries = new JsonArray();

        foreach (var plugin in plugins.Where(p => p.Manifest is not null))
        {
            entries.Add(new JsonObject
            {
                ["name"] = plugin.Name ?? plugin.DirectoryName,
                ["source"] = new JsonObject
                {
                    ["source"] = "local",
                    ["path"] = $"./plugins/{plugin.DirectoryName}"
                },
                ["policy"] = new JsonObject
                {
                    ["installation"] = "AVAILABLE",
                    ["authentication"] = "ON_INSTALL"
                },
                ["category"] = "Productivity"
            });
        }

        var marketplace = new JsonObject
        {
            ["name"] = ClaudeCompatGenerator.MarketplaceName,
            ["interface"] = new JsonObject { ["displayName"] = "agentPacks" },
            ["plugins"] = entries
        };

        return [GeneratedFile.FromJson(context.CodexMarketplaceRelativePath, marketplace)];
    }
}
