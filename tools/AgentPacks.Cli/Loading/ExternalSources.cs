using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Loading;

/// <summary>A reviewed external skill owned by one plugin directory.</summary>
internal sealed record ExternalSourceEntry
{
    public required string Name { get; init; }
    public required string Repository { get; init; }
    public required string Path { get; init; }
    public required string Commit { get; init; }
    public required string License { get; init; }
    public string? Description { get; init; }

    /// <summary>The plugin directory containing external-skills.json.</summary>
    public required string PluginDirectory { get; init; }

    public string SourceFile => System.IO.Path.Combine(PluginDirectory, ExternalSources.FileName);
}

internal static class ExternalSources
{
    public const string FileName = "external-skills.json";

    public static IReadOnlyList<ExternalSourceEntry> Load(RepositoryContext context)
    {
        if (!Directory.Exists(context.PluginsRoot))
        {
            return [];
        }

        var results = new List<ExternalSourceEntry>();

        foreach (var pluginDirectory in Directory.GetDirectories(context.PluginsRoot)
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            var sourceFile = Path.Combine(pluginDirectory, FileName);
            if (!File.Exists(sourceFile))
            {
                continue;
            }

            var node = JsonFile.TryRead(sourceFile, out _);
            if (node is not JsonObject root || root["sources"] is not JsonArray sources)
            {
                continue;
            }

            foreach (var entry in sources.OfType<JsonObject>())
            {
                var name = entry["name"]?.GetValue<string>();
                var repository = entry["repository"]?.GetValue<string>();
                var path = entry["path"]?.GetValue<string>();
                var commit = entry["commit"]?.GetValue<string>();
                var license = entry["license"]?.GetValue<string>();

                if (name is null || repository is null || path is null || commit is null || license is null)
                {
                    continue;
                }

                results.Add(new ExternalSourceEntry
                {
                    Name = name,
                    Repository = repository,
                    Path = path,
                    Commit = commit,
                    License = license,
                    Description = entry["description"]?.GetValue<string>(),
                    PluginDirectory = pluginDirectory
                });
            }
        }

        return results
            .OrderBy(s => s.PluginDirectory, StringComparer.Ordinal)
            .ThenBy(s => s.Name, StringComparer.Ordinal)
            .ToList();
    }
}
