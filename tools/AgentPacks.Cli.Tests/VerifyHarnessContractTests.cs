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
        Assert.Contains("references/doctor-tags.md", verifier, StringComparison.Ordinal);
        var tags = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "doctor-tags.md"));
        Assert.Contains("needs: kv|secret|auth|local-resource", tags, StringComparison.Ordinal);
        Assert.Contains("403", tags, StringComparison.Ordinal);
        Assert.Contains("the rest keeps running", tags, StringComparison.Ordinal);
        Assert.Contains("depends on a failed tagged check: `skipped (needs: <tag>, <status>)`", tags, StringComparison.Ordinal);
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
        Assert.Contains("Drive for every touched scenario, then Cleanup", File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-verifier.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Skill_and_verifier_run_check_verify_on_both_files()
    {
        const string both = "check-verify.sh docs/verify.md docs/smoke-report.md";
        Assert.Contains(both, File.ReadAllText(Path.Combine(VerifyHarnessDir(), "SKILL.md")), StringComparison.Ordinal);
        var verifier = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-verifier.md"));
        Assert.Contains("check-verify.sh docs/verify.md", verifier, StringComparison.Ordinal);
        Assert.DoesNotContain(both, verifier, StringComparison.Ordinal);
    }

    [Fact]
    public void Verifier_lints_smoke_report_only_when_it_exists_and_does_not_fix()
    {
        var verifier = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-verifier.md"));
        Assert.Contains("adding `docs/smoke-report.md` only if it exists. A finding is `fail`.", verifier, StringComparison.Ordinal);
        Assert.Contains("Read-only: never write these files", verifier, StringComparison.Ordinal);
        Assert.DoesNotContain("a finding is a fix", verifier, StringComparison.Ordinal);
    }

    [Fact]
    public void Squad_skill_lists_verify_harness_in_load_only_allowed()
    {
        var skill = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "skills", "squad", "SKILL.md"));
        Assert.Contains(
            "Load only with the Skill tool by exact name: `squad`, `caveman`, `verify-harness`,",
            skill,
            StringComparison.Ordinal);
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
        Assert.Contains("a touched scenario not driven is `not run`, which blocks", File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "doctor-tags.md")), StringComparison.Ordinal);
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
    public void Local_access_absent_means_ask_once_after_harness_yes()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("ask once after harness yes", body, StringComparison.Ordinal);
        Assert.Contains("provider and API scope the repo does not say", body, StringComparison.Ordinal);
        Assert.Contains("Optional sixth: `## Local access`", body, StringComparison.Ordinal);
        Assert.Contains("section present or `provider: skip` means never re-ask", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Local_access_section_present_means_no_re_ask()
    {
        var local = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "local-access.md"));
        Assert.Contains("Section present → no re-ask", local, StringComparison.Ordinal);
        Assert.Contains("Ask once, only after harness yes", local, StringComparison.Ordinal);
    }

    [Fact]
    public void Local_access_skip_is_saved_as_provider_skip()
    {
        var local = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "local-access.md"));
        Assert.Contains("Answer `skip` → `provider: skip`", local, StringComparison.Ordinal);
        Assert.Contains("`provider: skip` alone is valid and stops further asks", local, StringComparison.Ordinal);
        Assert.Contains("## Local access", File.ReadAllText(Path.Combine(VerifyHarnessDir(), "SKILL.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void CredentialUnavailable_fixture_maps_to_skipped_not_blocked()
    {
        var local = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "local-access.md"));
        const string fixture =
            "DefaultAzureCredential failed to retrieve a token from the included credentials. CredentialUnavailableException (Status: 401)";
        const string cut =
            "DefaultAzureCredential failed to retrieve a token from the included credentials.";
        Assert.Contains("CredentialUnavailable", local, StringComparison.Ordinal);
        Assert.Contains(fixture, local, StringComparison.Ordinal);
        Assert.Contains($"skipped (needs: auth, {cut})", local, StringComparison.Ordinal);
        Assert.Contains("Non-access startup failure stays `blocked`", local, StringComparison.Ordinal);
        Assert.Contains("System.IO.FileNotFoundException", local, StringComparison.Ordinal);
        Assert.Contains("→ Outcome: `blocked`", local, StringComparison.Ordinal);
        Assert.Contains("non-access startup failure stays `blocked`", File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "doctor-tags.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Contract_Aws_fixture_log_skips_with_aws_sso_login_hint()
    {
        var local = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "local-access.md"));
        Assert.Contains("Unable to locate credentials", local, StringComparison.Ordinal);
        Assert.Contains("aws sso login", local, StringComparison.Ordinal);
        Assert.Contains("→ Hint: `aws sso login`", local, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_token_is_skipped()
    {
        var body = VerifyHarnessBody();
        Assert.Contains("Missing token → `skipped (needs: auth, missing token)`", body, StringComparison.Ordinal);
    }

    [Fact]
    public void Contract_Five_auth_scenarios_one_attempt_five_skipped_one_report_one_retro()
    {
        var report = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "smoke-report.md"));
        Assert.Contains("one try only across the whole run", report, StringComparison.Ordinal);
        Assert.Contains("mark every scenario that needs that access `skipped` at once", report, StringComparison.Ordinal);
        Assert.Contains("Write the skip reason once in this report", report, StringComparison.Ordinal);
        Assert.Contains("one retro line", report, StringComparison.Ordinal);
        Assert.Contains("Five scenarios needing the same auth skip with one attempt: five `skipped` Outcomes, one shared reason line in the report, one retro line", report, StringComparison.Ordinal);
        Assert.Contains("Verify never blocks `/squad`", report, StringComparison.Ordinal);
    }

    [Fact]
    public void Contract_Local_access_example_has_exactly_provider_and_scope()
    {
        var local = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "local-access.md"));
        var start = local.IndexOf("```markdown\n## Local access\n", StringComparison.Ordinal);
        Assert.True(start >= 0);
        var blockStart = start + "```markdown\n".Length;
        var end = local.IndexOf("\n```", blockStart, StringComparison.Ordinal);
        Assert.True(end > blockStart);
        var block = local[blockStart..end];
        Assert.Equal(
            "## Local access\nprovider: azure\nscope: https://vault.azure.net/.default",
            block);
        Assert.DoesNotContain("vault:", block, StringComparison.Ordinal);
        Assert.DoesNotContain("secret:", block, StringComparison.Ordinal);
    }

    [Fact]
    public void Contract_Example_outcome_lines_are_at_most_120_chars()
    {
        foreach (var relative in new[] { "references/local-access.md", "references/smoke-report.md" })
        {
            var path = Path.Combine(VerifyHarnessDir(), relative);
            var n = 0;
            foreach (var line in File.ReadLines(path))
            {
                n++;
                if (!line.Contains("Outcome:", StringComparison.Ordinal)
                    && !line.Contains("skipped (needs:", StringComparison.Ordinal))
                    continue;
                Assert.True(
                    line.Length <= 120,
                    $"{relative}:{n} length {line.Length}: {line}");
            }
        }
    }

    [Fact]
    public void Local_access_redaction_masks_secrets_and_cuts_at_120()
    {
        var local = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "local-access.md"));
        Assert.Contains("Cut at 120 chars", local, StringComparison.Ordinal);
        Assert.Contains("cut -c1-120", local, StringComparison.Ordinal);
        Assert.Contains("[redacted]", local, StringComparison.Ordinal);
        const string fakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.faketokensigforlocalaccesstestonly";
        var line =
            $"Authorization Bearer {fakeJwt} and padding to exceed one hundred twenty characters in the redacted startup log line XX";
        Assert.True(line.Length > 120);
        const string redact = """
            printf '%s' "$1" | sed -E \
              -e 's/eyJ[A-Za-z0-9_-]*\.[A-Za-z0-9_-]*\.[A-Za-z0-9_-]*/[redacted]/g' \
              -e 's/(AKIA|ASIA)[A-Z0-9]{16}/[redacted]/g' \
              -e 's/ya29\.[A-Za-z0-9_-]+/[redacted]/g' \
              -e 's/(key|password|secret)=[^[:space:]]+/[redacted]/g' \
              -e 's/aws_secret_access_key[^[:space:]]*/[redacted]/g' \
              -e 's/X-Amz-Security-Token[^[:space:]]*/[redacted]/g' \
              -e 's/[A-Za-z0-9+\/=_-]{40,}/[redacted]/g' \
              | cut -c1-120
            """;
        var start = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "sh",
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add(redact);
        start.ArgumentList.Add("redact");
        start.ArgumentList.Add(line);
        using var process = System.Diagnostics.Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd();
        Assert.True(process.WaitForExit(TimeSpan.FromSeconds(10)));
        Assert.Equal(0, process.ExitCode);
        Assert.DoesNotContain(fakeJwt, output, StringComparison.Ordinal);
        Assert.Contains("[redacted]", output, StringComparison.Ordinal);
        Assert.True(output.TrimEnd().Length <= 120, $"redacted length {output.Length}: {output}");
    }

    [Fact]
    public void Drive_uses_inline_token_or_sigv4_never_stored()
    {
        var local = File.ReadAllText(Path.Combine(VerifyHarnessDir(), "references", "local-access.md"));
        Assert.Contains("$(az account get-access-token --scope <s> --query accessToken -o tsv)", local, StringComparison.Ordinal);
        Assert.Contains("SigV4", local, StringComparison.Ordinal);
        Assert.Contains("no echo, no stored variable, no `-v`/`--trace`", local, StringComparison.Ordinal);
        Assert.Contains("Tokens never in any markdown, never shown to the model", local, StringComparison.Ordinal);
    }

    [Fact]
    public void Scenarios_command_names_full_skill_path_never_find()
    {
        var command = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands", "scenarios.md"));
        Assert.Contains("`<Squad plugin root>/skills/scenarios-md/SKILL.md`", command, StringComparison.Ordinal);
        Assert.Contains("never run `find` for it", command, StringComparison.Ordinal);
    }

    [Fact]
    public void Small_change_verify_runs_wider_unit_suite_once()
    {
        var skill = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "skills", "squad", "SKILL.md"));
        Assert.Contains("verify (verify cmd, then the wider unit suite once;", skill, StringComparison.Ordinal);
        Assert.Contains("Verify runs that command, then the wider unit suite once.", skill, StringComparison.Ordinal);
        Assert.Contains("verify directly: run the verify command, then the wider unit suite once.",
            File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands", "squad.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Implementer_writes_no_comments_and_simplifier_flags_them()
    {
        var agents = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "agents");
        Assert.Contains("- Prefer no new comments. Put a constraint in a test, type or lint instead, because agents copy comments. Write a comment only when none of those can hold it, and say why in it.", File.ReadAllText(Path.Combine(agents, "squad-implementer.md")), StringComparison.Ordinal);
        Assert.Contains("Exception: public API doc comments a pack standard asks for stay (C# `///` XML docs, rustdoc `///`, TSDoc on exported symbols); an inline or workaround comment needs that reason.", File.ReadAllText(Path.Combine(agents, "squad-implementer.md")), StringComparison.Ordinal);
        Assert.Contains("List every comment added in the diff by `file:line` with `kept` or `not kept` and the reason.", File.ReadAllText(Path.Combine(agents, "squad-simplifier.md")), StringComparison.Ordinal);
        Assert.Contains("- A comment in the diff without a stated reason a test, type or lint can't hold is a finding. Keep one for something we can't change (an external API quirk, a vendor bug) when it says why. Public API doc comments a pack standard asks for (C# `///` XML docs, rustdoc `///`, TSDoc on exported symbols) are not findings.", File.ReadAllText(Path.Combine(agents, "squad-simplifier.md")), StringComparison.Ordinal);
    }

    private static string VerifyHarnessDir() => Path.Combine(
        TestRepository.SourceRoot(), "plugins", "squad", "skills", "verify-harness");

    private static string VerifyHarnessBody() => string.Join("\n",
        new[]
        {
            "SKILL.md",
            "references/smoke-report.md",
            "references/doctor-tags.md",
            "references/process.md",
            "references/local-access.md",
        }
            .Select(f => File.ReadAllText(Path.Combine(VerifyHarnessDir(), f))));
}
