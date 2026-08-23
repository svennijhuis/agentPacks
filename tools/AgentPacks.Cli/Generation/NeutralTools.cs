namespace AgentPacks.Cli.Generation;

/// <summary>
/// The neutral tool vocabulary, and the one place it is written down.
///
/// A tool name is the only part of an agent that no client validates for the author: Claude drops a
/// name it does not recognise, and the other three pass their lists through untouched. So a
/// misspelling costs the agent a capability with nothing printed at any stage — the agent simply
/// works less well, on one client, for as long as nobody notices. Both halves of the defence read
/// from this file: <see cref="Claude"/> is what the generator maps with, and <see cref="All"/> is
/// what ComponentValidator rejects against, so a name can never be valid in one and unknown in the
/// other.
/// </summary>
internal static class NeutralTools
{
    /// <summary>
    /// Neutral name to Claude's. The neutral format is lowercase because that is what Cursor and
    /// Codex use; Claude is the only client that renames them.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Claude =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["read"] = "Read",
            ["write"] = "Write",
            ["edit"] = "Edit",
            ["grep"] = "Grep",
            ["glob"] = "Glob",
            ["bash"] = "Bash",
            ["webfetch"] = "WebFetch",
            ["websearch"] = "WebSearch"
        };

    /// <summary>Every tool a neutral agent may declare.</summary>
    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(Claude.Keys, StringComparer.Ordinal);

    /// <summary>
    /// Tools that can change the working tree. No client expresses "read-only" as a flag, so the
    /// only place the neutral 'readonly' can be enforced is against the declared tool list — which
    /// works only because every name in that list is known to be one of these or not.
    /// </summary>
    public static readonly IReadOnlySet<string> Writing =
        new HashSet<string>(StringComparer.Ordinal) { "write", "edit" };

    /// <summary>The vocabulary as a diagnostic reads it.</summary>
    public static string Listed => string.Join(", ", All.Order(StringComparer.Ordinal));
}
