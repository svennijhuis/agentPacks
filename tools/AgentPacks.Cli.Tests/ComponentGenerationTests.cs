using System.Text.Json.Nodes;

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
    /// The two web tools are separate capabilities in Claude and a single unmapped name is not an
    /// error anywhere: validation passes, generation passes, and Claude drops the tool it does not
    /// recognise. An agent that was given web access silently loses it, which is why the mapping is
    /// asserted rather than assumed.
    /// </summary>
    [Fact]
    public void Web_tools_are_translated_into_claude_tool_names()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("researcher", extraFrontmatter: "tools:\n  - websearch\n  - webfetch")
            .ValidateAndGenerate();

        var claude = run.File($"{Plugin}/com.anthropic.claude-code/agents/researcher.md").Text;

        Assert.Contains("tools: [\"WebSearch\", \"WebFetch\"]", claude, StringComparison.Ordinal);
    }

    /// <summary>
    /// The mistake no client reports: Claude drops a tool name it does not recognise and the other
    /// three pass the list through, so a typo generates cleanly and costs the agent a capability
    /// with nothing printed. The neutral vocabulary is closed here so the typo fails the build.
    /// </summary>
    [Theory]
    [InlineData("Read")]
    [InlineData("web")]
    [InlineData("web_search")]
    [InlineData("websearchh")]
    public void An_unknown_tool_name_is_rejected(string tool)
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: $"tools:\n  - {tool}")
            .ValidateAndGenerate();

        Assert.True(run.HasErrors, run.Text);
        Assert.Contains($"unknown tool '{tool}'", run.Text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The read-only check matches declared names against the writing tools, so a misspelt writing
    /// tool would pass it while the generated agent still gets whatever the client makes of the
    /// name. Closing the vocabulary closes that path too.
    /// </summary>
    [Fact]
    public void A_read_only_agent_cannot_slip_a_misspelt_writing_tool_past_the_check()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "readonly: true\ntools:\n  - read\n  - Write")
            .ValidateAndGenerate();

        Assert.True(run.HasErrors, run.Text);
        Assert.Contains("unknown tool 'Write'", run.Text, StringComparison.Ordinal);
    }

    /// <summary>
    /// 'readonly' is Cursor's key. Copying it into a Claude or Copilot agent would put a key in the
    /// frontmatter that neither reads, which looks like a working restriction and is not one. What
    /// carries the restriction into every client is the tool list, which ComponentValidator forces
    /// a read-only agent to declare.
    /// </summary>
    [Fact]
    public void Frontmatter_keys_a_client_does_not_read_are_dropped()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "readonly: true\ntools:\n  - read")
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
    /// Copilot's plugin schema declares agents, skills, commands, hooks, mcpServers and lspServers
    /// — and nothing for instructions. An instructions file inside a plugin is therefore a file no
    /// manifest can point at and nothing ever loads, so rules take the same route they take for
    /// Claude: a SessionStart hook that prints them.
    /// </summary>
    [Fact]
    public void An_always_on_rule_reaches_copilot_as_a_session_start_hook()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards", body: "- Changes do one thing.")
            .ValidateAndGenerate();

        var script = run.File($"{Plugin}/com.github.copilot/scripts/rules-context.sh").Text;
        var entry = (JsonObject)run.File($"{Plugin}/com.github.copilot/hooks/hooks.json")
            .Content["hooks"]!["SessionStart"]![0]!;

        Assert.Contains("- Changes do one thing.", script, StringComparison.Ordinal);
        Assert.Contains(
            "${PLUGIN_ROOT}/com.github.copilot/scripts/rules-context",
            entry["bash"]!.GetValue<string>(),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The unroutable path must stay gone: regenerating it would ship a file into the marketplace
    /// that no Copilot manifest key can declare.
    /// </summary>
    [Fact]
    public void No_rule_is_written_as_a_copilot_instructions_file()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithRule("standards")
            .WithRule("checklist", scope: "globs:\n  - \"**/*.cs\"\n  - \"**/*.ts\"")
            .ValidateAndGenerate();

        Assert.False(run.HasFile($"{Plugin}/com.github.copilot/instructions/standards.instructions.md"));
        Assert.False(run.HasFile($"{Plugin}/com.github.copilot/instructions/checklist.instructions.md"));
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
        Assert.Contains("Only Cursor has a path-scoped rule concept", run.Text, StringComparison.Ordinal);
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

    /// <summary>
    /// The rules hook command is extensionless like every other one, so the same dispatcher has to
    /// exist here. Without it the always-on rules reach no Claude session on macOS or Linux, and
    /// nothing says so: the hook simply fails to launch.
    /// </summary>
    [Fact]
    public void The_rules_hook_command_resolves_to_a_generated_dispatcher()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithRule("standards").ValidateAndGenerate();

        var command = ((JsonArray)((JsonObject)run
                .File($"{Plugin}/com.anthropic.claude-code/hooks/hooks.json").Content["hooks"]!)["SessionStart"]![0]!["hooks"]!)[0]!["command"]!
            .GetValue<string>()
            .Trim('"');

        var pluginRelative = command.Replace("${CLAUDE_PLUGIN_ROOT}/", string.Empty, StringComparison.Ordinal);

        Assert.True(run.HasFile($"{Plugin}/{pluginRelative}"));
        Assert.True(run.File($"{Plugin}/{pluginRelative}").Executable);
    }

    /// <summary>
    /// Claude groups SessionStart hooks by matcher, and two groups carrying the same matcher mean
    /// one of them silently wins. The generated rules hook has no matcher, so it belongs inside the
    /// authored matcher-less group rather than in a second one beside it.
    /// </summary>
    [Fact]
    public void The_rules_hook_joins_the_authored_session_start_group()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithRule("standards")
            .WithHook("sessionStart")
            .ValidateAndGenerate();

        var sessionStart = ((JsonObject)run
            .File($"{Plugin}/com.anthropic.claude-code/hooks/hooks.json").Content["hooks"]!)["SessionStart"]!.AsArray();

        Assert.Single(sessionStart);

        var commands = ((JsonArray)sessionStart[0]!["hooks"]!)
            .Select(entry => entry!["command"]!.GetValue<string>())
            .ToList();

        Assert.Equal(2, commands.Count);
        Assert.Contains(commands, c => c.Contains("scripts/guard", StringComparison.Ordinal));
        Assert.Contains(commands, c => c.Contains("scripts/rules-context", StringComparison.Ordinal));
    }

    /// <summary>
    /// The warning is the only signal a scoped rule gives, and it is most needed when there is no
    /// always-on rule to generate: three of the four clients get nothing at all, and the generator
    /// would otherwise return before saying so.
    /// </summary>
    [Fact]
    public void A_plugin_with_only_scoped_rules_still_warns()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithRule("checklist", scope: "globs:\n  - \"**/*.cs\"")
            .ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.Contains("Only Cursor has a path-scoped rule concept", run.Text, StringComparison.Ordinal);

        // Cursor reads the authored rules/ directly, so nothing is generated for it either. The
        // other three get no rules file at all, which is exactly what the warning is for.
        Assert.False(run.HasFile($"{Plugin}/com.anthropic.claude-code/scripts/rules-context.sh"));
        Assert.False(run.HasFile($"{Plugin}/com.github.copilot/scripts/rules-context.sh"));
        Assert.False(run.HasFile($"{Plugin}/com.openai.codex/AGENTS.md"));
    }

    /// <summary>
    /// No client has a read-only flag, so an agent that declares one and no tools inherits every
    /// tool the client has — the flag reads as a restriction and is not one.
    /// </summary>
    [Fact]
    public void A_read_only_agent_without_tools_fails()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "readonly: true")
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("'readonly: true' but no 'tools'", run.Text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The tool list is the only thing that carries the restriction into the generated trees, so a
    /// read-only agent asking for a writing tool gets exactly that tool and the flag means nothing.
    /// </summary>
    [Fact]
    public void A_read_only_agent_requesting_a_writing_tool_fails()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "readonly: true\ntools:\n  - read\n  - write")
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("but requests write", run.Text, StringComparison.Ordinal);
    }

    /// <summary>
    /// A TOML basic string only accepts a fixed escape set, so an unescaped backslash in an agent
    /// body is a parse error Codex reports at load — and \t or \n would parse and silently rewrite
    /// the instruction instead.
    /// </summary>
    [Fact]
    public void A_codex_agent_body_escapes_backslashes()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithAgent("reviewer", body: @"Match [0-9]\d+ and read C:\src\app.")
            .ValidateAndGenerate();

        var toml = run.File($"{Plugin}/com.openai.codex/agents/reviewer.toml").Text;

        Assert.Contains(@"[0-9]\\d+", toml, StringComparison.Ordinal);
        Assert.Contains(@"C:\\src\\app", toml, StringComparison.Ordinal);
    }
}
