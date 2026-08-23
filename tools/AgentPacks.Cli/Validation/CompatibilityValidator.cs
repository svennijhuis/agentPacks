using System.Text.Json.Nodes;
using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Validation;

/// <summary>
/// Checks the generated output against what the target clients accept. This is where portable-but-
/// incompatible input is caught: Agent Plugins permits periods in a plugin name, so "acme.tools"
/// is a legal plugin that provider marketplaces reject. Failing here beats failing on a
/// developer's machine at install time.
/// </summary>
internal sealed class CompatibilityValidator(RepositoryContext context)
{
    public void Validate(IReadOnlyList<GeneratedFile> files)
    {
        foreach (var file in files)
        {
            var relative = file.RelativePath.Replace('\\', '/');

            if (Path.GetFileName(relative) == ".mcp.json")
            {
                ValidateGeneratedMcp(file);
            }
            else if (relative.EndsWith("hooks/hooks.json", StringComparison.Ordinal))
            {
                ValidateGeneratedHooks(file);
            }
            else if (string.Equals(relative, context.CodexMarketplaceRelativePath.Replace('\\', '/'), StringComparison.Ordinal))
            {
                ValidateCodexMarketplace(file);
            }
            else if (string.Equals(relative, context.MarketplaceRelativePath.Replace('\\', '/'), StringComparison.Ordinal)
                     || string.Equals(relative, context.CopilotMarketplaceRelativePath.Replace('\\', '/'), StringComparison.Ordinal))
            {
                ValidateMarketplace(file);
            }
        }
    }

    private void ValidateCodexMarketplace(GeneratedFile file)
    {
        var path = file.RelativePath.Replace('\\', '/');

        if (file.Content is not JsonObject marketplace ||
            string.IsNullOrWhiteSpace(marketplace["name"]?.GetValue<string>()) ||
            marketplace["interface"]?["displayName"] is null ||
            marketplace["plugins"] is not JsonArray plugins)
        {
            context.Diagnostics.Policy(
                path,
                "generated Codex marketplace must define name, interface.displayName and plugins.");
            return;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in plugins.OfType<JsonObject>())
        {
            var name = entry["name"]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(name) || !seen.Add(name))
            {
                context.Diagnostics.Policy(path, "every Codex plugin entry must have a unique non-empty name.");
                continue;
            }

            if (entry["source"] is not JsonObject source ||
                source["source"]?.GetValue<string>() != "local" ||
                string.IsNullOrWhiteSpace(source["path"]?.GetValue<string>()))
            {
                context.Diagnostics.Policy(path, $"Codex plugin entry '{name}' must define a local source path.");
            }
            else
            {
                ValidateLocalPath(source["path"]!.GetValue<string>(), name, path);
            }

            if (entry["policy"]?["installation"] is null || entry["policy"]?["authentication"] is null)
            {
                context.Diagnostics.Policy(path, $"Codex plugin entry '{name}' must define its install and auth policy.");
            }

            if (string.IsNullOrWhiteSpace(entry["category"]?.GetValue<string>()))
            {
                context.Diagnostics.Policy(path, $"Codex plugin entry '{name}' must define a category.");
            }
        }
    }

    private void ValidateLocalPath(string source, string name, string path)
    {
        if (!source.StartsWith("./", StringComparison.Ordinal) || PathUtils.HasTraversalSegment(source))
        {
            context.Diagnostics.Policy(
                path, $"Codex plugin entry '{name}' source '{source}' must stay inside the marketplace root.");
            return;
        }

        if (!Directory.Exists(Path.Combine(context.Root, source[2..])))
        {
            context.Diagnostics.Policy(path, $"Codex plugin entry '{name}' source '{source}' does not exist.");
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
                $"plugin name '{name}' is valid for Agent Plugins but not for the generated provider " +
                "marketplaces, which require kebab-case names without periods. Rename the plugin.");
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
        // An external skill is referenced, not shipped: its source is an object naming the
        // upstream repository and the exact commit to fetch.
        if (entry["source"] is JsonObject remote)
        {
            ValidateRemoteSource(remote, name, path);
            return;
        }

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

    /// <summary>
    /// Git-backed sources must pin an exact commit. A branch or tag can move under us, which is
    /// the whole reason external content is reviewed once and pinned.
    /// </summary>
    private void ValidateRemoteSource(JsonObject remote, string name, string path)
    {
        var kind = remote["source"]?.GetValue<string>();

        if (kind is not ("git-subdir" or "github" or "url"))
        {
            context.Diagnostics.Policy(
                path, $"external entry '{name}' has source type '{kind}', which clients do not fetch by git.");
            return;
        }

        if (string.IsNullOrWhiteSpace(remote["url"]?.GetValue<string>()) &&
            string.IsNullOrWhiteSpace(remote["repo"]?.GetValue<string>()))
        {
            context.Diagnostics.Policy(path, $"external entry '{name}' must name the repository to fetch.");
        }

        if (kind == "git-subdir" && string.IsNullOrWhiteSpace(remote["path"]?.GetValue<string>()))
        {
            context.Diagnostics.Policy(path, $"external entry '{name}' must give the subdirectory to fetch.");
        }

        var sha = remote["sha"]?.GetValue<string>();

        if (sha is null || !AgentPluginSpec.GitCommitSha.IsMatch(sha))
        {
            context.Diagnostics.Policy(
                path,
                $"external entry '{name}' must pin a full 40-character commit SHA. " +
                "A branch or tag would let reviewed content change underneath us.");
        }
    }

    private void ValidateComponentPaths(JsonObject entry, string name, string path)
    {
        // Claude expresses skills and hooks as a single path and agents and commands as arrays of
        // them, so both node kinds are unwrapped before the same rules are applied.
        foreach (var field in (string[])["skills", "mcpServers", "hooks", "agents", "commands"])
        {
            foreach (var value in ComponentPaths(entry[field]))
            {
                if (!value.StartsWith("./", StringComparison.Ordinal) || PathUtils.HasTraversalSegment(value))
                {
                    context.Diagnostics.Policy(
                        path,
                        $"plugin entry '{name}' {field} path '{value}' must be plugin-relative and must not traverse.");
                }
            }
        }
    }

    private static IEnumerable<string> ComponentPaths(JsonNode? node) => node switch
    {
        JsonArray array => array.Select(item => item?.GetValue<string>()).OfType<string>(),
        null => [],
        _ => node.GetValueKind() == System.Text.Json.JsonValueKind.String
            ? [node.GetValue<string>()]
            : []
    };

    /// <summary>
    /// A generated hooks file runs commands on a developer's machine, so the last check before it
    /// is published is that every entry actually names one — a hook with no command is a client
    /// error at session start, and one that reaches outside the plugin should never have been
    /// generated in the first place.
    /// </summary>
    private void ValidateGeneratedHooks(GeneratedFile file)
    {
        var path = file.RelativePath.Replace('\\', '/');

        if (file.Content is not JsonObject document || document["hooks"] is not JsonObject events)
        {
            context.Diagnostics.Policy(path, "generated hooks config must contain a 'hooks' object.");
            return;
        }

        foreach (var (name, node) in events)
        {
            if (node is not JsonArray entries || entries.Count == 0)
            {
                context.Diagnostics.Policy(path, $"event '{name}' must hold at least one hook.");
                continue;
            }

            foreach (var command in entries.OfType<JsonObject>().SelectMany(Commands))
            {
                if (string.IsNullOrWhiteSpace(command))
                {
                    context.Diagnostics.Policy(path, $"a hook on '{name}' generated an empty command.");
                    continue;
                }

                if (PathUtils.HasTraversalSegment(command))
                {
                    context.Diagnostics.Policy(
                        path, $"a hook on '{name}' generated a command that traverses outside the plugin: {command}");
                }
            }
        }
    }

    /// <summary>
    /// Commands in an entry, flattening the nested dialects into the flat one. The POSIX command
    /// is required and reported when missing, but its key differs by client — Copilot reads
    /// "bash" where the others read "command" — so an entry qualifies on whichever it carries.
    /// The per-OS keys are optional and only checked when present.
    /// </summary>
    private static IEnumerable<string?> Commands(JsonObject entry)
    {
        var sources = entry["hooks"] is JsonArray nested
            ? nested.OfType<JsonObject>()
            : [entry];

        foreach (var source in sources)
        {
            yield return (source["command"] ?? source["bash"])?.GetValue<string>();

            foreach (var key in (string[])["commandWindows", "powershell"])
            {
                if (source[key] is { } windows)
                {
                    yield return windows.GetValue<string>();
                }
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
