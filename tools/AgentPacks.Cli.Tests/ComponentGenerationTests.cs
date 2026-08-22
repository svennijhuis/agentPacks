namespace AgentPacks.Cli.Tests;

/// <summary>
/// Agents, commands and rules translated into each client's format. The formats look similar
/// enough that a wrong extension or a leaked frontmatter key produces a file the client loads and
/// then ignores, so the differences are asserted explicitly.
/// </summary>
public sealed class ComponentGenerationTests
{
    private const string Plugin = "plugins/engineering";

    [Fact]
    public void An_agent_is_generated_for_every_client_in_its_own_format()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithAgent("security-reviewer").ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.True(run.HasFile($"{Plugin}/com.anthropic.claude-code/agents/security-reviewer.md"));
        Assert.True(run.HasFile($"{Plugin}/com.github.copilot/agents/security-reviewer.agent.md"));
        Assert.True(run.HasFile($"{Plugin}/com.openai.codex/agents/security-reviewer.toml"));
    }

    /// <summary>
    /// Cursor reads agents/*.md from the plugin root, which is where the neutral source is already
    /// authored. Generating a copy would put two definitions of one agent in the package.
    /// </summary>
    [Fact]
    public void Cursor_reads_the_authored_agent_directly()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithAgent("security-reviewer").ValidateAndGenerate();

        Assert.False(run.HasFile($"{Plugin}/agents/security-reviewer.md"));
    }

    /// <summary>Claude names its tools in PascalCase; the neutral format uses Cursor's lowercase.</summary>
    [Fact]
    public void Agent_tools_are_translated_into_claude_tool_names()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "tools:\n  - read\n  - grep")
            .ValidateAndGenerate();

        var claude = run.File($"{Plugin}/com.anthropic.claude-code/agents/reviewer.md").Text;

        Assert.Contains("tools: [\"Read\", \"Grep\"]", claude, StringComparison.Ordinal);
    }

    /// <summary>
    /// 'readonly' is Cursor's key. Copying it into a Claude or Copilot agent would put a key in the
    /// frontmatter that neither reads, which looks like a working restriction and is not one.
    /// </summary>
    [Fact]
    public void Frontmatter_keys_a_client_does_not_read_are_dropped()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "readonly: true")
            .ValidateAndGenerate();

        Assert.DoesNotContain(
            "readonly",
            run.File($"{Plugin}/com.anthropic.claude-code/agents/reviewer.md").Text,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "readonly",
            run.File($"{Plugin}/com.github.copilot/agents/reviewer.agent.md").Text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void A_codex_agent_carries_the_body_as_developer_instructions()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", body: "You review changes for defects.")
            .ValidateAndGenerate();

        var toml = run.File($"{Plugin}/com.openai.codex/agents/reviewer.toml").Text;

        Assert.Contains("name = \"reviewer\"", toml, StringComparison.Ordinal);
        Assert.Contains("developer_instructions = \"\"\"", toml, StringComparison.Ordinal);
        Assert.Contains("You review changes for defects.", toml, StringComparison.Ordinal);
    }

    [Fact]
    public void An_agent_whose_name_does_not_match_its_filename_fails()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithAgent("reviewer", name: "something-else").Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("does not match the filename", run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void An_agent_without_a_description_fails()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithAgent("reviewer", description: "").Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void A_command_is_generated_for_claude_and_copilot()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithCommand("review-diff").ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.True(run.HasFile($"{Plugin}/com.anthropic.claude-code/commands/review-diff.md"));
        Assert.True(run.HasFile($"{Plugin}/com.github.copilot/commands/review-diff.md"));
    }

    // ------------------------------------------------------------------ rules

    /// <summary>
    /// Copilot expresses scope as applyTo. An always-on rule has no globs, so it becomes the
    /// everything glob rather than being emitted without a scope, which Copilot would not apply.
    /// </summary>
    [Fact]
    public void An_always_on_rule_becomes_copilot_instructions_scoped_to_everything()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards").ValidateAndGenerate();

        var instructions = run.File($"{Plugin}/com.github.copilot/instructions/standards.instructions.md").Text;

        Assert.Contains("applyTo: \"**\"", instructions, StringComparison.Ordinal);
    }

    [Fact]
    public void A_scoped_rule_carries_its_globs_into_copilot_instructions()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithRule("checklist", scope: "globs:\n  - \"**/*.cs\"\n  - \"**/*.ts\"")
            .ValidateAndGenerate();

        var instructions = run.File($"{Plugin}/com.github.copilot/instructions/checklist.instructions.md").Text;

        Assert.Contains("applyTo: \"**/*.cs,**/*.ts\"", instructions, StringComparison.Ordinal);
    }

    /// <summary>
    /// Claude has no rules component, so always-on rules are baked into a SessionStart script.
    /// Baked, not read at runtime: the hook must not depend on files being where it expects.
    /// </summary>
    [Fact]
    public void An_always_on_rule_is_baked_into_the_claude_session_script()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithRule("standards", body: "- Changes do one thing.")
            .ValidateAndGenerate();

        var script = run.File($"{Plugin}/com.anthropic.claude-code/scripts/rules-context.sh").Text;

        Assert.Contains("- Changes do one thing.", script, StringComparison.Ordinal);
        Assert.True(run.HasFile($"{Plugin}/com.anthropic.claude-code/scripts/rules-context.ps1"));
        Assert.True(run.HasFile($"{Plugin}/com.anthropic.claude-code/scripts/rules-context.cmd"));
    }

    [Fact]
    public void The_claude_rules_script_is_wired_to_session_start()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards").ValidateAndGenerate();

        var hooks = run.File($"{Plugin}/com.anthropic.claude-code/hooks/hooks.json").Text;

        Assert.Contains("SessionStart", hooks, StringComparison.Ordinal);
        Assert.Contains("scripts/rules-context", hooks, StringComparison.Ordinal);
    }

    /// <summary>
    /// A scoped rule cannot be expressed on Claude at all. Dropping it silently would leave a rule
    /// the author believes is active on four clients when it is active on two.
    /// </summary>
    [Fact]
    public void A_scoped_rule_warns_that_claude_does_not_receive_it()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithRule("standards")
            .WithRule("checklist", scope: "globs:\n  - \"**/*.cs\"")
            .ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.Contains("Claude has no path-scoped rule concept", run.Text, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "checklist",
            run.File($"{Plugin}/com.anthropic.claude-code/scripts/rules-context.sh").Text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void An_always_on_rule_reaches_codex_as_an_agents_file()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards", body: "- Changes do one thing.").ValidateAndGenerate();

        var agents = run.File($"{Plugin}/com.openai.codex/AGENTS.md").Text;

        Assert.Contains("- Changes do one thing.", agents, StringComparison.Ordinal);
    }

    [Fact]
    public void A_rule_that_is_neither_always_on_nor_scoped_fails()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards", scope: "").Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("would ever apply it", run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_rule_that_is_both_always_on_and_scoped_fails()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithRule("standards", scope: "alwaysApply: true\nglobs:\n  - \"**/*.cs\"")
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("Pick one", run.Text, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ shims

    [Fact]
    public void A_windows_shim_is_generated_for_every_complete_script_pair()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart").ValidateAndGenerate();

        var shim = run.File($"{Plugin}/scripts/guard.cmd").Text;

        Assert.Contains("%~dp0guard.ps1", shim, StringComparison.Ordinal);
        Assert.Contains("powershell", shim, StringComparison.Ordinal);
    }

    /// <summary>The POSIX script must stay executable: the hook command runs it directly.</summary>
    [Fact]
    public void The_generated_rules_script_is_marked_executable()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards").ValidateAndGenerate();

        Assert.True(run.File($"{Plugin}/com.anthropic.claude-code/scripts/rules-context.sh").Executable);
        Assert.False(run.File($"{Plugin}/com.anthropic.claude-code/scripts/rules-context.cmd").Executable);
    }
}
