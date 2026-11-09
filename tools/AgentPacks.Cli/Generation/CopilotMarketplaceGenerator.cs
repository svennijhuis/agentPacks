using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Routes Copilot to its generated namespace. Copilot checks .github/plugin/marketplace.json before
/// the Claude-compatible catalog, so each client receives component paths in its own dialect.
/// <c>strict</c> is omitted: Copilot's documented default is true, and the flag is schema
/// validation, not Claude path-only routing. github/copilot-cli#1513 (honouring false) is open.
/// </summary>
internal sealed class CopilotMarketplaceGenerator(RepositoryContext context)
{
    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins) =>
        [MarketplaceCatalog.Metadata(
            context.CopilotMarketplaceRelativePath,
            "agentPacks plugins generated for GitHub Copilot CLI.",
            plugins,
            BuildEntry)];

    private static JsonObject BuildEntry(PluginPackage plugin)
    {
        var manifest = plugin.Manifest!;
        var entry = new JsonObject
        {
            ["name"] = plugin.CanonicalName,
            ["source"] = $"./plugins/{plugin.DirectoryName}",
            ["description"] = manifest["description"]?.GetValue<string>() ?? plugin.CanonicalName
        };

        JsonFile.CopyProperties(entry, manifest, "author", "homepage", "repository", "license", "keywords");

        var profile = ClientProfile.Copilot;
        PluginComponentRouting.ApplySkillsHooksMcp(entry, plugin, profile);

        if (plugin.Agents.Count > 0)
        {
            entry["agents"] = $"./{profile.Directory}/agents/";
        }

        if (plugin.Commands.Count > 0)
        {
            entry["commands"] = $"./{profile.Directory}/commands/";
        }

        return entry;
    }
}
