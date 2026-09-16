using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Human-readable plugin titles for Cursor <c>plugin.json</c>, Codex
/// <c>interface.displayName</c>, and Claude marketplace entries
/// (<c>displayName</c>, Claude Code v2.1.143+). Kebab-case becomes title case
/// (squad → Squad, pack-check → Pack Check); <c>dotnet</c> and <c>typescript</c>
/// keep their product spelling so pickers do not show "Dotnet" / "Typescript".
/// Copilot's marketplace schema has no display-name field, so it stays kebab-case.
/// </summary>
internal static class PluginDisplayName
{
    public static string From(PluginPackage plugin) => From(plugin.Name ?? plugin.DirectoryName);

    public static string From(string name) => name switch
    {
        "dotnet" => ".NET",
        "typescript" => "TypeScript",
        _ => string.Join(' ', name
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]))
    };
}
