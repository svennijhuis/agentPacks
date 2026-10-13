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
        Assert.Contains("needs: kv|secret|auth|local-resource", VerifyHarnessBody(), StringComparison.Ordinal);
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
        Assert.Contains("When `docs/verify.md` is missing, FIRST ask yes/no: generate runnable verify harness?", skill, StringComparison.Ordinal);
        Assert.Contains("No → write only the smoke md; write no `docs/verify.md` and no `docs/smoke-report.md`", skill, StringComparison.Ordinal);
        Assert.Contains("load `verify-harness` by exact Skill name", skill, StringComparison.Ordinal);
        Assert.Contains("`## Drive` gets one entry per scenario matched by Case name", skill, StringComparison.Ordinal);
        Assert.Contains("rewrite tracked `docs/smoke-report.md`", skill, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2014', skill);
    }

    [Fact]
    public void Verify_harness_rewrites_smoke_report_with_sections_and_local_evidence()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("docs/smoke-report.md", body, StringComparison.Ordinal);
        Assert.Contains("fully rewritten each run", body, StringComparison.Ordinal);
        Assert.Contains("(never appended)", body, StringComparison.Ordinal);
        Assert.Contains("Manual steps", body, StringComparison.Ordinal);
        Assert.Contains("Expected", body, StringComparison.Ordinal);
        Assert.Contains("Outcome", body, StringComparison.Ordinal);
        Assert.Contains("`pass`", body, StringComparison.Ordinal);
        Assert.Contains("`fail`", body, StringComparison.Ordinal);
        Assert.Contains("`skipped`", body, StringComparison.Ordinal);
        Assert.Contains("`blocked`", body, StringComparison.Ordinal);
        Assert.Contains("local to the run machine", body, StringComparison.Ordinal);
        Assert.Contains("$(git rev-parse --git-dir)/squad/verify/", body, StringComparison.Ordinal);
        Assert.Contains("never tracked", body, StringComparison.Ordinal);
        Assert.Contains("No clickable link", body, StringComparison.Ordinal);
        Assert.Contains("plain text", body, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2014', body);
    }

    [Fact]
    public void Squad_skill_asks_once_for_verify_harness_and_no_writes_no_file()
    {
        var skill = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "skills", "squad", "SKILL.md"));
        Assert.Contains("Make a runnable verify harness too? (yes/no)", skill, StringComparison.Ordinal);
        Assert.Contains("Yes → load `verify-harness` by exact Skill name", skill, StringComparison.Ordinal);
        Assert.Contains("No → carry on; write no `docs/verify.md`", skill, StringComparison.Ordinal);
        Assert.Contains("needs: kv|secret|auth|local-resource", VerifyHarnessBody(), StringComparison.Ordinal);
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
        Assert.Contains("e2e tool or Playwright only if the repo already has one", body, StringComparison.Ordinal);
        Assert.Contains("never install or vendor browser tooling", body, StringComparison.Ordinal);
        Assert.Contains("v1 adds no tools", body, StringComparison.Ordinal);
        Assert.DoesNotContain('\u2014', body);
    }

    [Fact]
    public void Verify_harness_extends_existing_verify_md_never_overwrites()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("Extend an existing verify.md, never overwrite it", body, StringComparison.Ordinal);
        Assert.Contains("hand edits must survive a rerun", body, StringComparison.Ordinal);
        Assert.Contains("change a line only where the app now contradicts it", body, StringComparison.Ordinal);
        Assert.Contains("Generate or extend `docs/verify.md`", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_harness_evidence_bar_real_path_action_side_effects_mock_boundary()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("Drive the real user path, not test-only endpoints", body, StringComparison.Ordinal);
        Assert.Contains("Capture action and result", body, StringComparison.Ordinal);
        Assert.Contains("check side effects in files, rows or logs", body, StringComparison.Ordinal);
        Assert.Contains("Mock only at an existing external boundary", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_harness_cleanup_is_pid_only_and_keeps_evidence()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("Stop only what this run started (track PIDs)", body, StringComparison.Ordinal);
        Assert.Contains("never kill by name", body, StringComparison.Ordinal);
        Assert.Contains("confirm evidence still exists after it", body, StringComparison.Ordinal);
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

    [Fact]
    public void Verify_harness_skill_is_at_most_50_lines_and_proves_one_feature()
    {
        var path = Path.Combine(VerifyHarnessDir(), "SKILL.md");
        Assert.True(File.ReadAllLines(path).Length <= 50);
        var skill = File.ReadAllText(path);
        Assert.Contains("Write for the next agent: it arrives cold, mid-task", skill, StringComparison.Ordinal);
        Assert.Contains("Run Cleanup on failure too", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("|---", skill, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_evidence_is_never_tracked()
    {
        var tracked = TestRepository.SourceRoot();
        Assert.Contains("never tracked", VerifyHarnessBody(), StringComparison.Ordinal);
        Assert.DoesNotContain(Directory.EnumerateFiles(tracked, "*", SearchOption.AllDirectories)
            .Select(f => f.Replace('\\', '/'))
            .Where(f => !f.Contains("/.git/", StringComparison.Ordinal)),
            f => f.Contains("/squad/verify/", StringComparison.Ordinal));
    }

    [Fact]
    public void Commands_mirror_verify_harness_ask_and_verifier_skips_needs_tags()
    {
        var root = TestRepository.SourceRoot();
        var commands = Path.Combine(root, "plugins", "squad", "commands");
        Assert.Contains("Make a runnable verify harness too? (yes/no)", File.ReadAllText(Path.Combine(commands, "squad.md")), StringComparison.Ordinal);
        Assert.Contains("also writes `docs/verify.md` and `docs/smoke-report.md`", File.ReadAllText(Path.Combine(commands, "scenarios.md")), StringComparison.Ordinal);
        Assert.Contains("tagged `needs:` → `skipped`", File.ReadAllText(Path.Combine(root, "plugins", "squad", "agents", "squad-verifier.md")), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("skills/scenarios-md/SKILL.md")]
    [InlineData("commands/scenarios.md")]
    [InlineData("skills/squad/SKILL.md")]
    [InlineData("commands/squad.md")]
    public void Verify_harness_ask_fires_only_when_verify_md_is_missing(string relative)
    {
        var text = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", relative));
        Assert.Contains("`docs/verify.md` is missing", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_harness_drive_defaults_to_api_curl()
    {
        var skill = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "SKILL.md"));
        Assert.Contains("Drive defaults to API: HTTP calls with curl", skill, StringComparison.Ordinal);
        Assert.Contains("status code, key payload fields and one error case", skill, StringComparison.Ordinal);
        Assert.Contains("port or health endpoint first", skill, StringComparison.Ordinal);
        Assert.Contains("curl -s -w", File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "smoke-report.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Creation_proof_drives_one_feature()
        => Assert.Contains("When creating `docs/verify.md`, run Launch, Doctor, Drive for ONE feature", File.ReadAllText(Path.Combine(VerifyHarnessDir(), "SKILL.md")), StringComparison.Ordinal);

    [Fact]
    public void Later_runs_drive_every_touched_scenario()
    {
        Assert.Contains("Every later run (verifier, or `/scenarios` with an existing verify.md) drives every touched scenario", VerifyHarnessBody(), StringComparison.Ordinal);
        Assert.Contains("Drive for every touched scenario (not only the one-feature creation proof)", File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-verifier.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Skill_and_verifier_run_check_verify_on_both_files()
    {
        const string cmd = "check-verify.sh docs/verify.md docs/smoke-report.md";
        Assert.Contains(cmd, File.ReadAllText(Path.Combine(VerifyHarnessDir(), "SKILL.md")), StringComparison.Ordinal);
        Assert.Contains(cmd, File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-verifier.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Smoke_report_outcome_line_carries_skip_reason()
        => Assert.Contains("`skipped (needs: kv, 403)`", File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "smoke-report.md")), StringComparison.Ordinal);

    [Fact]
    public void Generated_docs_ban_em_dashes()
        => Assert.Contains("No em dashes in `docs/verify.md` or `docs/smoke-report.md`", VerifyHarnessBody(), StringComparison.Ordinal);

    [Fact]
    public void Skipped_is_only_for_failed_needs_and_not_run_is_separate()
    {
        Assert.Contains("drives every touched scenario; `skipped` only for a failed `needs:` check, `not run` for anything else not driven", VerifyHarnessBody(), StringComparison.Ordinal);
        Assert.Contains("not driven → `not run`", File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-verifier.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Needs_check_fails_on_403_any_non_2xx_or_error()
        => Assert.Contains("A 403, any other non-2xx status, or any error means the check failed", VerifyHarnessBody(), StringComparison.Ordinal);

    [Fact]
    public void Evidence_is_per_run_dir()
        => Assert.Contains("squad/verify/<slug>/<run-id>/", VerifyHarnessBody(), StringComparison.Ordinal);

    [Fact]
    public void Cleanup_kills_process_group_and_checks_ports_free()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("`exec` or `setsid`", body, StringComparison.Ordinal);
        Assert.Contains("kills that process group", body, StringComparison.Ordinal);
        Assert.Contains("check every Launch port is free", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Scenarios_command_names_full_skill_path_never_find()
    {
        var command = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands", "scenarios.md"));
        Assert.Contains("`<Squad plugin root>/skills/scenarios-md/SKILL.md`", command, StringComparison.Ordinal);
        Assert.Contains("never run `find` for it", command, StringComparison.Ordinal);
    }

    private static string VerifyHarnessDir() => Path.Combine(
        TestRepository.SourceRoot(), "plugins", "squad", "skills", "verify-harness");

    private static string VerifyHarnessBody() => string.Join("\n",
        new[] { "SKILL.md", "references/smoke-report.md", "references/doctor-tags.md", "references/process.md" }
            .Select(f => File.ReadAllText(Path.Combine(VerifyHarnessDir(), f))));
}
