using System.Text.Json.Nodes;
using Company.AI.Tooling.Generation;
using Company.AI.Tooling.Io;
using Company.AI.Tooling.Loading;

namespace Company.AI.Tooling.Validation;

/// <summary>
/// Checks the generated output against what the target clients accept. This is where portable-but-
/// incompatible input is caught: Agent Plugins permits periods in a plugin name, so "acme.tools"
/// is a legal plugin that a Claude or Copilot marketplace would reject. Failing here beats failing
/// on a developer's machine at install time.
/// </summary>
internal sealed class CompatibilityValidator(RepositoryContext context)
{
    public void Validate(IReadOnlyList<GeneratedFile> files)
    {
        foreach (var file in files)
        {
            if (Path.GetFileName(file.RelativePath) == ".mcp.json")
            {
                ValidateGeneratedMcp(file);
            }
            else
            {
                ValidateMarketplace(file);
            }
        }
    }

    private void ValidateMarketplace(GeneratedFile file)
    {
        var path = file.RelativePath.Replace('\\', '/');

        if (file.Content is not JsonObject marketplace)
        {
            context.Diagnostics.Policy(path, "generated marketplace must be a JSON object.");
            return;
        }

        if (string.IsNullOrWhiteSpace(marketplace["name"]?.GetValue<string>()))
        {
            context.Diagnostics.Policy(path, "generated marketplace must define 'name'.");
        }

        if (marketplace["owner"] is not JsonObject)
        {
            context.Diagnostics.Policy(path, "generated marketplace must define an 'owner' object.");
        }

        if (marketplace["plugins"] is not JsonArray plugins)
        {
            context.Diagnostics.Policy(path, "generated marketplace must define a 'plugins' array.");
            return;
        }

        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in plugins)
        {
            if (node is JsonObject entry)
            {
                ValidateEntry(entry, path, seen);
            }
        }
    }

    private void ValidateEntry(JsonObject entry, string path, Dictionary<string, string> seen)
    {
        var name = entry["name"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            context.Diagnostics.Policy(path, "every generated plugin entry must define 'name'.");
            return;
        }

        if (!AgentPluginSpec.MarketplaceSafeName.IsMatch(name))
        {
            context.Diagnostics.Policy(
                path,
                $"plugin name '{name}' is valid for Agent Plugins but not for a Claude or Copilot " +
                "marketplace, which require kebab-case names without periods. Rename the plugin.");
        }

        if (seen.TryGetValue(name, out var existing))
        {
            context.Diagnostics.Policy(
                path, $"plugin names '{existing}' and '{name}' collide after normalization.");
        }
        else
        {
            seen[name] = name;
        }

        if (entry["version"] is not null)
        {
            context.Diagnostics.Policy(
                path,
                $"plugin entry '{name}' declares 'version'. It must be omitted so update detection " +
                "falls back to the Git commit SHA.");
        }

        if (entry["author"] is { } author && author is not JsonObject)
        {
            context.Diagnostics.Policy(path, $"plugin entry '{name}' must express 'author' as an object.");
        }

        ValidateSource(entry, name, path);
        ValidateComponentPaths(entry, name, path);
    }

    private void ValidateSource(JsonObject entry, string name, string path)
    {
        var source = entry["source"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(source))
        {
            context.Diagnostics.Policy(path, $"plugin entry '{name}' must define 'source'.");
            return;
        }

        if (!source.StartsWith("./", StringComparison.Ordinal))
        {
            context.Diagnostics.Policy(
                path, $"plugin entry '{name}' source '{source}' must be repository-relative and start with './'.");
            return;
        }

        if (PathUtils.HasTraversalSegment(source))
        {
            context.Diagnostics.Policy(
                path, $"plugin entry '{name}' source '{source}' must not traverse outside the repository.");
            return;
        }

        if (!Directory.Exists(Path.Combine(context.Root, source[2..])))
        {
            context.Diagnostics.Policy(
                path, $"plugin entry '{name}' source '{source}' does not exist.");
        }
    }

    private void ValidateComponentPaths(JsonObject entry, string name, string path)
    {
        foreach (var field in (string[])["skills", "agents", "mcpServers"])
        {
            if (entry[field]?.GetValue<string>() is not { } value)
            {
                continue;
            }

            if (!value.StartsWith("./", StringComparison.Ordinal) || PathUtils.HasTraversalSegment(value))
            {
                context.Diagnostics.Policy(
                    path,
                    $"plugin entry '{name}' {field} path '{value}' must be plugin-relative and must not traverse.");
            }
        }
    }

    /// <summary>The derived .mcp.json must still be a usable Claude MCP document.</summary>
    private void ValidateGeneratedMcp(GeneratedFile file)
    {
        var path = file.RelativePath.Replace('\\', '/');

        if (file.Content is not JsonObject document || document["mcpServers"] is not JsonObject servers)
        {
            context.Diagnostics.Policy(path, "generated MCP config must contain an 'mcpServers' object.");
            return;
        }

        foreach (var entry in servers)
        {
            if (entry.Value is not JsonObject server)
            {
                context.Diagnostics.Policy(path, $"server '{entry.Key}' must be an object.");
                continue;
            }

            var type = server["type"]?.GetValue<string>();

            if (type is not ("stdio" or "http" or "sse"))
            {
                context.Diagnostics.Policy(
                    path, $"server '{entry.Key}' has transport '{type}', which Claude does not accept.");
            }

            if (type == "stdio" && server["command"] is null)
            {
                context.Diagnostics.Policy(path, $"stdio server '{entry.Key}' is missing 'command'.");
            }

            if (type is "http" or "sse" && server["url"] is null)
            {
                context.Diagnostics.Policy(path, $"remote server '{entry.Key}' is missing 'url'.");
            }
        }
    }
}
