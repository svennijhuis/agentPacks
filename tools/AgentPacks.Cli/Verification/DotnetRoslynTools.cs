namespace AgentPacks.Cli.Verification;

/// <summary>
/// Local Roslyn surface: symbols, refs, diagnostics. No hosted server. No write tools.
/// </summary>
public static class DotnetRoslynTools
{
    public static readonly IReadOnlyList<string> ReadOnlyTools =
        ["list_symbols", "find_references", "list_diagnostics"];

    public static readonly IReadOnlyList<string> WriteTools =
        ["apply_fix", "rename_symbol", "refactor", "generate_code"];

    public static bool IsAllowedTool(string name) =>
        ReadOnlyTools.Contains(name, StringComparer.Ordinal);

    public static bool IsWriteTool(string name) =>
        WriteTools.Contains(name, StringComparer.Ordinal);
}
