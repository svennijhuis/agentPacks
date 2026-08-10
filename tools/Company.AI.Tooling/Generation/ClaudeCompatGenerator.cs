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
    /// Identifier developers type after '@' when installing, e.g. company-engineering@agentpacks.
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

        foreach (var source in ExternalSources.Load(context))
        {
            entries.Add(BuildExternalEntry(source));
        }

        var marketplace = new JsonObject
        {
            ["name"] = MarketplaceName,
            ["owner"] = new JsonObject { ["name"] = "Company Developer Platform" },
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

    /// <summary>
    /// An external skill enters the catalog by reference, never by copy. Claude fetches the pinned
    /// commit itself with a sparse clone of just that subdirectory, and a directory whose SKILL.md
    /// sits at its root loads as a single skill. The skill's own frontmatter supplies its name and
    /// description upstream; the metadata here is only what the catalog needs to list it.
    /// </summary>
    private static JsonObject BuildExternalEntry(ExternalSourceEntry source)
    {
        var entry = new JsonObject
        {
            ["name"] = source.Name,
            ["source"] = new JsonObject
            {
                ["source"] = "git-subdir",
                ["url"] = source.Repository,
                ["path"] = source.Path,
                ["sha"] = source.Commit
            }
        };

        if (source.Description is { } description)
        {
            entry["description"] = description;
        }

        // No "strict": the upstream directory defines itself, and we add no components to it.
        return entry;
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

        if (plugin.HasAgentsDirectory)
        {
            entry["agents"] = "./agents/";
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
