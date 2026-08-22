using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Validation;

/// <summary>
/// Validates the neutral hook manifest. Hooks run arbitrary code on a developer's machine at
/// lifecycle points they did not trigger, so this is the strictest validator in the tool: an event
/// outside the vocabulary, a script missing a platform half, or a matcher that does not compile all
/// fail the build rather than degrading silently on one client.
/// </summary>
internal sealed class HookValidator(RepositoryContext context)
{
    private static readonly IReadOnlySet<string> EntryKeys =
        new HashSet<string>(StringComparer.Ordinal) { "script", "matcher", "timeout" };

    private const int MaxTimeoutSeconds = 600;

    public void Validate(IReadOnlyList<PluginPackage> plugins)
    {
        foreach (var plugin in plugins)
        {
            ValidatePlugin(plugin);
        }
    }

    private void ValidatePlugin(PluginPackage plugin)
    {
        ValidateScriptPairs(plugin);

        if (plugin.Hooks is not { } document || plugin.HooksPath is null)
        {
            return;
        }

        var relative = context.Relative(plugin.HooksPath);

        foreach (var key in document.Select(p => p.Key).Where(k => k is not ("$schema" or "hooks" or "description")))
        {
            context.Diagnostics.SpecFatal(relative, $"unknown top-level key '{key}'.");
        }

        if (document["hooks"] is not JsonObject events)
        {
            context.Diagnostics.SpecFatal(relative, "must define a 'hooks' object.");
            return;
        }

        if (events.Count == 0)
        {
            context.Diagnostics.Policy(relative, "declares no hooks. Remove the file instead.");
            return;
        }

        foreach (var (name, node) in events)
        {
            ValidateEvent(plugin, relative, name, node);
        }
    }

    private void ValidateEvent(PluginPackage plugin, string relative, string name, JsonNode? node)
    {
        var declared = HookDialect.Event(name);

        if (declared is null)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"'{name}' is not a neutral hook event. Only events every client can express are " +
                $"allowed: {string.Join(", ", HookDialect.Events)}.");
            return;
        }

        if (node is not JsonArray entries)
        {
            context.Diagnostics.SpecFatal(relative, $"event '{name}' must hold an array of hook entries.");
            return;
        }

        foreach (var entry in entries)
        {
            if (entry is not JsonObject hook)
            {
                context.Diagnostics.SpecFatal(relative, $"every entry under '{name}' must be an object.");
                continue;
            }

            ValidateEntry(plugin, relative, declared, hook);
        }
    }

    private void ValidateEntry(PluginPackage plugin, string relative, HookEvent declared, JsonObject hook)
    {
        foreach (var key in hook.Select(p => p.Key).Where(k => !EntryKeys.Contains(k)))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"hook key '{key}' is not part of the neutral format. Allowed keys: " +
                $"{string.Join(", ", EntryKeys.Order(StringComparer.Ordinal))}.");
        }

        ValidateScriptReference(plugin, relative, declared, hook);
        ValidateMatcher(relative, declared, hook);
        ValidateTimeout(relative, hook);
    }

    /// <summary>
    /// A hook names a script basename, never a command line. The generator owns the invocation, so
    /// nothing authored here can smuggle in shell syntax or reach outside the plugin.
    /// </summary>
    private void ValidateScriptReference(PluginPackage plugin, string relative, HookEvent declared, JsonObject hook)
    {
        var script = hook["script"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(script))
        {
            context.Diagnostics.SpecFatal(
                relative, $"a hook on '{declared.Name}' must name a 'script' in scripts/.");
            return;
        }

        if (!AgentPluginSpec.SkillName.IsMatch(script))
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"hook script '{script}' must be a kebab-case basename of a file in scripts/, not a " +
                "path or a command line.");
            return;
        }

        var definition = plugin.Script(script);

        if (definition is null)
        {
            context.Diagnostics.SpecFatal(
                relative, $"hook script '{script}' has no matching file in scripts/.");
            return;
        }

        // Both halves are mandatory: one hooks.json is shared by macOS and Windows, so a script
        // that exists on only one of them is a hook that silently does nothing on the other.
        if (definition.PosixPath is null)
        {
            context.Diagnostics.SpecFatal(
                relative, $"hook script '{script}' has no scripts/{script}.sh, so it cannot run on macOS or Linux.");
        }

        if (definition.PowerShellPath is null)
        {
            context.Diagnostics.SpecFatal(
                relative, $"hook script '{script}' has no scripts/{script}.ps1, so it cannot run on Windows.");
        }
    }

    private void ValidateMatcher(string relative, HookEvent declared, JsonObject hook)
    {
        if (hook["matcher"] is not { } node)
        {
            return;
        }

        var matcher = node.GetValue<string>();

        if (declared.Subject == MatcherSubject.None)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"event '{declared.Name}' takes no matcher: there is nothing to match against. " +
                "Remove it, or filter inside the script.");
            return;
        }

        if (string.IsNullOrWhiteSpace(matcher))
        {
            context.Diagnostics.SpecFatal(relative, $"matcher on '{declared.Name}' must not be empty.");
            return;
        }

        // The matcher is emitted verbatim inside a double-quoted shell argument, and sh and
        // PowerShell disagree on how these characters escape. A regex that means two different
        // things depending on the platform is worse than one the author has to rewrite, so they
        // are rejected here rather than escaped in the generator.
        var hostile = matcher.Where(c => c is '"' or '\\' or '$' or '`').Distinct().ToArray();

        if (hostile.Length > 0)
        {
            context.Diagnostics.SpecFatal(
                relative,
                $"matcher '{matcher}' on '{declared.Name}' contains {string.Join(" ", hostile.Select(c => $"'{c}'"))}, " +
                "which cannot be quoted the same way in a POSIX shell and in PowerShell. Rewrite it " +
                "without them — for example [0-9] instead of \\d.");
            return;
        }

        try
        {
            _ = Regex.Match(string.Empty, matcher, RegexOptions.None, TimeSpan.FromSeconds(1));
        }
        catch (ArgumentException ex)
        {
            context.Diagnostics.SpecFatal(
                relative, $"matcher '{matcher}' on '{declared.Name}' is not a valid regular expression: {ex.Message}");
        }
    }

    private void ValidateTimeout(string relative, JsonObject hook)
    {
        if (hook["timeout"] is not { } node)
        {
            return;
        }

        if (node.GetValueKind() != System.Text.Json.JsonValueKind.Number)
        {
            context.Diagnostics.SpecFatal(relative, "hook 'timeout' must be a number of seconds.");
            return;
        }

        var timeout = node.GetValue<double>();

        if (timeout is <= 0 or > MaxTimeoutSeconds)
        {
            context.Diagnostics.SpecFatal(
                relative, $"hook 'timeout' must be between 1 and {MaxTimeoutSeconds} seconds.");
        }
    }

    /// <summary>
    /// Scripts are checked even when no hook references them: an orphaned half-pair is either a
    /// leftover or a hook someone forgot to wire up, and both are worth saying out loud.
    /// </summary>
    private void ValidateScriptPairs(PluginPackage plugin)
    {
        foreach (var script in plugin.Scripts.Where(s => !s.IsComplete))
        {
            var present = script.PosixPath ?? script.PowerShellPath!;
            var missing = script.PosixPath is null ? ".sh" : ".ps1";

            context.Diagnostics.Policy(
                context.Relative(present),
                $"has no matching {script.Name}{missing}. Hook scripts ship as a pair so the same " +
                "hook runs on macOS and on Windows.");
        }
    }
}
