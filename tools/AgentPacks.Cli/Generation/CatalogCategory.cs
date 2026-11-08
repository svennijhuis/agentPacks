using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Catalog classifiers that differ by client. Official Claude entries use
/// <c>development</c> / <c>security</c>; official Cursor workflow packs set
/// <c>developer-tools</c> on <c>.cursor-plugin/plugin.json</c> (the catalog
/// cannot carry it). Codex title-case lives in <see cref="CodexCategory"/>.
/// Copilot's published community catalog does not set category.
/// </summary>
internal static class CatalogCategory
{
    public const string ClaudeDevelopment = "development";
    public const string ClaudeSecurity = "security";
    public const string CursorDeveloperTools = "developer-tools";

    public static string Claude(PluginPackage plugin) => Claude(plugin.CanonicalName);

    public static string Claude(string name) => name switch
    {
        "security" => ClaudeSecurity,
        _ => ClaudeDevelopment
    };

    public static string Cursor() => CursorDeveloperTools;
}
