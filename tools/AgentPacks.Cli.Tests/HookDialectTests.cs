using System.Text.Json.Nodes;
using AgentPacks.Cli.Generation;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// The neutral hook vocabulary against the four dialects. Every difference between the clients is
/// a place where a wrong translation produces a file that loads fine and silently never fires, so
/// each row of the mapping is pinned here rather than trusted to review.
/// </summary>
public sealed class HookDialectTests
{
    private const string ClaudeHooks = "plugins/engineering/com.anthropic.claude-code/hooks/hooks.json";
    private const string CursorHooks = "plugins/engineering/hooks/hooks.json";
    private const string CodexHooks = "plugins/engineering/com.openai.codex/hooks/hooks.json";
    private const string CopilotHooks = "plugins/engineering/com.github.copilot/hooks/hooks.json";

    public static TheoryData<string, string, string, string, string> EventMappings() => new()
    {
        // neutral, Claude, Cursor, Codex, Copilot
        { "sessionStart", "SessionStart", "sessionStart", "SessionStart", "SessionStart" },
        { "sessionEnd", "SessionEnd", "sessionEnd", "SessionEnd", "SessionEnd" },
        { "userPromptSubmit", "UserPromptSubmit", "beforeSubmitPrompt", "UserPromptSubmit", "UserPromptSubmit" },
        { "stop", "Stop", "stop", "Stop", "Stop" },
        { "preToolUse", "PreToolUse", "preToolUse", "PreToolUse", "PreToolUse" },
        { "postToolUse", "PostToolUse", "postToolUse", "PostToolUse", "PostToolUse" },
        { "beforeShellExecution", "PreToolUse", "beforeShellExecution", "PreToolUse", "PreToolUse" },
        { "afterFileEdit", "PostToolUse", "afterFileEdit", "PostToolUse", "PostToolUse" }
    };

    [Theory]
    [MemberData(nameof(EventMappings))]
    public void Every_neutral_event_maps_to_each_client_dialect(
        string neutral,
        string claude,
        string cursor,
        string codex,
        string copilot)
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook(neutral).ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.NotNull(Events(run, ClaudeHooks)[claude]);
        Assert.NotNull(Events(run, CursorHooks)[cursor]);
        Assert.NotNull(Events(run, CodexHooks)[codex]);
        Assert.NotNull(Events(run, CopilotHooks)[copilot]);
    }

    [Fact]
    public void Vocabulary_is_covered_by_the_mapping_table()
    {
        // Guards the table above: a new neutral event added to HookDialect without a row here
        // would otherwise ship untested.
        var tested = EventMappings().Select(row => (string)row[0]!).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(HookDialect.Events.Order(StringComparer.Ordinal), tested.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Cursor_puts_the_command_flat_in_the_event_array()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var entry = (JsonObject)Events(run, CursorHooks)["sessionStart"]![0]!;

        Assert.Equal("command", entry["type"]!.GetValue<string>());
        Assert.Contains("scripts/guard", entry["command"]!.GetValue<string>(), StringComparison.Ordinal);
        Assert.Null(entry["hooks"]);
    }

    [Fact]
    public void Claude_and_codex_nest_the_command_under_the_matcher_object()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        foreach (var path in (string[])[ClaudeHooks, CodexHooks])
        {
            var group = (JsonObject)Events(run, path)["SessionStart"]![0]!;
            var nested = (JsonObject)((JsonArray)group["hooks"]!)[0]!;

            Assert.Equal("command", nested["type"]!.GetValue<string>());
            Assert.Null(group["command"]);
        }
    }

    /// <summary>
    /// The case that motivated MatcherSubject. Claude's PreToolUse matcher filters on the tool
    /// name, which is already spent naming Bash, so an authored matcher about command text must go
    /// to the script instead of overwriting the tool matcher.
    /// </summary>
    [Fact]
    public void Shell_matcher_reaches_claude_through_the_script_not_the_tool_matcher()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin()
            .WithHook("beforeShellExecution", "rm +-rf")
            .ValidateAndGenerate();

        var group = (JsonObject)Events(run, ClaudeHooks)["PreToolUse"]![0]!;
        var command = ((JsonArray)group["hooks"]!)[0]!["command"]!.GetValue<string>();

        Assert.Equal("Bash", group["matcher"]!.GetValue<string>());
        Assert.Contains("-Matcher \"rm +-rf\"", command, StringComparison.Ordinal);
    }

    /// <summary>Cursor's shell matcher does filter command text, so it is applied natively.</summary>
    [Fact]
    public void Shell_matcher_is_applied_natively_by_cursor()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin()
            .WithHook("beforeShellExecution", "rm +-rf")
            .ValidateAndGenerate();

        var entry = (JsonObject)Events(run, CursorHooks)["beforeShellExecution"]![0]!;

        Assert.Equal("rm +-rf", entry["matcher"]!.GetValue<string>());
        Assert.DoesNotContain("-Matcher", entry["command"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    /// <summary>A tool matcher means the same thing everywhere, so every client applies it itself.</summary>
    [Fact]
    public void Tool_matcher_is_applied_natively_by_every_client()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("preToolUse", "Read").ValidateAndGenerate();

        foreach (var (path, name) in ((string, string)[])
                 [(ClaudeHooks, "PreToolUse"), (CodexHooks, "PreToolUse")])
        {
            var group = (JsonObject)Events(run, path)[name]![0]!;
            var command = ((JsonArray)group["hooks"]!)[0]!["command"]!.GetValue<string>();

            Assert.Equal("Read", group["matcher"]!.GetValue<string>());
            Assert.DoesNotContain("-Matcher", command, StringComparison.Ordinal);
        }

        // Copilot puts the entry and its matcher flat under the event, with no group around them.
        var copilot = (JsonObject)Events(run, CopilotHooks)["PreToolUse"]![0]!;

        Assert.Equal("Read", copilot["matcher"]!.GetValue<string>());
        Assert.DoesNotContain("-Matcher", copilot["bash"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    [Fact]
    public void File_edit_event_carries_the_write_and_edit_tools_on_claude()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("afterFileEdit").ValidateAndGenerate();

        var group = (JsonObject)Events(run, ClaudeHooks)["PostToolUse"]![0]!;

        Assert.Equal("Write|Edit", group["matcher"]!.GetValue<string>());
    }

    /// <summary>
    /// Codex and Copilot each have a per-OS command field, spelled differently: Codex reads a full
    /// shell invocation from "commandWindows", Copilot reads the PowerShell command itself from
    /// "powershell". Claude and Cursor have neither and rely on the generated .cmd shim, so
    /// emitting either key for them would be a key they silently ignore.
    /// </summary>
    [Fact]
    public void Codex_and_copilot_carry_a_windows_command_in_their_own_spelling()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var codex = (JsonObject)((JsonArray)Events(run, CodexHooks)["SessionStart"]![0]!["hooks"]!)[0]!;
        var claude = (JsonObject)((JsonArray)Events(run, ClaudeHooks)["SessionStart"]![0]!["hooks"]!)[0]!;
        var copilot = (JsonObject)Events(run, CopilotHooks)["SessionStart"]![0]!;
        var cursor = (JsonObject)Events(run, CursorHooks)["sessionStart"]![0]!;

        var codexWindows = codex["commandWindows"]!.GetValue<string>();
        var copilotWindows = copilot["powershell"]!.GetValue<string>();

        Assert.Contains("guard.ps1", codexWindows, StringComparison.Ordinal);
        Assert.StartsWith("powershell -NoProfile", codexWindows, StringComparison.Ordinal);

        // Already a PowerShell context, so the interpreter is not named a second time.
        Assert.Contains("guard.ps1", copilotWindows, StringComparison.Ordinal);
        Assert.StartsWith("& ", copilotWindows, StringComparison.Ordinal);

        Assert.Null(claude["commandWindows"]);
        Assert.Null(claude["powershell"]);
        Assert.Null(copilot["commandWindows"]);
        Assert.Null(cursor["commandWindows"]);
        Assert.Null(cursor["powershell"]);
    }

    /// <summary>
    /// Copilot's hook document declares its format version and puts entries flat under the event.
    /// The other three carry neither, and a version key they do not read is a key that can only
    /// confuse a schema check.
    /// </summary>
    [Fact]
    public void Only_the_copilot_hook_document_declares_a_version()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        Assert.Equal(1, run.File(CopilotHooks).Content["version"]!.GetValue<int>());
        Assert.Null(run.File(ClaudeHooks).Content["version"]);
        Assert.Null(run.File(CodexHooks).Content["version"]);
        Assert.Null(run.File(CursorHooks).Content["version"]);

        // Flat: the entry itself, not a matcher group wrapping a nested "hooks" array.
        var entry = (JsonObject)Events(run, CopilotHooks)["SessionStart"]![0]!;

        Assert.Null(entry["hooks"]);
        Assert.Equal("command", entry["type"]!.GetValue<string>());
        Assert.Contains("${PLUGIN_ROOT}/scripts/", entry["bash"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    /// <summary>The command is extensionless so cmd.exe can resolve the .cmd shim through PATHEXT.</summary>
    [Fact]
    public void Posix_command_is_extensionless_and_rooted_at_the_plugin()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var claude = ((JsonArray)Events(run, ClaudeHooks)["SessionStart"]![0]!["hooks"]!)[0]!["command"]!
            .GetValue<string>();

        Assert.Equal("\"${CLAUDE_PLUGIN_ROOT}/scripts/guard\"", claude);
    }

    [Fact]
    public void Codex_commands_use_the_installed_plugin_root_on_both_platforms()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var command = (JsonObject)((JsonArray)Events(run, CodexHooks)["SessionStart"]![0]!["hooks"]!)[0]!;

        Assert.Equal("\"${PLUGIN_ROOT}/scripts/guard\"", command["command"]!.GetValue<string>());
        Assert.Contains("\"${PLUGIN_ROOT}/scripts/guard.ps1\"", command["commandWindows"]!.GetValue<string>());
    }

    /// <summary>
    /// The extensionless command has to name a file that exists. Only .sh and .ps1 are authored, so
    /// the generator owes the POSIX side a dispatcher: without it every hook resolves to nothing on
    /// macOS and Linux and fails with "No such file or directory".
    /// </summary>
    [Fact]
    public void Every_hook_command_resolves_to_a_file_that_is_generated()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var command = ((JsonArray)Events(run, ClaudeHooks)["SessionStart"]![0]!["hooks"]!)[0]!["command"]!
            .GetValue<string>()
            .Trim('"');

        var pluginRelative = command.Replace("${CLAUDE_PLUGIN_ROOT}/", string.Empty, StringComparison.Ordinal);

        Assert.True(run.HasFile($"plugins/engineering/{pluginRelative}"));
        Assert.True(run.File($"plugins/engineering/{pluginRelative}").Executable);
        Assert.True(run.HasFile($"plugins/engineering/{pluginRelative}.cmd"));
    }

    /// <summary>
    /// The dispatcher only forwards. Its whole reason to exist is that one extensionless string has
    /// to work in a POSIX shell and under cmd.exe, so it must reach the authored .sh with the
    /// arguments untouched — the matcher travels in them.
    /// </summary>
    [Fact]
    public void The_posix_dispatcher_forwards_to_the_authored_script()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var dispatcher = run.File("plugins/engineering/scripts/guard").Text;

        Assert.StartsWith("#!/usr/bin/env bash", dispatcher, StringComparison.Ordinal);
        Assert.Contains("guard.sh\" \"$@\"", dispatcher, StringComparison.Ordinal);
    }

    /// <summary>
    /// The matcher argument has to bind in two parsers. PowerShell has no double-dash parameter
    /// names, so "--matcher" would be swallowed as the value of param($Matcher) and the regex
    /// behind it would fail to bind, taking the whole hook down on Windows.
    /// </summary>
    [Fact]
    public void The_matcher_argument_binds_as_a_powershell_parameter()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithHook("beforeShellExecution", "rm +-rf")
            .ValidateAndGenerate();

        var windows = ((JsonArray)Events(run, CodexHooks)["PreToolUse"]![0]!["hooks"]!)[0]!["commandWindows"]!
            .GetValue<string>();

        Assert.StartsWith("-", HookDialect.MatcherArgument, StringComparison.Ordinal);
        Assert.DoesNotContain("--", HookDialect.MatcherArgument, StringComparison.Ordinal);
        Assert.Contains("-Matcher \"rm +-rf\"", windows, StringComparison.Ordinal);
    }

    /// <summary>
    /// PascalCase PreToolUse events use the Claude-compatible Bash matcher. Copilot's lowercase
    /// bash and powershell fields are commands, not matcher tool names.
    /// </summary>
    [Theory]
    [InlineData(ClaudeHooks, "Bash")]
    [InlineData(CodexHooks, "Bash")]
    [InlineData(CopilotHooks, "Bash")]
    public void PascalCase_shell_hooks_use_the_Bash_matcher_alias(string path, string expected)
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("beforeShellExecution", "git +reset").ValidateAndGenerate();

        var entry = (JsonObject)Events(run, path)["PreToolUse"]![0]!;

        Assert.Equal(expected, entry["matcher"]!.GetValue<string>());
    }

    /// <summary>
    /// Copilot's PascalCase PostToolUse event uses the Claude-compatible Write and Edit aliases;
    /// camelCase runtime tool names must not be mixed into this matcher vocabulary.
    /// </summary>
    [Theory]
    [InlineData(ClaudeHooks, "Write|Edit")]
    [InlineData(CodexHooks, "apply_patch")]
    [InlineData(CopilotHooks, "Write|Edit")]
    public void File_edit_hooks_use_the_provider_event_matcher_vocabulary(string path, string expected)
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("afterFileEdit").ValidateAndGenerate();

        var entry = (JsonObject)Events(run, path)["PostToolUse"]![0]!;

        Assert.Equal(expected, entry["matcher"]!.GetValue<string>());
    }

    [Fact]
    public void Two_hooks_on_one_event_with_the_same_matcher_share_one_group()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithScript("audit")
            .WithHooks("""
                {
                  "hooks": {
                    "sessionStart": [{ "script": "guard" }, { "script": "audit" }]
                  }
                }
                """)
            .ValidateAndGenerate();

        var groups = Events(run, ClaudeHooks)["SessionStart"]!.AsArray();

        Assert.Single(groups);
        Assert.Equal(2, ((JsonArray)groups[0]!["hooks"]!).Count);
    }

    [Fact]
    public void A_plugin_without_hooks_generates_no_hooks_file()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().ValidateAndGenerate();

        Assert.False(run.HasFile(ClaudeHooks));
        Assert.False(run.HasFile(CursorHooks));
        Assert.False(run.HasFile(CodexHooks));
        Assert.False(run.HasFile(CopilotHooks));
    }

    private static JsonObject Events(ValidationRun run, string path) =>
        (JsonObject)run.File(path).Content["hooks"]!;
}
