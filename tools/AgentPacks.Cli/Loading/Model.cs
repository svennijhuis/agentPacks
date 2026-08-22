using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Loading;

/// <summary>A discovered skill: its directory, its SKILL.md and the parsed frontmatter.</summary>
internal sealed record SkillDefinition(string Directory, string SkillFilePath, Frontmatter? Frontmatter)
{
    public string DirectoryName => Path.GetFileName(Directory);
}

/// <summary>
/// A component authored as one Markdown file with frontmatter: an agent, a command or a rule.
/// The three differ only in which frontmatter keys they carry and which clients receive them,
/// so they share a shape and are told apart by <see cref="Kind"/>.
/// </summary>
internal sealed record MarkdownComponent(string Kind, string FilePath, Frontmatter? Frontmatter)
{
    /// <summary>Filename without its extension. The authored name must match it.</summary>
    public string FileName => Path.GetFileNameWithoutExtension(FilePath);

    /// <summary>The declared name, falling back to the filename the way every client does.</summary>
    public string Name => Frontmatter?.Scalar("name") ?? FileName;

    public string Description => Frontmatter?.Scalar("description") ?? string.Empty;

    public string Body => Frontmatter?.Body ?? string.Empty;
}

/// <summary>A hook script authored as a POSIX/PowerShell pair. Both halves are required.</summary>
internal sealed record ScriptDefinition(string Name, string? PosixPath, string? PowerShellPath)
{
    public bool IsComplete => PosixPath is not null && PowerShellPath is not null;
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

    /// <summary>Subagents from agents/*.md, in the neutral dialect.</summary>
    public IReadOnlyList<MarkdownComponent> Agents { get; init; } = [];

    /// <summary>Slash commands from commands/*.md.</summary>
    public IReadOnlyList<MarkdownComponent> Commands { get; init; } = [];

    /// <summary>Persistent guidance from rules/*.mdc.</summary>
    public IReadOnlyList<MarkdownComponent> Rules { get; init; } = [];

    public string? HooksPath { get; init; }

    /// <summary>The neutral hooks.source.json document; null when the plugin declares no hooks.</summary>
    public JsonObject? Hooks { get; init; }

    /// <summary>Everything under scripts/, paired by basename.</summary>
    public IReadOnlyList<ScriptDefinition> Scripts { get; init; } = [];

    public string DirectoryName => Path.GetFileName(Directory);

    public string? Name => Manifest?["name"]?.GetValue<string>();

    public bool HasSkillsDirectory => System.IO.Directory.Exists(Path.Combine(Directory, "skills"));

    public ScriptDefinition? Script(string name) =>
        Scripts.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.Ordinal));
}
