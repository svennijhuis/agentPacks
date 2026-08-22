using System.Text.Json.Nodes;
using AgentPacks.Cli.Commands;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// The manifests that decide which tree each client reads, and the staleness sweep that removes a
/// tree once its source is gone.
/// </summary>
public sealed class ClientManifestTests
{
    private const string Plugin = "plugins/engineering";
    private const string Marketplace = ".claude-plugin/marketplace.json";

    /// <summary>
    /// Claude auto-discovers agents/, commands/ and hooks/ at the plugin root, but the root holds
    /// Cursor's dialect. Without these explicit paths Claude would load Cursor's files.
    /// </summary>
    [Fact]
    public void The_claude_entry_points_at_its_own_namespace()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer")
            .WithCommand("review-diff")
            .WithHook("sessionStart")
            .ValidateAndGenerate();

        var entry = (JsonObject)run.File(Marketplace).Content["plugins"]![0]!;

        Assert.Equal("./com.anthropic.claude-code/agents/", entry["agents"]![0]!.GetValue<string>());
        Assert.Equal("./com.anthropic.claude-code/commands/", entry["commands"]![0]!.GetValue<string>());
        Assert.Equal("./com.anthropic.claude-code/hooks/hooks.json", entry["hooks"]!.GetValue<string>());
    }

    [Fact]
    public void The_claude_entry_declares_only_the_components_that_exist()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().ValidateAndGenerate();

        var entry = (JsonObject)run.File(Marketplace).Content["plugins"]![0]!;

        Assert.Null(entry["agents"]);
        Assert.Null(entry["commands"]);
        Assert.Null(entry["hooks"]);
        Assert.Equal("./skills/", entry["skills"]!.GetValue<string>());
    }

    /// <summary>Rules reach Claude only as a SessionStart hook, so they imply the hooks path too.</summary>
    [Fact]
    public void A_plugin_with_only_rules_still_declares_the_claude_hooks_path()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards").ValidateAndGenerate();

        var entry = (JsonObject)run.File(Marketplace).Content["plugins"]![0]!;

        Assert.Equal("./com.anthropic.claude-code/hooks/hooks.json", entry["hooks"]!.GetValue<string>());
    }

    /// <summary>
    /// Cursor has no documented way to be pointed elsewhere, so it keeps the plugin root and only
    /// needs the manifest that turns an Agent Plugin into a Cursor plugin.
    /// </summary>
    [Fact]
    public void The_cursor_manifest_carries_the_plugin_identity()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().ValidateAndGenerate();

        var cursor = run.File($"{Plugin}/.cursor-plugin/plugin.json").Content;

        Assert.Equal("engineering", cursor["name"]!.GetValue<string>());
        Assert.Equal("Test plugin.", cursor["description"]!.GetValue<string>());
    }

    [Fact]
    public void The_codex_manifest_points_hooks_outside_the_cursor_root()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var codex = run.File($"{Plugin}/.codex-plugin/plugin.json").Content;

        Assert.Equal("./com.openai.codex/hooks/hooks.json", codex["hooks"]!.GetValue<string>());
        Assert.Equal("./skills/", codex["skills"]!.GetValue<string>());
    }

    [Fact]
    public void The_codex_manifest_omits_hooks_when_the_plugin_declares_none()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().ValidateAndGenerate();

        Assert.Null(run.File($"{Plugin}/.codex-plugin/plugin.json").Content["hooks"]);
    }

    // -------------------------------------------------------------- staleness

    /// <summary>
    /// A deleted agent has to disappear from all four trees. Leaving it behind means an agent
    /// nobody declares any more still answers when called.
    /// </summary>
    [Fact]
    public void A_removed_agent_is_deleted_from_every_client_tree()
    {
        using var repo = new TestRepository();
        repo.WithValidPlugin().WithAgent("reviewer").ValidateAndGenerate();

        var generated = (string[])
        [
            $"{Plugin}/com.anthropic.claude-code/agents/reviewer.md",
            $"{Plugin}/com.github.copilot/agents/reviewer.agent.md",
            $"{Plugin}/com.openai.codex/agents/reviewer.toml"
        ];

        Assert.All(generated, path => Assert.True(File.Exists(Path.Combine(repo.Root, path))));

        File.Delete(Path.Combine(repo.Root, Plugin, "agents", "reviewer.md"));
        repo.ValidateAndGenerate();

        Assert.All(generated, path => Assert.False(File.Exists(Path.Combine(repo.Root, path))));
    }

    [Fact]
    public void A_left_over_generated_file_is_reported_when_checking()
    {
        using var repo = new TestRepository();
        repo.WithValidPlugin().WithAgent("reviewer").ValidateAndGenerate();

        File.Delete(Path.Combine(repo.Root, Plugin, "agents", "reviewer.md"));
        var run = repo.ValidateAndGenerate(new CommandOptions { Check = true });

        Assert.True(run.HasErrors);
        Assert.Contains("is left over from a previous generation", run.Text, StringComparison.Ordinal);
    }

    /// <summary>Authored files must never be mistaken for generated ones and swept away.</summary>
    [Fact]
    public void Authored_source_survives_the_staleness_sweep()
    {
        using var repo = new TestRepository();
        repo.WithValidPlugin().WithAgent("reviewer").WithRule("standards").WithHook("sessionStart");
        repo.ValidateAndGenerate();

        foreach (var path in (string[])
                 [
                     $"{Plugin}/plugin.json",
                     $"{Plugin}/agents/reviewer.md",
                     $"{Plugin}/rules/standards.mdc",
                     $"{Plugin}/hooks.source.json",
                     $"{Plugin}/scripts/guard.sh",
                     $"{Plugin}/scripts/guard.ps1",
                     $"{Plugin}/skills/dotnet-review/SKILL.md"
                 ])
        {
            Assert.True(File.Exists(Path.Combine(repo.Root, path)), path);
        }
    }

    // ------------------------------------------------------------ determinism

    [Fact]
    public void Generation_is_byte_identical_between_runs()
    {
        using var repo = new TestRepository();
        repo.WithValidPlugin().WithAgent("reviewer").WithRule("standards").WithHook("beforeShellExecution", "rm +-rf");

        var first = repo.ValidateAndGenerate().Generated.Select(f => (f.RelativePath, f.Text)).ToList();
        var second = repo.ValidateAndGenerate().Generated.Select(f => (f.RelativePath, f.Text)).ToList();

        Assert.Equal(first, second);
    }

    [Fact]
    public void Freshly_generated_output_passes_check()
    {
        using var repo = new TestRepository();
        repo.WithValidPlugin().WithAgent("reviewer").WithRule("standards").WithHook("sessionStart");
        repo.ValidateAndGenerate();

        var run = repo.ValidateAndGenerate(new CommandOptions { Check = true });

        Assert.False(run.HasErrors, run.Text);
    }

    /// <summary>
    /// The seven existing packs declare none of the new components. They must generate exactly what
    /// they generated before, or this change quietly rewrites every published plugin.
    /// </summary>
    [Fact]
    public void A_plugin_without_the_new_components_generates_only_what_it_did_before()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().ValidateAndGenerate();

        var paths = run.Generated
            .Select(f => f.RelativePath.Replace('\\', '/'))
            .Where(p => p != $"{Plugin}/.cursor-plugin/plugin.json" && p != $"{Plugin}/.codex-plugin/plugin.json")
            .ToList();

        Assert.Equal([Marketplace], paths);
    }
}
