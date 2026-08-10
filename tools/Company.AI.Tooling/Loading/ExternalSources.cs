using System.Text.Json.Nodes;
using Company.AI.Tooling.Io;

namespace Company.AI.Tooling.Loading;

/// <summary>One reviewed, pinned external skill.</summary>
internal sealed record ExternalSourceEntry
{
    public required string Name { get; init; }

    public required string Repository { get; init; }

    /// <summary>Path to the skill directory inside the source repository.</summary>
    public required string Path { get; init; }

    /// <summary>Exact 40-character commit SHA. Branches are never tracked.</summary>
    public required string Commit { get; init; }

    public required string License { get; init; }

    public required string Owner { get; init; }

    /// <summary>Plugin to vendor into. Optional when the repository holds a single plugin.</summary>
    public string? Plugin { get; init; }
}

internal static class ExternalSources
{
    public const string MarkerFileName = ".vendored.json";

    /// <summary>
    /// Reads external/sources.json. Returns an empty list when the file is missing or malformed;
    /// PluginValidator reports those cases, so this stays quiet rather than double-reporting.
    /// </summary>
    public static IReadOnlyList<ExternalSourceEntry> Load(RepositoryContext context)
    {
        if (!File.Exists(context.ExternalSourcesPath))
        {
            return [];
        }

        var node = JsonFile.TryRead(context.ExternalSourcesPath, out _);

        if (node is not JsonObject root || root["sources"] is not JsonArray sources)
        {
            return [];
        }

        var results = new List<ExternalSourceEntry>();

        foreach (var entry in sources.OfType<JsonObject>())
        {
            var name = entry["name"]?.GetValue<string>();
            var repository = entry["repository"]?.GetValue<string>();
            var path = entry["path"]?.GetValue<string>();
            var commit = entry["commit"]?.GetValue<string>();
            var license = entry["license"]?.GetValue<string>();
            var owner = entry["owner"]?.GetValue<string>();

            if (name is null || repository is null || path is null ||
                commit is null || license is null || owner is null)
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
                Owner = owner,
                Plugin = entry["plugin"]?.GetValue<string>()
            });
        }

        return results
            .OrderBy(s => s.Name, StringComparer.Ordinal)
            .ToList();
    }
}
