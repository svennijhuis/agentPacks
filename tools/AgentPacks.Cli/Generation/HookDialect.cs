namespace AgentPacks.Cli.Generation;

/// <summary>The clients a neutral component is translated for.</summary>
internal enum Client
{
    Claude,
    Cursor,
    Codex,
    Copilot
}

/// <summary>What an authored matcher is meant to filter on.</summary>
internal enum MatcherSubject
{
    /// <summary>The event admits no matcher; authoring one is an error.</summary>
    None,

    /// <summary>The name of the tool being invoked.</summary>
    Tool,

    /// <summary>The shell command text.</summary>
    Command,

    /// <summary>The path of the file being edited.</summary>
    FilePath
}

/// <summary>
/// How one neutral event lands in one client.
///
/// <see cref="Event"/> is the client's event name. <see cref="Matcher"/> is the matcher the client
/// needs to narrow a broad event down to what the neutral name means — Claude has no
/// beforeShellExecution, so it becomes PreToolUse matched to the Bash tool.
///
/// <see cref="AppliesAuthorMatcher"/> is the part that is easy to get wrong: a client matcher
/// filters on exactly one subject. Under Claude's PreToolUse that subject is the tool name, which
/// is already spent identifying Bash, so an authored "rm|curl" — which is about the command text —
/// cannot be expressed there and is handed to the script instead. Cursor's beforeShellExecution
/// matcher does filter on command text, so Cursor applies it natively.
/// </summary>
internal sealed record HookTarget(string Event, string? Matcher = null, bool AppliesAuthorMatcher = false);

/// <summary>One neutral event: what its matcher means, and where it lands in each client.</summary>
internal sealed record HookEvent(string Name, MatcherSubject Subject, Dictionary<Client, HookTarget> Targets);

/// <summary>
/// The neutral hook vocabulary and its translation into the four client dialects.
///
/// This is the only place the mapping exists. Beyond renaming, the dialects differ structurally:
/// Cursor puts the command flat in the event array, while Claude and Codex nest it under a matcher
/// object. Codex is the only one with a per-OS field (commandWindows); the others rely on the
/// generated .cmd shim being resolved by cmd.exe through PATHEXT.
/// </summary>
internal static class HookDialect
{
    /// <summary>Argument carrying an authored matcher the client itself cannot express.</summary>
    public const string MatcherArgument = "--matcher";

    private static readonly Dictionary<string, HookEvent> Map = new(StringComparer.Ordinal)
    {
        ["sessionStart"] = new("sessionStart", MatcherSubject.None, new()
        {
            [Client.Claude] = new("SessionStart"),
            [Client.Cursor] = new("sessionStart"),
            [Client.Codex] = new("SessionStart"),
            [Client.Copilot] = new("sessionStart")
        }),
        ["sessionEnd"] = new("sessionEnd", MatcherSubject.None, new()
        {
            [Client.Claude] = new("SessionEnd"),
            [Client.Cursor] = new("sessionEnd"),
            [Client.Codex] = new("SessionEnd"),
            [Client.Copilot] = new("sessionEnd")
        }),
        ["userPromptSubmit"] = new("userPromptSubmit", MatcherSubject.None, new()
        {
            [Client.Claude] = new("UserPromptSubmit"),
            [Client.Cursor] = new("beforeSubmitPrompt"),
            [Client.Codex] = new("UserPromptSubmit"),
            [Client.Copilot] = new("userPromptSubmit")
        }),
        ["stop"] = new("stop", MatcherSubject.None, new()
        {
            [Client.Claude] = new("Stop"),
            [Client.Cursor] = new("stop"),
            [Client.Codex] = new("Stop"),
            [Client.Copilot] = new("stop")
        }),

        // The tool-matched events: every client's matcher filters on the tool name, which is
        // exactly what an authored matcher means here, so all four apply it natively.
        ["preToolUse"] = new("preToolUse", MatcherSubject.Tool, new()
        {
            [Client.Claude] = new("PreToolUse", null, AppliesAuthorMatcher: true),
            [Client.Cursor] = new("preToolUse", null, AppliesAuthorMatcher: true),
            [Client.Codex] = new("PreToolUse", null, AppliesAuthorMatcher: true),
            [Client.Copilot] = new("preToolUse", null, AppliesAuthorMatcher: true)
        }),
        ["postToolUse"] = new("postToolUse", MatcherSubject.Tool, new()
        {
            [Client.Claude] = new("PostToolUse", null, AppliesAuthorMatcher: true),
            [Client.Cursor] = new("postToolUse", null, AppliesAuthorMatcher: true),
            [Client.Codex] = new("PostToolUse", null, AppliesAuthorMatcher: true),
            [Client.Copilot] = new("postToolUse", null, AppliesAuthorMatcher: true)
        }),

        // Only Cursor has a first-class shell event whose matcher reads the command text. The
        // others spend their matcher naming the shell tool, so the command filter goes to the
        // script and the same regex still decides the outcome on every client.
        ["beforeShellExecution"] = new("beforeShellExecution", MatcherSubject.Command, new()
        {
            [Client.Claude] = new("PreToolUse", "Bash"),
            [Client.Cursor] = new("beforeShellExecution", null, AppliesAuthorMatcher: true),
            [Client.Codex] = new("PreToolUse", "shell"),
            [Client.Copilot] = new("preToolUse", "shell")
        }),
        ["afterFileEdit"] = new("afterFileEdit", MatcherSubject.FilePath, new()
        {
            [Client.Claude] = new("PostToolUse", "Write|Edit"),
            [Client.Cursor] = new("afterFileEdit"),
            [Client.Codex] = new("PostToolUse", "apply_patch|write"),
            [Client.Copilot] = new("postToolUse", "write|edit")
        })
    };

    /// <summary>Every neutral event name, ordered so generated output and errors are stable.</summary>
    public static IReadOnlyList<string> Events { get; } =
        Map.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

    public static bool IsKnownEvent(string neutralEvent) => Map.ContainsKey(neutralEvent);

    public static HookEvent? Event(string neutralEvent) =>
        Map.TryGetValue(neutralEvent, out var value) ? value : null;

    /// <summary>The client's event name and matcher, or null when the event is not in the vocabulary.</summary>
    public static HookTarget? Target(string neutralEvent, Client client) =>
        Map.TryGetValue(neutralEvent, out var value) ? value.Targets[client] : null;
}
