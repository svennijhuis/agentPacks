namespace AgentPacks.Cli.Tests;

/// <summary>
/// Contracts for the hidden verify-harness skill, scenarios/squad yes-no gate, and verifier Drive rules.
/// </summary>
public sealed class VerifyHarnessContractTests
{
    [Fact]
    public void Verify_harness_is_hidden_loop_skill_and_links_local_secrets_rule()
    {
        var root = TestRepository.SourceRoot();
        var path = Path.Combine(root, "plugins", "squad", "skills", "verify-harness", "SKILL.md");
        Assert.True(File.Exists(path));
        var skill = TestRepository.ParseFrontmatter(File.ReadAllText(path));
        Assert.Equal("verify-harness", skill.Scalar("name"));
        Assert.Equal("true", skill.Scalar("disable-model-invocation"));
        Assert.Equal("false", skill.Scalar("user-invocable"));
        var metadata = skill.StringMap("metadata");
        Assert.NotNull(metadata);
        Assert.Equal("loop", metadata!["audience"]);
        Assert.Contains("../squad/SKILL.md", skill.Body, StringComparison.Ordinal);
        Assert.Contains("needs: kv|secret|auth|local-resource", skill.Body, StringComparison.Ordinal);
        Assert.Contains("$(git rev-parse --git-dir)/squad/verify/", skill.Body, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2014', skill.Body);
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "verify.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "verify-harness.md")));
    }

    [Fact]
    public void Scenarios_md_asks_yes_no_and_requires_same_name_drive_entry_per_scenario()
    {
        var skill = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "skills", "scenarios-md", "SKILL.md"));
        Assert.Contains("FIRST ask yes/no: generate runnable verify harness?", skill, StringComparison.Ordinal);
        Assert.Contains("No → write only the smoke md; write no `docs/verify.md`", skill, StringComparison.Ordinal);
        Assert.Contains("load `verify-harness` by exact Skill name", skill, StringComparison.Ordinal);
        Assert.Contains("`## Drive` gets one entry per scenario matched by Case name", skill, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2014', skill);
    }

    [Fact]
    public void Squad_skill_asks_once_for_verify_harness_and_no_writes_no_file()
    {
        var skill = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "skills", "squad", "SKILL.md"));
        Assert.Contains("Make a runnable verify harness too? (yes/no)", skill, StringComparison.Ordinal);
        Assert.Contains("Yes → load `verify-harness` by exact Skill name", skill, StringComparison.Ordinal);
        Assert.Contains("No → carry on; write no `docs/verify.md`", skill, StringComparison.Ordinal);
        Assert.Contains("needs: kv|secret|auth|local-resource", File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "skills", "verify-harness", "SKILL.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Verifier_hard_doctor_fail_is_blocked_and_needs_kv_skips_rest_runs()
    {
        var verifier = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-verifier.md"));
        Assert.Contains("Hard Doctor fail → `blocked`, never `pass`", verifier, StringComparison.Ordinal);
        Assert.Contains("needs: kv|secret|auth|local-resource", verifier, StringComparison.Ordinal);
        Assert.Contains("needs: kv 403", verifier, StringComparison.Ordinal);
        Assert.Contains("`skipped` with reason; rest runs", verifier, StringComparison.Ordinal);
        Assert.Contains("dependent Drive → `skipped`", verifier, StringComparison.Ordinal);
        Assert.Contains("Cleanup (always)", verifier, StringComparison.Ordinal);
        Assert.Contains("Missing file → today's behavior", verifier, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2014', verifier);
    }

    [Fact]
    public void Verify_harness_drive_may_use_existing_e2e_or_playwright_never_install()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("tester-army `e2e`) or Playwright", body, StringComparison.Ordinal);
        Assert.Contains("Drive may use it for UI steps", body, StringComparison.Ordinal);
        Assert.Contains("never install or vendor tester-army/e2e or any browser tooling", body, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2014', body);
    }

    [Fact]
    public void Verify_harness_extends_existing_verify_md_never_overwrites()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("If one already exists, extend it", body, StringComparison.Ordinal);
        Assert.Contains("add missing parts only", body, StringComparison.Ordinal);
        Assert.Contains("change a line only where the app now contradicts it", body, StringComparison.Ordinal);
        Assert.Contains("Never overwrite (hand edits survive a rerun)", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_harness_evidence_bar_real_path_action_side_effects_mock_boundary()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("Drive the real user path, not test-only endpoints", body, StringComparison.Ordinal);
        Assert.Contains("Capture the action and its result", body, StringComparison.Ordinal);
        Assert.Contains("Confirm side effects in files, rows, or logs", body, StringComparison.Ordinal);
        Assert.Contains("Mock only at an existing external boundary", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_harness_cleanup_is_pid_only_and_keeps_evidence()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("Stop only processes/resources this run started (track PIDs)", body, StringComparison.Ordinal);
        Assert.Contains("never kill by name", body, StringComparison.Ordinal);
        Assert.Contains("Never delete evidence (earlier run evidence stays)", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Command_file_count_unchanged()
    {
        new HttpScenariosContractTests().User_commands_stay_squad_squad_review_pack_check_plus_http_scenarios();
        var directory = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands");
        var names = Directory.GetFiles(directory, "*.md")
            .Select(path => Path.GetFileName(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["scenarios.md", "squad-review.md", "squad.md"], names);
    }

    private static string VerifyHarnessBody() => File.ReadAllText(Path.Combine(
        TestRepository.SourceRoot(), "plugins", "squad", "skills", "verify-harness", "SKILL.md"));
}
