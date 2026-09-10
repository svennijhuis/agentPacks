namespace AgentPacks.Cli.Generation;

/// <summary>
/// What one client needs from a generated tree: where its files go, how it refers to the plugin
/// root, and whether its hooks nest. This is the hook/paths adapter; a new client also needs
/// component transforms, manifest routing, validation and fixtures.
/// </summary>
internal sealed record ClientProfile(
    Client Client,
    string Directory,
    string PluginRootToken,
    bool NestsHooks,
    string CommandField,
    string? WindowsCommandField,
    string TimeoutField,
    int? HookDocumentVersion,
    string? HookCwd = null)
{
    /// <summary>
    /// Claude reads the root agents/, commands/ and hooks/ by default unless the marketplace
    /// entry is strict. The entry points at this namespace and sets strict so Cursor's root
    /// dialect is not a second discoverable copy. pack-check and git omit marketplace hooks
    /// (Claude rejects a path or array) and put this dialect at plugin-root hooks/hooks.json.
    /// </summary>
    public static readonly ClientProfile Claude = new(
        Client.Claude,
        "com.anthropic.claude-code",
        "${CLAUDE_PLUGIN_ROOT}",
        NestsHooks: true,
        CommandField: "command",
        WindowsCommandField: null,
        TimeoutField: "timeout",
        HookDocumentVersion: null);

    /// <summary>
    /// Cursor-shaped hooks for packs whose plugin-root <c>hooks/hooks.json</c> is Claude-shaped
    /// (Claude marketplace cannot declare a hooks path). Cursor's plugin manifest points here.
    /// </summary>
    public const string RelocatedCursorHooks = ".cursor-plugin/hooks/hooks.json";

    /// <summary>
    /// Cursor keeps the plugin root for most packs — which is also where the neutral source is
    /// authored. Its official template uses plugin-relative commands, so no root token is
    /// substituted. When Claude owns the root hooks file, the manifest points at
    /// <see cref="RelocatedCursorHooks"/> instead.
    /// </summary>
    public static readonly ClientProfile Cursor = new(
        Client.Cursor,
        string.Empty,
        ".",
        NestsHooks: false,
        CommandField: "command",
        WindowsCommandField: null,
        TimeoutField: "timeout",
        HookDocumentVersion: null);

    /// <summary>
    /// Codex is the only client with a first-class per-OS hook command, so it gets the PowerShell
    /// invocation directly rather than going through the .cmd shim.
    /// </summary>
    public static readonly ClientProfile Codex = new(
        Client.Codex,
        "com.openai.codex",
        "${PLUGIN_ROOT}",
        NestsHooks: true,
        CommandField: "command",
        WindowsCommandField: "commandWindows",
        TimeoutField: "timeout",
        HookDocumentVersion: null);

    /// <summary>
    /// Copilot's hook document is the outlier: entries sit flat under the event with the matcher on
    /// the entry itself, the POSIX command is keyed "bash" rather than "command", its Windows half
    /// is an inline PowerShell command keyed "powershell", the timeout is "timeoutSec", and the
    /// document carries a format version. Event names accept both casings, so the PascalCase
    /// aliases are emitted for consistency with the other two nesting clients.
    /// Copilot CLI resolves hook scripts against the project cwd unless the entry sets
    /// <c>cwd</c> to <c>${PLUGIN_ROOT}</c> (github/copilot-cli#3659). As of ~1.0.57 a missing
    /// script fail-closes PreToolUse and denies the tool.
    /// </summary>
    public static readonly ClientProfile Copilot = new(
        Client.Copilot,
        "com.github.copilot",
        "${PLUGIN_ROOT}",
        NestsHooks: false,
        CommandField: "bash",
        WindowsCommandField: "powershell",
        TimeoutField: "timeoutSec",
        HookDocumentVersion: 1,
        HookCwd: "${PLUGIN_ROOT}");

    public static readonly IReadOnlyList<ClientProfile> All = [Claude, Codex, Copilot, Cursor];

    /// <summary>Path of a file inside this client's tree, relative to the plugin directory.</summary>
    public string PluginRelative(string path) =>
        Directory.Length == 0 ? path : $"{Directory}/{path}";
}
