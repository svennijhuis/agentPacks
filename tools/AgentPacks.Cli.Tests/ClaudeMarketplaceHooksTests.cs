using System.Text.Json.Nodes;
using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Loading;
using AgentPacks.Cli.Validation;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Claude marketplace entries stay a thin name/source list. Component routing, including a
/// hooks path that actually runs, lives on <c>.claude-plugin/plugin.json</c>. Plugin-root
/// <c>hooks/hooks.json</c> is never a dialect.
/// </summary>
public sealed class ClaudeMarketplaceHooksTests
{
    /// <summary>
    /// Official Claude docs: a marketplace hooks path or array never runs. Every pack omits
    /// the field. Packs that have hooks declare the namespaced file on plugin.json instead.
    /// </summary>
    [Fact]
    public void Claude_marketplace_omits_hooks_path_and_array()
    {
        using var repo = HookedCapabilityPacks();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var plugins = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .ToDictionary(entry => entry["name"]!.GetValue<string>(), StringComparer.Ordinal);

        foreach (var name in (string[])["pack-check", "git"])
        {
            var entry = plugins[name];
            Assert.False(entry.ContainsKey("hooks"));
            Assert.Null(entry["strict"]);
            Assert.Null(entry["skills"]);
            Assert.Equal(
                "./com.anthropic.claude-code/hooks/hooks.json",
                run.File($"plugins/{name}/.claude-plugin/plugin.json").Content["hooks"]!.GetValue<string>());
        }

        using var other = new TestRepository()
            .WithValidPlugin()
            .WithHook("sessionStart");
        var otherRun = other.ValidateAndGenerate();
        var engineering = (JsonObject)otherRun.File(".claude-plugin/marketplace.json")
            .Content["plugins"]![0]!;
        Assert.Null(engineering["hooks"]);
        Assert.Equal(
            "./com.anthropic.claude-code/hooks/hooks.json",
            otherRun.File("plugins/engineering/.claude-plugin/plugin.json")
                .Content["hooks"]!.GetValue<string>());
    }

    /// <summary>
    /// A leftover marketplace hooks path fails compatibility the way Claude Code fails to load it.
    /// </summary>
    [Fact]
    public void Claude_marketplace_rejects_component_fields()
    {
        using var repo = new TestRepository().WithValidPlugin();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var marketplace = (JsonObject)run.File(".claude-plugin/marketplace.json").Content.DeepClone()!;
        var entry = (JsonObject)marketplace["plugins"]![0]!;
        entry["hooks"] = "./com.anthropic.claude-code/hooks/hooks.json";
        entry["strict"] = true;

        var context = new RepositoryContext { Root = repo.Root };
        new CompatibilityValidator(context).Validate(
            [GeneratedFile.FromJson(".claude-plugin/marketplace.json", marketplace)]);

        var text = context.Diagnostics.Render();
        Assert.True(context.Diagnostics.HasErrors, text);
        Assert.Contains("declares 'hooks'", text, StringComparison.Ordinal);
        Assert.Contains("declares 'strict'", text, StringComparison.Ordinal);
        Assert.Contains("never runs", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Namespaced Claude hooks stay Claude-shaped. Copilot's namespaced tree stays Copilot-shaped.
    /// Plugin-root hooks/hooks.json is not written.
    /// </summary>
    [Fact]
    public void Claude_namespaced_hooks_json_is_claude_shaped()
    {
        using var repo = HookedCapabilityPacks();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        foreach (var plugin in (string[])["pack-check", "git"])
        {
            var root = $"plugins/{plugin}/hooks/hooks.json";
            var claude = $"plugins/{plugin}/com.anthropic.claude-code/hooks/hooks.json";
            var copilot = $"plugins/{plugin}/com.github.copilot/hooks/hooks.json";

            Assert.False(run.HasFile(root), root);
            Assert.True(run.HasFile(claude), claude);
            Assert.True(run.HasFile(copilot), copilot);

            var claudeDoc = run.File(claude).Content;
            var copilotDoc = run.File(copilot).Content;

            Assert.Null(claudeDoc["version"]);
            Assert.Equal(1, copilotDoc["version"]!.GetValue<int>());

            var claudeEvents = (JsonObject)claudeDoc["hooks"]!;
            var copilotEvents = (JsonObject)copilotDoc["hooks"]!;
            Assert.All(claudeEvents, pair => Assert.Matches("^[A-Z]", pair.Key));

            var claudeEntry = FirstHookEntry(claudeEvents);
            var copilotEntry = FirstHookEntry(copilotEvents);

            Assert.True(claudeEntry["hooks"] is JsonArray);
            Assert.Contains("command", ((JsonObject)((JsonArray)claudeEntry["hooks"]!)[0]!).ToJsonString(), StringComparison.Ordinal);
            Assert.DoesNotContain("bash", claudeEntry.ToJsonString(), StringComparison.Ordinal);
            Assert.DoesNotContain("${PLUGIN_ROOT}", claudeEntry.ToJsonString(), StringComparison.Ordinal);
            Assert.Contains("${CLAUDE_PLUGIN_ROOT}", claudeEntry.ToJsonString(), StringComparison.Ordinal);

            Assert.Null(copilotEntry["hooks"]);
            Assert.Contains("bash", copilotEntry.ToJsonString(), StringComparison.Ordinal);
            Assert.Contains("${PLUGIN_ROOT}", copilotEntry.ToJsonString(), StringComparison.Ordinal);
            Assert.DoesNotContain("${CLAUDE_PLUGIN_ROOT}", copilotEntry.ToJsonString(), StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Item 50 fail bar. Copilot CLI fail-closes PreToolUse when a hook script
    /// cannot be resolved from the project cwd (github/copilot-cli#3659).
    /// Generated Copilot hooks.json for git and pack-check must set
    /// <c>cwd</c> to <c>${PLUGIN_ROOT}</c> on every script-running entry.
    /// Claude namespaced hooks stay Claude-shaped with no Copilot cwd, and the
    /// marketplace still omits a hooks path.
    /// </summary>
    [Fact]
    public void Copilot_hooks_set_cwd_plugin_root()
    {
        using var repo = HookedCapabilityPacks();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        foreach (var plugin in (string[])["pack-check", "git"])
        {
            var copilotPath = $"plugins/{plugin}/com.github.copilot/hooks/hooks.json";
            var claudePath = $"plugins/{plugin}/com.anthropic.claude-code/hooks/hooks.json";
            Assert.True(run.HasFile(copilotPath), copilotPath);

            var copilotEvents = (JsonObject)run.File(copilotPath).Content["hooks"]!;
            var entries = CopilotHookEntries(copilotEvents).ToArray();
            Assert.NotEmpty(entries);

            foreach (var entry in entries)
            {
                Assert.Equal("command", entry["type"]!.GetValue<string>());
                Assert.Contains("bash", entry.ToJsonString(), StringComparison.Ordinal);
                Assert.Equal("${PLUGIN_ROOT}", entry["cwd"]!.GetValue<string>());
            }

            var claudeEvents = (JsonObject)run.File(claudePath).Content["hooks"]!;
            var claudeGroup = FirstHookEntry(claudeEvents);
            Assert.Null(claudeGroup["cwd"]);
            Assert.Null(((JsonObject)((JsonArray)claudeGroup["hooks"]!)[0]!)["cwd"]);
            Assert.DoesNotContain("\"cwd\"", run.File(claudePath).Text, StringComparison.Ordinal);
        }

        Claude_marketplace_omits_hooks_path_and_array();
        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Cursor-shaped hooks always live under .cursor-plugin/. Plugin-root hooks/hooks.json is
    /// not written, so Claude's plugin.json can declare a hooks path without merging Cursor's
    /// document. Folder discovery is replaced by the Cursor manifest path.
    /// </summary>
    [Fact]
    public void Cursor_loads_relocated_cursor_shaped_hooks()
    {
        using var repo = HookedCapabilityPacks();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        foreach (var plugin in (string[])["pack-check", "git"])
        {
            var cursorPath = $"plugins/{plugin}/{ClientProfile.RelocatedCursorHooks}";
            var rootPath = $"plugins/{plugin}/hooks/hooks.json";
            var manifestPath = $"plugins/{plugin}/.cursor-plugin/plugin.json";

            Assert.True(run.HasFile(cursorPath), cursorPath);
            Assert.False(run.HasFile(rootPath), rootPath);

            var cursorDoc = run.File(cursorPath).Content;
            var cursorEvents = (JsonObject)cursorDoc["hooks"]!;

            Assert.All(cursorEvents, pair => Assert.Matches("^[a-z]", pair.Key));

            var cursorEntry = FirstHookEntry(cursorEvents);
            Assert.Null(cursorEntry["hooks"]);
            Assert.Equal("command", cursorEntry["type"]!.GetValue<string>());
            Assert.Contains("scripts/", cursorEntry["command"]!.GetValue<string>(), StringComparison.Ordinal);

            Assert.Equal(
                $"./{ClientProfile.RelocatedCursorHooks}",
                run.File(manifestPath).Content["hooks"]!.GetValue<string>());
        }

        using var other = new TestRepository()
            .WithValidPlugin()
            .WithHook("beforeShellExecution");
        var otherRun = other.ValidateAndGenerate();
        Assert.False(otherRun.HasFile("plugins/engineering/hooks/hooks.json"));
        Assert.True(otherRun.HasFile($"plugins/engineering/{ClientProfile.RelocatedCursorHooks}"));
        Assert.Equal(
            $"./{ClientProfile.RelocatedCursorHooks}",
            otherRun.File("plugins/engineering/.cursor-plugin/plugin.json").Content["hooks"]!.GetValue<string>());
    }

    private static IEnumerable<JsonObject> CopilotHookEntries(JsonObject events)
    {
        foreach (var pair in events)
        {
            foreach (var item in pair.Value!.AsArray())
                yield return (JsonObject)item!;
        }
    }

    private static JsonObject FirstHookEntry(JsonObject events) =>
        (JsonObject)events.First().Value!.AsArray()[0]!;

    private static TestRepository HookedCapabilityPacks()
    {
        var root = TestRepository.SourceRoot();
        var repo = new TestRepository()
            .WithPlugin("pack-check", File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "plugin.json")))
            .WithSkill("pack-check", plugin: "pack-check")
            .WithFile(
                "plugins/pack-check/hooks.source.json",
                File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "hooks.source.json")))
            .WithScript("pack-check-session", plugin: "pack-check")
            .WithPlugin("git", File.ReadAllText(Path.Combine(root, "plugins", "git", "plugin.json")))
            .WithFile(
                "plugins/git/hooks.source.json",
                File.ReadAllText(Path.Combine(root, "plugins", "git", "hooks.source.json")))
            .WithScript("git-guard", plugin: "git");

        return repo;
    }
}
