using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Shared catalog shell for Cursor and Copilot marketplace JSON. Claude keeps a top-level
/// description; Codex uses <c>interface.displayName</c> and a local-source object.
/// </summary>
internal static class MarketplaceCatalog
{
    public const string OwnerName = "agentPacks Maintainers";

    public static GeneratedFile Metadata(
        string relativePath,
        string description,
        IReadOnlyList<PluginPackage> plugins,
        Func<PluginPackage, JsonObject> buildEntry)
    {
        var entries = new JsonArray();

        foreach (var plugin in plugins.Where(p => p.Manifest is not null))
        {
            entries.Add(buildEntry(plugin));
        }

        return GeneratedFile.FromJson(relativePath, new JsonObject
        {
            ["name"] = ClaudeCompatGenerator.MarketplaceName,
            ["owner"] = new JsonObject { ["name"] = OwnerName },
            ["metadata"] = new JsonObject { ["description"] = description },
            ["plugins"] = entries
        });
    }
}
