using System.Text.Json.Nodes;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Generation;

/// <summary>One hook to emit: which script runs, and the matcher the script itself must apply.</summary>
internal sealed record HookInvocation(string Script, string? ScriptMatcher, double? Timeout);

/// <summary>
/// Translates the neutral hook manifest into one client's dialect.
///
/// Two things vary beyond event names. Structure: Cursor puts the command flat in the event array
/// while Claude, Codex and Copilot nest it under a matcher object. And matcher placement: when the
/// client's matcher filters the same subject the author meant, it is emitted as the client matcher;
/// otherwise it is passed to the script, which applies the same regex itself. That keeps one
/// authored regex deciding the outcome on all four clients.
/// </summary>
internal static class HookGenerator
{
    /// <summary>Builds the hooks document, or null when the plugin declares no usable hooks.</summary>
    public static JsonObject? Build(PluginPackage plugin, ClientProfile profile)
    {
        if (plugin.Hooks?["hooks"] is not JsonObject events || events.Count == 0)
        {
            return null;
        }

        var byClientEvent = new SortedDictionary<string, JsonArray>(StringComparer.Ordinal);

        foreach (var neutralEvent in HookDialect.Events)
        {
            if (events[neutralEvent] is not JsonArray entries)
            {
                continue;
            }

            var declared = HookDialect.Event(neutralEvent)!;
            var target = declared.Targets[profile.Client];

            foreach (var entry in entries.OfType<JsonObject>())
            {
                var script = entry["script"]?.GetValue<string>();

                if (script is null)
                {
                    continue;
                }

                var authored = entry["matcher"]?.GetValue<string>();
                var timeout = entry["timeout"]?.GetValue<double>();

                // The client applies the authored matcher only when its own matcher filters the
                // same subject. Otherwise the client matcher stays the one the event implies and
                // the script receives the regex.
                var clientMatcher = target.AppliesAuthorMatcher ? authored ?? target.Matcher : target.Matcher;
                var scriptMatcher = target.AppliesAuthorMatcher ? null : authored;

                var invocation = new HookInvocation(script, scriptMatcher, timeout);

                if (!byClientEvent.TryGetValue(target.Event, out var list))
                {
                    list = [];
                    byClientEvent[target.Event] = list;
                }

                Append(list, profile, invocation, clientMatcher);
            }
        }

        if (byClientEvent.Count == 0)
        {
            return null;
        }

        var hooks = new JsonObject();

        foreach (var (name, entries) in byClientEvent)
        {
            hooks[name] = entries;
        }

        var document = new JsonObject();

        // Copilot's format carries a version; the other three reject nothing but declare none.
        if (profile.HookDocumentVersion is { } version)
        {
            document["version"] = version;
        }

        document["hooks"] = hooks;

        return document;
    }

    private static void Append(JsonArray target, ClientProfile profile, HookInvocation invocation, string? matcher)
    {
        if (!profile.NestsHooks)
        {
            var flat = new JsonObject { ["type"] = "command" };

            AddCommands(flat, profile, invocation);
            AddMatcher(flat, matcher);
            AddTimeout(flat, profile, invocation);
            target.Add(flat);
            return;
        }

        // Nesting clients group by matcher: two hooks with the same matcher belong in one entry, or
        // the second silently replaces the first in some clients.
        var group = target
            .OfType<JsonObject>()
            .FirstOrDefault(existing => (existing["matcher"]?.GetValue<string>()) == matcher);

        if (group is null)
        {
            group = new JsonObject();
            AddMatcher(group, matcher);
            group["hooks"] = new JsonArray();
            target.Add(group);
        }

        var nested = new JsonObject { ["type"] = "command" };

        AddCommands(nested, profile, invocation);
        AddTimeout(nested, profile, invocation);
        ((JsonArray)group["hooks"]!).Add(nested);
    }

    private static void AddMatcher(JsonObject entry, string? matcher)
    {
        if (matcher is not null)
        {
            entry["matcher"] = matcher;
        }
    }

    /// <summary>
    /// The POSIX command under whichever key this client reads, plus its Windows half where the
    /// client has one. Claude, Cursor and Copilot have no .cmd concern for the Windows key: Codex
    /// spells it "commandWindows" and takes a full shell invocation, while Copilot spells it
    /// "powershell" and takes the PowerShell command itself.
    /// </summary>
    private static void AddCommands(JsonObject entry, ClientProfile profile, HookInvocation invocation)
    {
        entry[profile.CommandField] = Command(profile, invocation);

        if (profile.WindowsCommandField is { } windows)
        {
            entry[windows] = WindowsCommand(profile, invocation);
        }
    }

    private static void AddTimeout(JsonObject entry, ClientProfile profile, HookInvocation invocation)
    {
        if (invocation.Timeout is { } timeout)
        {
            entry[profile.TimeoutField] = timeout;
        }
    }

    /// <summary>
    /// The POSIX command. The path is deliberately extensionless: a shell runs the executable
    /// scripts/&lt;name&gt; directly, and cmd.exe appends PATHEXT to find the generated
    /// scripts/&lt;name&gt;.cmd shim, so one string works on both platforms.
    /// </summary>
    public static string Command(ClientProfile profile, HookInvocation invocation) =>
        PosixCommand(profile, $"scripts/{invocation.Script}", invocation.ScriptMatcher);

    /// <summary>
    /// The Windows command, for the clients that accept a per-OS override. Codex reads a full
    /// shell invocation, so it gets one; Copilot's "powershell" key is already a PowerShell
    /// context, so the interpreter is not named again.
    /// </summary>
    public static string WindowsCommand(ClientProfile profile, HookInvocation invocation) =>
        WindowsCommand(profile, $"scripts/{invocation.Script}", invocation.ScriptMatcher);

    /// <summary>An empty document in this client's shape, for a plugin that authored no hooks.</summary>
    public static JsonObject EmptyDocument(ClientProfile profile)
    {
        var document = new JsonObject();

        if (profile.HookDocumentVersion is { } version)
        {
            document["version"] = version;
        }

        document["hooks"] = new JsonObject();

        return document;
    }

    /// <summary>
    /// Adds the generated rules hook to a SessionStart array. It carries no matcher, and its
    /// script lives inside the client tree rather than beside the authored pairs, so it cannot go
    /// through <see cref="HookInvocation"/> — but it must still be shaped by the same profile.
    /// </summary>
    public static void AppendRulesCommand(JsonArray target, ClientProfile profile, string pluginRelativeScript)
    {
        var entry = new JsonObject { ["type"] = "command" };

        entry[profile.CommandField] = PosixCommand(profile, pluginRelativeScript, null);

        if (profile.WindowsCommandField is { } windows)
        {
            entry[windows] = WindowsCommand(profile, pluginRelativeScript, null);
        }

        if (!profile.NestsHooks)
        {
            target.Add(entry);
            return;
        }

        // The rules hook carries no matcher, so it belongs in the matcher-less group rather than in
        // a second one beside it: nesting clients group by matcher, and two groups with the same
        // matcher mean one of them silently wins.
        var group = target.OfType<JsonObject>().FirstOrDefault(candidate => candidate["matcher"] is null);

        if (group is null)
        {
            group = new JsonObject { ["hooks"] = new JsonArray() };
            target.Add(group);
        }

        ((JsonArray)group["hooks"]!).Add(entry);
    }

    private static string PosixCommand(ClientProfile profile, string pluginRelative, string? matcher)
    {
        var command = $"\"{profile.PluginRootToken}/{pluginRelative}\"";

        return matcher is { } value
            ? $"{command} {HookDialect.MatcherArgument} \"{value}\""
            : command;
    }

    private static string WindowsCommand(ClientProfile profile, string pluginRelative, string? matcher)
    {
        var script = $"\"{profile.PluginRootToken}/{pluginRelative}.ps1\"";

        var command = profile.WindowsCommandField == "powershell"
            ? $"& {script}"
            : $"powershell -NoProfile -ExecutionPolicy Bypass -File {script}";

        // The matcher is emitted verbatim inside double quotes. HookValidator rejects the
        // characters that would escape those quotes, because sh and PowerShell disagree on how to
        // escape them and a matcher that means two different things per platform is worse than one
        // the author has to rewrite.
        return matcher is { } value
            ? $"{command} {HookDialect.MatcherArgument} \"{value}\""
            : command;
    }
}
