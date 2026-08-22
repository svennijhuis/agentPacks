namespace AgentPacks.Cli.Tests;

/// <summary>
/// Hooks run code on a developer's machine at moments they did not trigger, so the neutral manifest
/// is the strictest thing this tool validates.
/// </summary>
public sealed class HookValidationTests
{
    [Fact]
    public void A_complete_hook_passes()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("beforeShellExecution", "rm +-rf").Validate();

        Assert.False(run.HasErrors, run.Text);
    }

    /// <summary>
    /// One hooks.json is shared by macOS and Windows, so a script with only one platform half is a
    /// hook that silently does nothing for half the team.
    /// </summary>
    [Theory]
    [InlineData(false, true, ".sh")]
    [InlineData(true, false, ".ps1")]
    public void A_script_missing_a_platform_half_fails(bool posix, bool powerShell, string missing)
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard", posix, powerShell)
            .WithHooks("""{ "hooks": { "sessionStart": [{ "script": "guard" }] } }""")
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains(missing, run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_hook_naming_a_script_that_does_not_exist_fails()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithHooks("""{ "hooks": { "sessionStart": [{ "script": "missing" }] } }""")
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("has no matching file in scripts/", run.Text, StringComparison.Ordinal);
    }

    /// <summary>A hook names a basename, never a command line: the generator owns the invocation.</summary>
    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("/usr/bin/env")]
    [InlineData("guard.sh")]
    [InlineData("guard; rm -rf /")]
    public void A_script_that_is_not_a_bare_basename_fails(string script)
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithHooks($$"""{ "hooks": { "sessionStart": [{ "script": "{{script}}" }] } }""")
            .Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void An_event_outside_the_neutral_vocabulary_fails()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithHooks("""{ "hooks": { "PostToolUse": [{ "script": "guard" }] } }""")
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("is not a neutral hook event", run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_matcher_that_is_not_a_regex_fails()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("preToolUse", "Read(").Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("not a valid regular expression", run.Text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The matcher is emitted verbatim inside a double-quoted shell argument. sh and PowerShell
    /// disagree on how these escape, so they are rejected rather than quoted two different ways.
    /// </summary>
    [Theory]
    [InlineData(@"rm \\d+")]
    [InlineData("rm $HOME")]
    [InlineData(@"say \""hi\""")]
    [InlineData("run `whoami`")]
    public void A_matcher_with_shell_hostile_characters_fails(string jsonEscapedMatcher)
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithHooks($$"""
                { "hooks": { "beforeShellExecution": [{ "script": "guard", "matcher": "{{jsonEscapedMatcher}}" }] } }
                """)
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("cannot be quoted the same way", run.Text, StringComparison.Ordinal);
    }

    /// <summary>An event with nothing to match against silently drops the matcher on every client.</summary>
    [Fact]
    public void A_matcher_on_an_event_that_takes_none_fails()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithHook("sessionStart", "anything").Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("takes no matcher", run.Text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("6000")]
    public void An_out_of_range_timeout_fails(string timeout)
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithHooks($$"""{ "hooks": { "sessionStart": [{ "script": "guard", "timeout": {{timeout}} }] } }""")
            .Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void An_unknown_hook_key_fails()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithHooks("""
                { "hooks": { "sessionStart": [{ "script": "guard", "command": "rm -rf /" }] } }
                """)
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("is not part of the neutral format", run.Text, StringComparison.Ordinal);
    }

    /// <summary>An orphaned half-pair is either a leftover or a hook someone forgot to wire up.</summary>
    [Fact]
    public void An_unreferenced_script_missing_a_half_is_reported()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().WithScript("audit", powerShell: false).Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("audit.ps1", run.Text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The .cmd shim is generated output that lives beside the authored pair on the published
    /// branch, where validation also runs. Treating it as a stray script would make the published
    /// branch fail its own drift check.
    /// </summary>
    [Fact]
    public void A_generated_cmd_shim_is_not_mistaken_for_a_stray_script()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithFile("plugins/engineering/scripts/guard.cmd", "@echo off\n")
            .Validate();

        Assert.False(run.HasErrors, run.Text);
    }

    /// <summary>
    /// An event with no entries generates no hooks file, while the plugin still reads as declaring
    /// hooks. Rejecting it keeps the manifests from pointing at a file that was never written.
    /// </summary>
    [Fact]
    public void An_event_declared_with_no_hooks_fails()
    {
        using var repo = new TestRepository();

        var run = repo.WithValidPlugin()
            .WithScript("guard")
            .WithHooks("""
                { "hooks": { "stop": [] } }
                """)
            .Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("declares no hooks", run.Text, StringComparison.Ordinal);
    }

    /// <summary>
    /// A manifest that names a hooks file the generator did not write fails the plugin at install
    /// time, so the two decisions are made from the same fact rather than from two similar ones.
    /// </summary>
    [Fact]
    public void No_manifest_points_at_a_hooks_file_that_was_not_generated()
    {
        using var repo = new TestRepository();
        var run = repo.WithValidPlugin().ValidateAndGenerate();

        var marketplace = run.File(".claude-plugin/marketplace.json").Text;
        var codex = run.File("plugins/engineering/.codex-plugin/plugin.json").Text;

        Assert.False(run.HasFile("plugins/engineering/com.anthropic.claude-code/hooks/hooks.json"));
        Assert.False(run.HasFile("plugins/engineering/com.openai.codex/hooks/hooks.json"));
        Assert.DoesNotContain("hooks/hooks.json", marketplace, StringComparison.Ordinal);
        Assert.DoesNotContain("hooks/hooks.json", codex, StringComparison.Ordinal);
    }
}
