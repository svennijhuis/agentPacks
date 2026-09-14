namespace AgentPacks.Cli.Generation;

/// <summary>
/// The plugin-relative locations the generators own. Everything matching one of these is derived
/// from the authored source, so a file found there that the current run does not produce is stale
/// and gets deleted. Authored content (plugin.json, skills/, rules/, agents/, commands/, mcp.json,
/// hooks.source.json, standards.source.json, standards/, scripts/*.sh, scripts/*.ps1) never matches.
/// The one exception is <c>skills/&lt;slot&gt;/SKILL.md</c> beside a <c>SKILL.source.md</c>: that
/// file is rendered from the shared slot template.
/// </summary>
internal static class GeneratedPaths
{
    /// <summary>Client namespaces and manifest directories generated inside a plugin.</summary>
    public static readonly string[] OwnedDirectories =
    [
        "hooks",
        ".cursor-plugin",
        ".codex-plugin",
        "com.anthropic.claude-code",
        "com.openai.codex",
        "com.github.copilot"
    ];

    /// <summary>
    /// True when <paramref name="pluginRelative"/> is generated rather than authored.
    /// <paramref name="renderedSkills"/> names the skill directories that carry a
    /// <c>SKILL.source.md</c>; their <c>SKILL.md</c> is a render, not an authored file.
    /// </summary>
    public static bool IsGenerated(string pluginRelative, IReadOnlyCollection<string>? renderedSkills = null)
    {
        var path = pluginRelative.Replace('\\', '/');

        if (path == ".mcp.json")
        {
            return true;
        }

        // The extensionless POSIX dispatcher and the Windows .cmd shim are generated beside the
        // authored .sh and .ps1 pair. Everything else under scripts/ is authored.
        if (path.StartsWith("scripts/", StringComparison.Ordinal))
        {
            return path.EndsWith(".cmd", StringComparison.Ordinal)
                || Path.GetExtension(path).Length == 0;
        }

        // Only these nested skill paths are generated. A skill may keep any other authored
        // references beside them without the staleness sweep touching them.
        var segments = path.Split('/');

        if (segments.Length >= 5 &&
            segments[0] == "skills" &&
            segments[2] == "references" &&
            segments[3] == "standards")
        {
            return true;
        }

        if (renderedSkills is { Count: > 0 } &&
            segments.Length == 3 &&
            segments[0] == "skills" &&
            segments[2] == "SKILL.md" &&
            renderedSkills.Contains(segments[1]))
        {
            return true;
        }

        // Codex's user-invoked flag lives beside the portable SKILL.md and is regenerated from
        // disable-model-invocation so the two dialects cannot drift.
        if (segments.Length == 4 &&
            segments[0] == "skills" &&
            segments[2] == "agents" &&
            segments[3] == "openai.yaml")
        {
            return true;
        }

        return OwnedDirectories.Any(directory =>
            path.StartsWith(directory + "/", StringComparison.Ordinal));
    }
}
