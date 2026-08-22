namespace AgentPacks.Cli.Generation;

/// <summary>
/// The plugin-relative locations the generators own. Everything matching one of these is derived
/// from the authored source, so a file found there that the current run does not produce is stale
/// and gets deleted. Authored content (plugin.json, skills/, rules/, agents/, commands/, mcp.json,
/// hooks.source.json, scripts/*.sh, scripts/*.ps1) never matches.
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

    /// <summary>True when <paramref name="pluginRelative"/> is generated rather than authored.</summary>
    public static bool IsGenerated(string pluginRelative)
    {
        var path = pluginRelative.Replace('\\', '/');

        if (path == ".mcp.json")
        {
            return true;
        }

        // The Windows shim is generated beside the authored .sh and .ps1 pair.
        if (path.StartsWith("scripts/", StringComparison.Ordinal))
        {
            return path.EndsWith(".cmd", StringComparison.Ordinal);
        }

        return OwnedDirectories.Any(directory =>
            path.StartsWith(directory + "/", StringComparison.Ordinal));
    }
}
