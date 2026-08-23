using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Routes Copilot to its generated namespace. Copilot checks .github/plugin/marketplace.json before
/// the Claude-compatible catalog, so each client receives component paths in its own dialect.
/// </summary>
internal sealed class CopilotMarketplaceGenerator(RepositoryContext context)
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
                ["description"] = "agentPacks plugins generated for GitHub Copilot CLI."
            },
            ["plugins"] = entries
        };

        return [GeneratedFile.FromJson(context.CopilotMarketplaceRelativePath, marketplace)];
    }

    private static JsonObject BuildEntry(PluginPackage plugin)
    {
        var manifest = plugin.Manifest!;
        var entry = new JsonObject
        {
            ["name"] = plugin.Name ?? plugin.DirectoryName,
            ["source"] = $"./plugins/{plugin.DirectoryName}",
            ["description"] = manifest["description"]?.GetValue<string>() ?? plugin.DirectoryName,
            ["strict"] = false
        };

        foreach (var field in (string[])["author", "homepage", "repository", "license", "keywords"])
        {
            if (manifest[field] is { } value)
            {
                entry[field] = value.DeepClone();
            }
        }

        var profile = ClientProfile.Copilot;

        if (plugin.HasSkillsDirectory)
        {
            entry["skills"] = "./skills/";
        }

        if (plugin.Agents.Count > 0)
        {
            entry["agents"] = $"./{profile.Directory}/agents/";
        }

        if (plugin.Commands.Count > 0)
        {
            entry["commands"] = $"./{profile.Directory}/commands/";
        }

        // Keyed on what ClientTreeGenerator actually writes: always-on rules become a SessionStart
        // hook for Copilot the same way they do for Claude, so a plugin with no authored hooks but
        // one alwaysApply rule still produces a hooks.json that has to be declared.
        var hasHooks = HookGenerator.Build(plugin, profile) is not null
            || plugin.Rules.Any(r => r.Frontmatter?.Scalar("alwaysApply") == "true");

        if (hasHooks)
        {
            entry["hooks"] = $"./{profile.Directory}/hooks/hooks.json";
        }

        // Keyed on what the generator actually produces, and pointed at the converted copy rather
        // than the authored one: mcp.json is the portable dialect, and .mcp.json is what
        // ClaudeValueConverter rewrites out of it. Declaring the source file here would hand
        // Copilot unexpanded ${PLUGIN_ROOT} values and the portable transport name, and would
        // declare servers for a plugin whose mcpServers object is empty.
        if (plugin.Mcp?["mcpServers"] is JsonObject servers && servers.Count > 0)
        {
            entry["mcpServers"] = "./.mcp.json";
        }

        return entry;
    }
}
