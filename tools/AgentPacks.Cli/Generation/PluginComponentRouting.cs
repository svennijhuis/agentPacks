using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>
/// Shared skills / hooks / MCP path declarations for Copilot catalog entries and Codex
/// <c>.codex-plugin/plugin.json</c>. MCP points at the generated <c>.mcp.json</c> (converted),
/// never the authored <c>mcp.json</c>, which still has unexpanded <c>${PLUGIN_ROOT}</c>.
/// Claude keeps its own entry builder: agent paths are files, hooks skip auto-discovered
/// root files, and MCP writes <c>.mcp.json</c> as a side effect.
/// </summary>
internal static class PluginComponentRouting
{
    public readonly record struct RoutedComponents(bool Skills, bool Hooks, bool Mcp);

    public static RoutedComponents ApplySkillsHooksMcp(
        JsonObject target,
        PluginPackage plugin,
        ClientProfile profile)
    {
        var skills = plugin.HasSkillsDirectory;
        if (skills)
        {
            target["skills"] = "./skills/";
        }

        var hooks = HookGenerator.EmitsHooksFile(plugin, profile);
        if (hooks)
        {
            target["hooks"] = $"./{profile.Directory}/hooks/hooks.json";
        }

        var mcp = plugin.McpServers is not null;
        if (mcp)
        {
            target["mcpServers"] = "./.mcp.json";
        }

        return new RoutedComponents(skills, hooks, mcp);
    }
}
