using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Validation;

/// <summary>Validates the small authored interface that fans standards into skill references.</summary>
internal sealed class StandardsValidator(RepositoryContext context)
{
    private const string SchemaPath = "../../schema/standards.schema.json";
    private static readonly IReadOnlySet<string> TopLevelKeys =
        new HashSet<string>(StringComparer.Ordinal) { "$schema", "version", "documents", "consumers" };

    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        foreach (var plugin in plugins.Where(p => p.Standards is not null))
        {
            Validate(plugin);
        }
    }

    private void Validate(PluginPackage plugin)
    {
        var definition = plugin.Standards!;
        var document = definition.Document;
        var relative = context.Relative(definition.FilePath);

        foreach (var key in document.Select(p => p.Key).Where(k => !TopLevelKeys.Contains(k)))
        {
            context.Diagnostics.SpecFatal(relative, $"unknown top-level key '{key}'.");
        }

        if (document["$schema"]?.GetValue<string>() != SchemaPath)
        {
            context.Diagnostics.SpecFatal(relative, $"must declare \"$schema\": \"{SchemaPath}\".");
        }

        if (document["version"] is not JsonValue version ||
            !version.TryGetValue<int>(out var value) || value != 1)
        {
            context.Diagnostics.SpecFatal(relative, "must declare integer 'version': 1.");
        }

        if (document["documents"] is not JsonObject documents || documents.Count == 0)
        {
            context.Diagnostics.SpecFatal(relative, "must define a non-empty 'documents' object.");
            return;
        }

        if (document["consumers"] is not JsonObject consumers || consumers.Count == 0)
        {
            context.Diagnostics.SpecFatal(relative, "must define a non-empty 'consumers' object.");
            return;
        }

        var validDocuments = ValidateDocuments(plugin, relative, documents);
        ValidateConsumers(plugin, relative, consumers, validDocuments);
    }

    private HashSet<string> ValidateDocuments(PluginPackage plugin, string relative, JsonObject documents)
    {
        var valid = new HashSet<string>(StringComparer.Ordinal);
        var pluginRoot = Path.GetFullPath(plugin.Directory) + Path.DirectorySeparatorChar;

        foreach (var (id, node) in documents.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (!AgentPluginSpec.SkillName.IsMatch(id))
            {
                context.Diagnostics.SpecFatal(relative, $"document id '{id}' must be kebab-case.");
                continue;
            }

            if (node is not JsonValue pathValue ||
                !pathValue.TryGetValue<string>(out var path) ||
                string.IsNullOrWhiteSpace(path))
            {
                context.Diagnostics.SpecFatal(relative, $"document '{id}' must map to a Markdown path.");
                continue;
            }

            if (Path.IsPathRooted(path) || !path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                context.Diagnostics.SpecFatal(relative, $"document '{id}' path '{path}' must be a relative .md file.");
                continue;
            }

            var full = Path.GetFullPath(Path.Combine(plugin.Directory, path));

            if (!full.StartsWith(pluginRoot, StringComparison.Ordinal))
            {
                context.Diagnostics.SpecFatal(relative, $"document '{id}' path '{path}' escapes the plugin directory.");
                continue;
            }

            if (!File.Exists(full))
            {
                context.Diagnostics.SpecFatal(relative, $"document '{id}' path '{path}' does not exist.");
                continue;
            }

            valid.Add(id);
        }

        return valid;
    }

    private void ValidateConsumers(
        PluginPackage plugin,
        string relative,
        JsonObject consumers,
        IReadOnlySet<string> validDocuments)
    {
        var skills = plugin.Skills.Select(s => s.DirectoryName).ToHashSet(StringComparer.Ordinal);
        var used = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (consumer, node) in consumers.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (!skills.Contains(consumer))
            {
                context.Diagnostics.SpecFatal(relative, $"consumer '{consumer}' is not a skill in this plugin.");
            }

            if (node is not JsonArray ids || ids.Count == 0)
            {
                context.Diagnostics.SpecFatal(relative, $"consumer '{consumer}' must list at least one document id.");
                continue;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var idNode in ids)
            {
                if (idNode is not JsonValue idValue || !idValue.TryGetValue<string>(out var id))
                {
                    context.Diagnostics.SpecFatal(relative, $"consumer '{consumer}' contains a non-string document id.");
                    continue;
                }

                if (!seen.Add(id))
                {
                    context.Diagnostics.SpecFatal(relative, $"consumer '{consumer}' lists document '{id}' more than once.");
                }

                if (!validDocuments.Contains(id))
                {
                    context.Diagnostics.SpecFatal(relative, $"consumer '{consumer}' references unknown document '{id}'.");
                    continue;
                }

                used.Add(id);
            }
        }

        foreach (var unused in validDocuments.Where(id => !used.Contains(id)).Order(StringComparer.Ordinal))
        {
            context.Diagnostics.Policy(relative, $"document '{unused}' has no consumer.");
        }
    }
}
