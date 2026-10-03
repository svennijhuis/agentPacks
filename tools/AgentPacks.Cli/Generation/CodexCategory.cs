using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Codex marketplace and <c>.codex-plugin/plugin.json</c> classifier. Official
/// <c>openai/plugins</c> puts coding/workflow packs (superpowers, github,
/// plugin-eval) under <c>Developer Tools</c> and audit packs (codex-security)
/// under <c>Security</c>. <c>Productivity</c> is for Linear/Notion/calendar
/// apps, not agent workflows.
/// </summary>
internal static class CodexCategory
{
    public const string DeveloperTools = "Developer Tools";
    public const string Security = "Security";

    public static string From(PluginPackage plugin) => From(plugin.Name ?? plugin.DirectoryName);

    public static string From(string name) => name switch
    {
        "security" => Security,
        _ => DeveloperTools
    };
}
