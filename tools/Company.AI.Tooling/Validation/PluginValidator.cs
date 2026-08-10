using System.Text.Json.Nodes;
using Company.AI.Tooling.Io;
using Company.AI.Tooling.Loading;

namespace Company.AI.Tooling.Validation;

/// <summary>
/// Manifest semantics the JSON Schema cannot express: cross-file agreement, repository-wide
/// uniqueness, filesystem containment, and the external source policy.
/// </summary>
internal sealed class PluginValidator(RepositoryContext context, SchemaValidator schemaValidator)
{
    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        var seenNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in plugins)
        {
            ValidatePlugin(plugin, seenNames);
        }

        ValidateExternalSources();
    }

    private void ValidatePlugin(PluginPackage plugin, Dictionary<string, string> seenNames)
    {
        string? manifestSchema = null;

        if (plugin.Manifest is not null)
        {
            manifestSchema = schemaValidator.Validate(
                plugin.Manifest, plugin.ManifestPath, AgentPluginSpec.PluginSchemaUrl);

            ValidateName(plugin, seenNames);
        }

        if (plugin.Mcp is not null && plugin.McpPath is not null)
        {
            var mcpSchema = schemaValidator.Validate(
                plugin.Mcp, plugin.McpPath, AgentPluginSpec.McpSchemaUrl);

            ValidateSchemaVersionsMatch(plugin, manifestSchema, mcpSchema);
        }

        ValidatePackageBoundary(plugin);
    }

    private void ValidateName(PluginPackage plugin, Dictionary<string, string> seenNames)
    {
        // Syntax is the schema's job. Uniqueness across the repository is ours.
        if (plugin.Name is not { } name)
        {
            return;
        }

        if (seenNames.TryGetValue(name, out var firstDirectory))
        {
            context.Diagnostics.Policy(
                context.Relative(plugin.ManifestPath),
                $"plugin name '{name}' is already used by {firstDirectory}. Names must be unique.");
            return;
        }

        seenNames[name] = context.Relative(plugin.Directory);
    }

    private void ValidateSchemaVersionsMatch(PluginPackage plugin, string? manifestSchema, string? mcpSchema)
    {
        var manifestVersion = AgentPluginSpec.SpecVersionOf(manifestSchema);
        var mcpVersion = AgentPluginSpec.SpecVersionOf(mcpSchema);

        if (manifestVersion is null || mcpVersion is null || manifestVersion == mcpVersion)
        {
            return;
        }

        context.Diagnostics.SpecFatal(
            context.Relative(plugin.McpPath!),
            $"targets Agent Plugins {mcpVersion} but plugin.json targets {manifestVersion}. " +
            "Both documents must declare the same specification version.");
    }

    /// <summary>
    /// The spec requires every packaged file to resolve within the plugin root, and forbids using
    /// symlinks to escape it. Walk what we ship and check where it actually lands.
    /// </summary>
    private void ValidatePackageBoundary(PluginPackage plugin)
    {
        IEnumerable<string> entries;

        try
        {
            entries = Directory.EnumerateFileSystemEntries(plugin.Directory, "*", SearchOption.AllDirectories);
        }
        catch (IOException ex)
        {
            context.Diagnostics.SpecFatal(context.Relative(plugin.Directory), $"could not be walked: {ex.Message}");
            return;
        }

        foreach (var entry in entries.OrderBy(e => e, StringComparer.Ordinal))
        {
            if (PathUtils.ResolvesWithin(plugin.Directory, entry))
            {
                continue;
            }

            context.Diagnostics.SpecFatal(
                context.Relative(entry),
                "resolves outside the plugin root. Packaged files must stay inside the package; " +
                "symlinks must not be used to escape it.");
        }
    }

    private void ValidateExternalSources()
    {
        var path = context.ExternalSourcesPath;
        var relative = context.Relative(path);

        if (!File.Exists(path))
        {
            context.Diagnostics.Policy(
                relative,
                "is required. Use {\"sources\": []} when no external sources are approved yet.");
            return;
        }

        var node = JsonFile.TryRead(path, out var error);

        if (node is null)
        {
            context.Diagnostics.Policy(relative, error!);
            return;
        }

        if (node is not JsonObject root || root["sources"] is not JsonArray sources)
        {
            context.Diagnostics.Policy(relative, "must be an object containing a 'sources' array.");
            return;
        }

        var index = 0;

        foreach (var entry in sources)
        {
            var label = $"{relative} sources[{index}]";
            index++;

            if (entry is not JsonObject source)
            {
                context.Diagnostics.Policy(relative, $"sources[{index - 1}] must be an object.");
                continue;
            }

            foreach (var field in (string[])["name", "repository", "path", "license", "commit"])
            {
                if (source[field] is not JsonValue value || value.GetValueKind() != System.Text.Json.JsonValueKind.String
                    || string.IsNullOrWhiteSpace(value.GetValue<string>()))
                {
                    context.Diagnostics.Policy(label, $"must define a non-empty string '{field}'.");
                }
            }

            if (source["commit"]?.GetValue<string>() is { } commit &&
                !AgentPluginSpec.GitCommitSha.IsMatch(commit))
            {
                context.Diagnostics.Policy(
                    label,
                    "must pin an exact 40-character Git commit SHA. Tracking a branch is not allowed.");
            }
        }
    }
}
