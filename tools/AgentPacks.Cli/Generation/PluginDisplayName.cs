using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Human-readable plugin titles for Cursor <c>plugin.json</c> and Codex
/// <c>interface.displayName</c>. Kebab-case becomes title case (squad → Squad,
/// pack-check → Pack Check); <c>dotnet</c> and <c>typescript</c> keep their
/// product spelling so Customize does not show "Dotnet" / "Typescript".
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
