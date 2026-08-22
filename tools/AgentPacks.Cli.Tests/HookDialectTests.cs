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
        { "sessionStart", "SessionStart", "sessionStart", "SessionStart", "sessionStart" },
        { "sessionEnd", "SessionEnd", "sessionEnd", "SessionEnd", "sessionEnd" },
        { "userPromptSubmit", "UserPromptSubmit", "beforeSubmitPrompt", "UserPromptSubmit", "userPromptSubmit" },
        { "stop", "Stop", "stop", "Stop", "stop" },
        { "preToolUse", "PreToolUse", "preToolUse", "PreToolUse", "preToolUse" },
        { "postToolUse", "PostToolUse", "postToolUse", "PostToolUse", "postToolUse" },
        { "beforeShellExecution", "PreToolUse", "beforeShellExecution", "PreToolUse", "preToolUse" },
        { "afterFileEdit", "PostToolUse", "afterFileEdit", "PostToolUse", "postToolUse" }
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
        Assert.Contains("--matcher \"rm +-rf\"", command, StringComparison.Ordinal);
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
        Assert.DoesNotContain("--matcher", entry["command"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    /// <summary>A tool matcher means the same thing everywhere, so every client applies it itself.</summary>
    [Fact]
    public void Tool_matcher_is_applied_natively_by_every_client()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("preToolUse", "Read").ValidateAndGenerate();

        foreach (var (path, name) in ((string, string)[])
                 [(ClaudeHooks, "PreToolUse"), (CodexHooks, "PreToolUse"), (CopilotHooks, "preToolUse")])
        {
            var group = (JsonObject)Events(run, path)[name]![0]!;
            var command = ((JsonArray)group["hooks"]!)[0]!["command"]!.GetValue<string>();

            Assert.Equal("Read", group["matcher"]!.GetValue<string>());
            Assert.DoesNotContain("--matcher", command, StringComparison.Ordinal);
        }
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
    /// Codex is the only client with a per-OS command field. The others rely on the generated .cmd
    /// shim, so emitting commandWindows for them would be a key they silently ignore.
    /// </summary>
    [Fact]
    public void Only_codex_carries_a_windows_command()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var codex = (JsonObject)((JsonArray)Events(run, CodexHooks)["SessionStart"]![0]!["hooks"]!)[0]!;
        var claude = (JsonObject)((JsonArray)Events(run, ClaudeHooks)["SessionStart"]![0]!["hooks"]!)[0]!;
        var copilot = (JsonObject)((JsonArray)Events(run, CopilotHooks)["sessionStart"]![0]!["hooks"]!)[0]!;
        var cursor = (JsonObject)Events(run, CursorHooks)["sessionStart"]![0]!;

        Assert.Contains("guard.ps1", codex["commandWindows"]!.GetValue<string>(), StringComparison.Ordinal);
        Assert.Null(claude["commandWindows"]);
        Assert.Null(copilot["commandWindows"]);
        Assert.Null(cursor["commandWindows"]);
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
