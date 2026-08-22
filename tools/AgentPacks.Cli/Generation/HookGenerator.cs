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

        return new JsonObject { ["hooks"] = hooks };
    }

    private static void Append(JsonArray target, ClientProfile profile, HookInvocation invocation, string? matcher)
    {
        if (!profile.NestsHooks)
        {
            var flat = new JsonObject
            {
                ["type"] = "command",
                ["command"] = Command(profile, invocation)
            };

            AddMatcher(flat, matcher);
            AddTimeout(flat, invocation);
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

        var nested = new JsonObject
        {
            ["type"] = "command",
            ["command"] = Command(profile, invocation)
        };

        if (profile.SupportsWindowsCommand)
        {
            nested["commandWindows"] = WindowsCommand(profile, invocation);
        }

        AddTimeout(nested, invocation);
        ((JsonArray)group["hooks"]!).Add(nested);
    }

    private static void AddMatcher(JsonObject entry, string? matcher)
    {
        if (matcher is not null)
        {
            entry["matcher"] = matcher;
        }
    }

    private static void AddTimeout(JsonObject entry, HookInvocation invocation)
    {
        if (invocation.Timeout is { } timeout)
        {
            entry["timeout"] = timeout;
        }
    }

    /// <summary>
    /// The POSIX command. The path is deliberately extensionless: a shell runs the executable
    /// scripts/&lt;name&gt; directly, and cmd.exe appends PATHEXT to find the generated
    /// scripts/&lt;name&gt;.cmd shim, so one string works on both platforms.
    /// </summary>
    public static string Command(ClientProfile profile, HookInvocation invocation)
    {
        var command = $"\"{profile.PluginRootToken}/scripts/{invocation.Script}\"";

        return invocation.ScriptMatcher is { } matcher
            ? $"{command} {HookDialect.MatcherArgument} \"{matcher}\""
            : command;
    }

    /// <summary>The Windows command, for the one client that accepts a per-OS override.</summary>
    public static string WindowsCommand(ClientProfile profile, HookInvocation invocation)
    {
        var command =
            "powershell -NoProfile -ExecutionPolicy Bypass -File " +
            $"\"{profile.PluginRootToken}/scripts/{invocation.Script}.ps1\"";

        // The matcher is emitted verbatim inside double quotes. HookValidator rejects the
        // characters that would escape those quotes, because sh and PowerShell disagree on how to
        // escape them and a matcher that means two different things per platform is worse than one
        // the author has to rewrite.
        return invocation.ScriptMatcher is { } matcher
            ? $"{command} {HookDialect.MatcherArgument} \"{matcher}\""
            : command;
    }
}
