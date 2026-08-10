using System.Text.Json.Nodes;
using Company.AI.Tooling.Io;

namespace Company.AI.Tooling.Loading;

/// <summary>A discovered skill: its directory, its SKILL.md and the parsed frontmatter.</summary>
internal sealed record SkillDefinition(string Directory, string SkillFilePath, Frontmatter? Frontmatter)
{
    public string DirectoryName => Path.GetFileName(Directory);
}

/// <summary>One plugin directory with everything loaded from it.</summary>
internal sealed record PluginPackage
{
    public required string Directory { get; init; }

    public required string ManifestPath { get; init; }

    /// <summary>Null when plugin.json is missing or unparseable; a diagnostic was recorded.</summary>
    public JsonObject? Manifest { get; init; }

    public string? McpPath { get; init; }

    /// <summary>Null when mcp.json is absent (valid) or unparseable (reported).</summary>
    public JsonObject? Mcp { get; init; }

    public IReadOnlyList<SkillDefinition> Skills { get; init; } = [];

    public string DirectoryName => Path.GetFileName(Directory);

    public string? Name => Manifest?["name"]?.GetValue<string>();

    public bool HasSkillsDirectory => System.IO.Directory.Exists(Path.Combine(Directory, "skills"));

}
