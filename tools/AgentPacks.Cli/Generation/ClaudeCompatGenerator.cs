using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Builds the Claude compatibility layer from the portable source. Two files come out:
/// a thin marketplace catalog, and one .mcp.json per plugin that declares MCP servers.
/// Component routing belongs on each plugin's .claude-plugin/plugin.json; a marketplace
/// hooks path or array never runs. The portable mcp.json stays the only authored copy;
/// .mcp.json is derived from it so Claude auto-loads the converted document.
/// </summary>
internal sealed class ClaudeCompatGenerator(RepositoryContext context)
{
    /// <summary>
    /// Identifier developers type after '@' when installing, e.g. squad@agentpacks.
    /// Marketplace names are kebab-case, so the repository's camel-case name is lowercased here.
    /// </summary>
    public const string MarketplaceName = "agentpacks";

    public IReadOnlyList<GeneratedFile> Generate(IReadOnlyList<PluginPackage> plugins)
    {
        var files = new List<GeneratedFile>();
        var entries = new JsonArray();

        foreach (var plugin in plugins.Where(p => p.Manifest is not null))
        {
            entries.Add(BuildEntry(plugin, files));
        }

        // Claude's catalog description is top-level. metadata.description still parses as a
        // compatibility fallback, but `claude plugin validate` warns "No marketplace
        // description provided" when the top-level field is missing. Official
        // claude-plugins-official uses the same shape. Cursor/Copilot keep metadata.description.
        var marketplace = new JsonObject
        {
            ["name"] = MarketplaceName,
            ["owner"] = new JsonObject { ["name"] = MarketplaceCatalog.OwnerName },
            ["description"] = "Private agentPacks plugins. Generated from the portable Agent Plugins source.",
            ["plugins"] = entries
        };

        files.Add(GeneratedFile.FromJson(context.MarketplaceRelativePath, marketplace));

        return files
            .OrderBy(f => f.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private JsonObject BuildEntry(PluginPackage plugin, List<GeneratedFile> files)
    {
        var manifest = plugin.Manifest!;
        var name = plugin.CanonicalName;

        var entry = new JsonObject
        {
            ["name"] = name,
            // Claude Code v2.1.143+ shows this in the plugin picker. Install still uses kebab-case
            // name (squad@agentpacks). Copilot's catalog schema has no display-name field.
            ["displayName"] = PluginDisplayName.From(plugin),
            ["source"] = $"./plugins/{plugin.DirectoryName}",
            ["description"] = manifest["description"]?.GetValue<string>() ?? name
        };

        // "version" is deliberately not copied. Claude resolves updates from an explicit version
        // before falling back to the Git commit SHA, so a version that is never bumped would keep
        // installs pinned to cached content even after skills change.
        JsonFile.CopyProperties(entry, manifest, "author", "homepage", "repository", "license", "keywords");

        if (plugin.McpServers is { } servers)
        {
            var mcpFile = new JsonObject
            {
                ["mcpServers"] = ClaudeValueConverter.ConvertServers(servers)
            };

            files.Add(GeneratedFile.FromJson(
                Path.Combine("plugins", plugin.DirectoryName, ".mcp.json"),
                mcpFile));
        }

        return entry;
    }
}
