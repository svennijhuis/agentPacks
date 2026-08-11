using System.Text.RegularExpressions;

namespace AgentPacks.Cli;

/// <summary>
/// Constants taken from the Agent Plugins 1.0.0 and Agent Skills specifications.
/// Anything the vendored JSON Schemas already enforce is deliberately absent here.
/// </summary>
internal static partial class AgentPluginSpec
{
    public const string PluginSchemaUrl = "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json";
    public const string McpSchemaUrl = "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json";

    /// <summary>
    /// Canonical schema URLs supported by this tooling.
    /// </summary>
    public static readonly IReadOnlySet<string> SupportedSchemaUrls =
        new HashSet<string>(StringComparer.Ordinal) { PluginSchemaUrl, McpSchemaUrl };

    /// <summary>Spec version a <c>$schema</c> identifier targets, used for the cross-file match.</summary>
    public static string? SpecVersionOf(string? schemaUrl) => schemaUrl switch
    {
        PluginSchemaUrl or McpSchemaUrl => "1.0.0",
        _ => null
    };

    /// <summary>Environment variables a plugin may not define. Also enforced by the MCP schema.</summary>
    public static readonly IReadOnlySet<string> ReservedEnvironmentVariables =
        new HashSet<string>(StringComparer.Ordinal) { "PLUGIN_ROOT", "PLUGIN_DATA" };

    /// <summary>
    /// Agent Skills name rule: 1-64 chars, lowercase alphanumeric and hyphens, no leading or
    /// trailing hyphen, no consecutive hyphens. Stricter than the plugin rule, which allows periods.
    /// </summary>
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public static partial Regex SkillName { get; }

    /// <summary>Names Claude Code and Copilot marketplaces accept: kebab-case, no periods.</summary>
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public static partial Regex MarketplaceSafeName { get; }

    /// <summary>A bare executable token: no whitespace, no path separators, no shell syntax.</summary>
    [GeneratedRegex(@"^[A-Za-z0-9._+-]+$")]
    public static partial Regex BareExecutable { get; }

    /// <summary>RFC 9110 field name.</summary>
    [GeneratedRegex(@"^[!#$%&'*+\-.^_`|~0-9A-Za-z]+$")]
    public static partial Regex HttpFieldName { get; }

    /// <summary>Exactly 40 hexadecimal characters.</summary>
    [GeneratedRegex("^[0-9a-fA-F]{40}$")]
    public static partial Regex GitCommitSha { get; }

    /// <summary>
    /// Keys that suggest a credential. The spec requires headers to be literal, non-secret
    /// package data and defines no portable credential fields, so these are spec violations.
    /// </summary>
    [GeneratedRegex("(token|secret|password|passwd|authorization|auth|cookie|credential|bearer|api[-_]?key|access[-_]?key|private[-_]?key)",
        RegexOptions.IgnoreCase)]
    public static partial Regex LikelySecret { get; }

    public const int SkillNameMaxLength = 64;
    public const int SkillDescriptionMaxLength = 1024;
    public const int SkillCompatibilityMaxLength = 500;
}
