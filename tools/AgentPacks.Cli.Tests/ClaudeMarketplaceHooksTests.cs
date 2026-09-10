using System.Text.Json.Nodes;
using AgentPacks.Cli.Generation;

namespace AgentPacks.Cli.Tests;

/// <summary>Po 47 fail bars: Claude marketplace omits hooks path/array; root hooks are Claude-shaped.</summary>
public sealed class ClaudeMarketplaceHooksTests
{
    /// <summary>
    /// Po 47 fail bar. Claude rejects a marketplace <c>hooks</c> file-path or array.
    /// <c>pack-check</c> and <c>git</c> omit the field entirely so install can succeed.
    /// Other plugins with hooks still declare the namespaced path.
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
            Assert.Null(entry["hooks"]);
            Assert.False(entry["hooks"] is JsonArray);
            Assert.DoesNotContain("hooks/hooks.json", entry.ToJsonString(), StringComparison.Ordinal);
        }

        using var other = new TestRepository()
            .WithValidPlugin()
            .WithHook("sessionStart");
        var otherRun = other.ValidateAndGenerate();
        var engineering = (JsonObject)otherRun.File(".claude-plugin/marketplace.json")
            .Content["plugins"]![0]!;
        Assert.Equal("./com.anthropic.claude-code/hooks/hooks.json", engineering["hooks"]!.GetValue<string>());
    }

    /// <summary>
    /// Po 47 fail bar. Claude auto-discovers plugin-root <c>hooks/hooks.json</c> when the
    /// marketplace omits the field. That file is the Claude dialect (nested, PascalCase,
    /// <c>command</c>), copied from <c>com.anthropic.claude-code/hooks/</c>. Copilot's
    /// namespaced tree stays Copilot-shaped.
    /// </summary>
    [Fact]
    public void Claude_plugin_root_hooks_json_is_claude_shaped()
    {
        using var repo = HookedCapabilityPacks();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        foreach (var plugin in (string[])["pack-check", "git"])
        {
            var root = $"plugins/{plugin}/hooks/hooks.json";
            var claude = $"plugins/{plugin}/com.anthropic.claude-code/hooks/hooks.json";
            var copilot = $"plugins/{plugin}/com.github.copilot/hooks/hooks.json";

            Assert.True(run.HasFile(root), root);
            Assert.True(run.HasFile(claude), claude);
            Assert.True(run.HasFile(copilot), copilot);

            Assert.Equal(run.File(claude).Text, run.File(root).Text);

            var rootDoc = run.File(root).Content;
            var copilotDoc = run.File(copilot).Content;

            Assert.Null(rootDoc["version"]);
            Assert.Equal(1, copilotDoc["version"]!.GetValue<int>());

            var rootEvents = (JsonObject)rootDoc["hooks"]!;
            var copilotEvents = (JsonObject)copilotDoc["hooks"]!;
            Assert.All(rootEvents, pair => Assert.Matches("^[A-Z]", pair.Key));

            var rootEntry = FirstHookEntry(rootEvents);
            var copilotEntry = FirstHookEntry(copilotEvents);

            Assert.True(rootEntry["hooks"] is JsonArray);
            Assert.Contains("command", ((JsonObject)((JsonArray)rootEntry["hooks"]!)[0]!).ToJsonString(), StringComparison.Ordinal);
            Assert.DoesNotContain("bash", rootEntry.ToJsonString(), StringComparison.Ordinal);
            Assert.DoesNotContain("${PLUGIN_ROOT}", rootEntry.ToJsonString(), StringComparison.Ordinal);
            Assert.Contains("${CLAUDE_PLUGIN_ROOT}", rootEntry.ToJsonString(), StringComparison.Ordinal);

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
    /// Claude root hooks stay Claude-shaped with no Copilot cwd, and the
    /// marketplace still omits a hooks path (item 47).
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
            var rootPath = $"plugins/{plugin}/hooks/hooks.json";
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

            var rootEvents = (JsonObject)run.File(rootPath).Content["hooks"]!;
            var rootGroup = FirstHookEntry(rootEvents);
            Assert.Null(rootGroup["cwd"]);
            Assert.Null(((JsonObject)((JsonArray)rootGroup["hooks"]!)[0]!)["cwd"]);
            Assert.DoesNotContain("\"cwd\"", run.File(rootPath).Text, StringComparison.Ordinal);
        }

        Claude_marketplace_omits_hooks_path_and_array();
        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Claude owns plugin-root hooks/hooks.json on git and pack-check. Cursor cannot read that
    /// dialect, and until the Cursor manifest pointed at a Cursor-shaped copy the guard never
    /// fired on Cursor (including Windows). Folder discovery stays on root hooks/ for other packs.
    /// </summary>
    [Fact]
    public void Cursor_loads_relocated_cursor_shaped_hooks_when_claude_owns_root()
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
            Assert.True(run.HasFile(rootPath), rootPath);
            Assert.NotEqual(run.File(rootPath).Text, run.File(cursorPath).Text);

            var cursorDoc = run.File(cursorPath).Content;
            var rootDoc = run.File(rootPath).Content;
            var cursorEvents = (JsonObject)cursorDoc["hooks"]!;
            var rootEvents = (JsonObject)rootDoc["hooks"]!;

            Assert.All(cursorEvents, pair => Assert.Matches("^[a-z]", pair.Key));
            Assert.All(rootEvents, pair => Assert.Matches("^[A-Z]", pair.Key));

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
        Assert.True(otherRun.HasFile("plugins/engineering/hooks/hooks.json"));
        Assert.False(otherRun.HasFile($"plugins/engineering/{ClientProfile.RelocatedCursorHooks}"));
        Assert.Null(otherRun.File("plugins/engineering/.cursor-plugin/plugin.json").Content["hooks"]);
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
        var root = SourceRoot();
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

    private static string SourceRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "plugins")) &&
                Directory.Exists(Path.Combine(directory.FullName, "tools", "AgentPacks.Cli")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the agentPacks source root.");
    }
}
