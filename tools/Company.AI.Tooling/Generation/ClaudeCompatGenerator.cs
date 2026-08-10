using System.Text.Json.Nodes;
using Company.AI.Tooling.Loading;

namespace Company.AI.Tooling.Generation;

/// <summary>A generated file, identified by its repository-relative path.</summary>
internal sealed record GeneratedFile(string RelativePath, JsonNode Content);

/// <summary>
/// Builds the Claude compatibility layer from the portable source. Two files come out:
/// the private marketplace catalog, and one .mcp.json per plugin that declares MCP servers.
/// The portable mcp.json stays the only authored copy; .mcp.json is derived from it.
/// </summary>
internal sealed class ClaudeCompatGenerator(RepositoryContext context)
{
    /// <summary>
    /// Identifier developers type after '@' when installing, e.g. engineering@agentpacks.
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

        var marketplace = new JsonObject
        {
            ["name"] = MarketplaceName,
            ["owner"] = new JsonObject { ["name"] = "agentPacks Maintainers" },
            ["metadata"] = new JsonObject
            {
                ["description"] = "Private agentPacks plugins. Generated from the portable Agent Plugins source."
            },
            ["plugins"] = entries
        };

        files.Add(new GeneratedFile(context.MarketplaceRelativePath, marketplace));

        return files
            .OrderBy(f => f.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private JsonObject BuildEntry(PluginPackage plugin, List<GeneratedFile> files)
    {
        var manifest = plugin.Manifest!;
        var name = plugin.Name ?? plugin.DirectoryName;

        var entry = new JsonObject
        {
            ["name"] = name,
            ["source"] = $"./plugins/{plugin.DirectoryName}",
            ["description"] = manifest["description"]?.GetValue<string>() ?? name
        };

        // "version" is deliberately not copied. Claude resolves updates from an explicit version
        // before falling back to the Git commit SHA, so a version that is never bumped would keep
        // installs pinned to cached content even after skills change.
        foreach (var field in (string[])["author", "homepage", "repository", "license", "keywords"])
        {
            if (manifest[field] is { } value)
            {
                entry[field] = value.DeepClone();
            }
        }

        if (plugin.HasSkillsDirectory)
        {
            entry["skills"] = "./skills/";
        }

        if (plugin.Mcp?["mcpServers"] is JsonObject servers && servers.Count > 0)
        {
            var mcpFile = new JsonObject
            {
                ["mcpServers"] = ClaudeValueConverter.ConvertServers(servers)
            };

            files.Add(new GeneratedFile(
                Path.Combine("plugins", plugin.DirectoryName, ".mcp.json"),
                mcpFile));

            entry["mcpServers"] = "./.mcp.json";
        }

        // strict:false makes this entry the authority for components. The portable manifest
        // declares no components of its own, so there is nothing for Claude to find in conflict.
        entry["strict"] = false;

        return entry;
    }
}
