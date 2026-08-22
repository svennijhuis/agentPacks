namespace AgentPacks.Cli.Generation;

/// <summary>
/// What one client needs from a generated tree: where its files go, how it refers to the plugin
/// root, and whether its hooks nest. Keeping these differences in data rather than in four
/// generators means a new client is a row, not a rewrite.
/// </summary>
internal sealed record ClientProfile(
    Client Client,
    string Directory,
    string PluginRootToken,
    bool NestsHooks,
    bool SupportsWindowsCommand)
{
    /// <summary>
    /// Claude reads the root agents/, commands/ and hooks/ by default, so its marketplace entry
    /// points at this namespace instead. Without the redirect it would load Cursor's dialect.
    /// </summary>
    public static readonly ClientProfile Claude = new(
        Client.Claude,
        "com.anthropic.claude-code",
        "${CLAUDE_PLUGIN_ROOT}",
        NestsHooks: true,
        SupportsWindowsCommand: false);

    /// <summary>
    /// Cursor is the only client with no documented way to point at a custom path, so it keeps the
    /// plugin root — which is also where the neutral source is authored. Its official template uses
    /// plugin-relative commands, so no root token is substituted.
    /// </summary>
    public static readonly ClientProfile Cursor = new(
        Client.Cursor,
        string.Empty,
        ".",
        NestsHooks: false,
        SupportsWindowsCommand: false);

    /// <summary>
    /// Codex is the only client with a first-class per-OS hook command, so it gets the PowerShell
    /// invocation directly rather than going through the .cmd shim.
    /// </summary>
    public static readonly ClientProfile Codex = new(
        Client.Codex,
        "com.openai.codex",
        ".",
        NestsHooks: true,
        SupportsWindowsCommand: true);

    public static readonly ClientProfile Copilot = new(
        Client.Copilot,
        "com.github.copilot",
        "${PLUGIN_ROOT}",
        NestsHooks: true,
        SupportsWindowsCommand: false);

    public static readonly IReadOnlyList<ClientProfile> All = [Claude, Codex, Copilot, Cursor];

    /// <summary>Path of a file inside this client's tree, relative to the plugin directory.</summary>
    public string PluginRelative(string path) =>
        Directory.Length == 0 ? path : $"{Directory}/{path}";
}
