namespace AgentPacks.Cli.Tests;

/// <summary>Guards the authored and generated delivery-loop behavioral contracts.</summary>
public class DeliveryLoopContractTests
{
    private const string Manifest = """
        {
          "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
          "name": "delivery-loop",
          "description": "Test delivery loop."
        }
        """;

    private static readonly string[] AgentNames =
    [
        "loop-planner",
        "loop-implementer",
        "loop-verifier",
        "loop-reviewer",
        "loop-security-reviewer",
        "loop-simplifier",
        "loop-orchestrator"
    ];

    private static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "delivery-loop", name));

    [Fact]
    public void All_seven_agents_are_preserved_in_every_generated_agent_client()
    {
        using var repo = new TestRepository().WithPlugin("delivery-loop", Manifest);
        foreach (var agent in AgentNames)
            repo.WithFile($"plugins/delivery-loop/agents/{agent}.md", Fixture($"{agent}.md"));

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.Equal(7, AgentNames.Length);
        foreach (var agent in AgentNames)
        {
            Assert.Contains($"name: {agent}", Fixture($"{agent}.md"), StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/delivery-loop/com.anthropic.claude-code/agents/{agent}.md").Text));
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/delivery-loop/com.openai.codex/agents/{agent}.toml").Text));
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/delivery-loop/com.github.copilot/agents/{agent}.agent.md").Text));
        }
    }

    [Fact]
    public void Planner_is_turn_based_main_agent_mediated_and_writes_only_after_confirmation()
    {
        var planner = Fixture("loop-planner.md");
        var contract = Fixture("planning-contract.md");
        var combined = string.Join('\n', planner, contract);

        Assert.Contains("Return one numbered question round", planner, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not address the user", planner, StringComparison.Ordinal);
        Assert.Contains("The main agent owns user interaction and planning state", contract, StringComparison.Ordinal);
        Assert.Contains("User confirmation", contract, StringComparison.Ordinal);
        Assert.Contains("write exactly one file", contract, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("docs/plans/<slug>.md", combined, StringComparison.Ordinal);
        Assert.Contains("Ask the whole frontier in one round", contract, StringComparison.Ordinal);
        Assert.Contains("Finding facts is the planner's job, never the user's", contract, StringComparison.Ordinal);
        Assert.Contains("A fact still being researched is an unsettled prerequisite", contract, StringComparison.Ordinal);
        Assert.Contains("If the user says to stop asking and decide", contract, StringComparison.Ordinal);
        Assert.Contains("An open question is never a criterion", contract, StringComparison.Ordinal);
        Assert.Contains("The main agent presents that round to the user and", contract, StringComparison.Ordinal);
        Assert.Contains("waits for the answers", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("docs/" + "research", combined, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The planner itself never waits across turns or assumes an answer", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void Small_changes_bypass_every_plan_dependent_and_review_phase_agent()
    {
        var skill = Fixture("SKILL.md");
        var command = Fixture("build.md");

        foreach (var agent in AgentNames)
            Assert.Contains(agent, command, StringComparison.Ordinal);

        Assert.Contains("main agent implements and verifies directly", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not call the planner, implementer, verifier, orchestrator, or reviewers", command, StringComparison.Ordinal);
        Assert.Contains("do not create a plan", command, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("small-change route bypasses this agent", Fixture("loop-implementer.md"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("small-change route bypasses this agent", Fixture("loop-verifier.md"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Main_agent_fans_reviewers_out_and_orchestrator_only_merges_completed_reports()
    {
        var command = Fixture("build.md");
        var orchestrator = Fixture("loop-orchestrator.md");

        Assert.Contains("Directly launch", command, StringComparison.Ordinal);
        Assert.Contains("in parallel", command, StringComparison.Ordinal);
        Assert.Contains("main agent has already run applicable reviewers", orchestrator, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("completed reports", orchestrator, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("launch, retry, or hand work to another agent", orchestrator, StringComparison.Ordinal);
        Assert.Contains("do not decide what runs next", orchestrator, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Orchestrator_requires_merge_inputs_and_pass_requires_complete_evidence()
    {
        var orchestrator = Fixture("loop-orchestrator.md");
        var contract = Fixture("review-contract.md");

        Assert.Contains("round number", orchestrator, StringComparison.Ordinal);
        Assert.Contains("plan path", orchestrator, StringComparison.Ordinal);
        Assert.Contains("loop-verifier", orchestrator, StringComparison.Ordinal);
        Assert.Contains("security-gate decision", orchestrator, StringComparison.Ordinal);
        Assert.Contains("completed reports", orchestrator, StringComparison.Ordinal);
        Assert.Contains("`pass` additionally requires", contract, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fail` or `not verified` row blocks `pass", contract, StringComparison.Ordinal);
        Assert.Contains("Use the supplied plan path as `Location`", contract, StringComparison.Ordinal);
        Assert.Contains("## Orchestrator input error", contract, StringComparison.Ordinal);
        Assert.Contains("Round 1 is the initial implementation review", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void Applicable_stacks_come_from_change_scope_and_mixed_work_loads_both()
    {
        var skill = Fixture("SKILL.md");
        var implementer = Fixture("loop-implementer.md");
        var verifier = Fixture("loop-verifier.md");

        Assert.Contains("target paths, the existing diff, and acceptance criteria", skill, StringComparison.Ordinal);
        Assert.Contains("Rust-only", skill, StringComparison.Ordinal);
        Assert.Contains(".NET-only", skill, StringComparison.Ordinal);
        Assert.Contains("cross-language scope loads both", skill, StringComparison.Ordinal);
        Assert.Contains("never request or load a pack for code outside the change", skill, StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-build`", implementer, StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-test-patterns`", verifier, StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-review`", Fixture("loop-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-security-review`", Fixture("loop-security-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-build`", Fixture("loop-simplifier.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Orchestrator_normalizes_usable_noncanonical_reports_without_another_agent_call()
    {
        var orchestrator = Fixture("loop-orchestrator.md");
        var contract = Fixture("review-contract.md");

        Assert.Contains("Normalize a noncanonical-but-usable report in memory", orchestrator, StringComparison.Ordinal);
        Assert.Contains("Presentation differences alone are not malformed", contract, StringComparison.Ordinal);
        Assert.Contains("normalizes that presentation in memory", contract, StringComparison.Ordinal);
        Assert.Contains("does not ask the producing agent", contract, StringComparison.Ordinal);
        Assert.Contains("never meaning", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void Malformed_report_input_is_terminal_and_cannot_enter_a_retry_loop()
    {
        var skill = Fixture("SKILL.md");
        var command = Fixture("build.md");
        var standalone = Fixture("review.md");
        var orchestrator = Fixture("loop-orchestrator.md");
        var contract = Fixture("review-contract.md");
        var combined = string.Join('\n', skill, command, standalone, orchestrator, contract);

        Assert.Contains("Return the review contract's input-error shape for any missing or malformed report", orchestrator, StringComparison.Ordinal);
        Assert.Contains("surface it unchanged to the human and end the current", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No plan write, verdict, retry, fix round, or agent handoff is allowed", contract, StringComparison.Ordinal);
        Assert.Contains("Do not obtain another report, invoke merge again", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("obtain the named conforming report", combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Security_checklist_is_exclusively_OWASP_2025()
    {
        var security = Fixture("loop-security-reviewer.md");

        for (var category = 1; category <= 10; category++)
            Assert.Contains($"A{category:00}_2025", security, StringComparison.Ordinal);

        Assert.DoesNotContain("_20" + "21", security, StringComparison.Ordinal);
        Assert.Contains("A03_2025-Software_Supply_Chain_Failures", security, StringComparison.Ordinal);
        Assert.Contains("A05_2025-Injection", security, StringComparison.Ordinal);
        Assert.Contains("SSRF", security, StringComparison.Ordinal);
        Assert.Contains("A10_2025-Mishandling_of_Exceptional_Conditions", security, StringComparison.Ordinal);
    }

    [Fact]
    public void Review_without_a_plan_is_explicit_in_every_applicable_reviewer()
    {
        Assert.Contains("For `/review`", Fixture("loop-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("With `/review`", Fixture("loop-security-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("With `/review`", Fixture("loop-simplifier.md"), StringComparison.Ordinal);
        Assert.Contains("## Standalone merge report", Fixture("review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("There is no `Verdict`", Fixture("review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("`round number: 1`", Fixture("review.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Worktree_cleanup_preserves_uncommitted_or_externally_owned_work()
    {
        var skill = Fixture("SKILL.md");

        Assert.Contains("git status --porcelain", skill, StringComparison.Ordinal);
        Assert.Contains("git worktree remove <exact-path>", skill, StringComparison.Ordinal);
        Assert.Contains("without `--force`", skill, StringComparison.Ordinal);
        Assert.Contains("externally created worktrees", skill, StringComparison.Ordinal);
        Assert.Contains("Preserve dirty worktrees", skill, StringComparison.Ordinal);
    }

    [Fact]
    public void Planning_and_review_each_have_one_contract_owner()
    {
        Assert.Contains("references/planning-contract.md", Fixture("SKILL.md"), StringComparison.Ordinal);
        Assert.Contains("references/planning-contract.md", Fixture("loop-planner.md"), StringComparison.Ordinal);
        Assert.Contains("references/review-contract.md", Fixture("SKILL.md"), StringComparison.Ordinal);

        foreach (var agent in new[] { "loop-orchestrator.md", "loop-reviewer.md", "loop-security-reviewer.md", "loop-simplifier.md" })
            Assert.Contains("references/review-contract.md", Fixture(agent), StringComparison.Ordinal);
    }

    [Fact]
    public void Two_user_invoked_entrypoints_are_build_and_review()
    {
        var skill = Fixture("SKILL.md");
        var build = Fixture("build.md");
        var review = Fixture("review.md");

        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.Contains("Two entrypoints only", skill, StringComparison.Ordinal);
        Assert.Contains("`/build`", skill, StringComparison.Ordinal);
        Assert.Contains("`/review`", skill, StringComparison.Ordinal);
        Assert.Contains("name: build", build, StringComparison.Ordinal);
        Assert.Contains("name: review", review, StringComparison.Ordinal);
        Assert.DoesNotContain("name: deliver", build + review, StringComparison.Ordinal);
        Assert.DoesNotContain("name: review-diff", build + review, StringComparison.Ordinal);
    }

    [Fact]
    public void Skills_are_loaded_by_exact_tool_name_never_slash_prose()
    {
        var combined = string.Join('\n',
            Fixture("SKILL.md"),
            Fixture("build.md"),
            Fixture("review.md"),
            Fixture("loop-planner.md"),
            Fixture("loop-implementer.md"),
            Fixture("loop-verifier.md"),
            Fixture("loop-reviewer.md"),
            Fixture("loop-simplifier.md"));

        Assert.Contains("Skill tool by exact name", combined, StringComparison.Ordinal);
        Assert.Contains("Never write `/delivery-loop`", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Load `/delivery-loop`", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Load `/dotnet-build`", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("mattpocock", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("grill-me", combined, StringComparison.Ordinal);
        Assert.Contains("Grill stays", Fixture("SKILL.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Review_is_dual_axis_and_startable_for_pr_uncommitted_and_main()
    {
        var skill = Fixture("SKILL.md");
        var review = Fixture("review.md");
        var reviewer = Fixture("loop-reviewer.md");

        Assert.Contains("Dual-axis", skill, StringComparison.Ordinal);
        Assert.Contains("correctness and plan/spec", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--pr", review, StringComparison.Ordinal);
        Assert.Contains("--uncommitted", review, StringComparison.Ordinal);
        Assert.Contains("vs main", review, StringComparison.Ordinal);
        Assert.Contains("PR, uncommitted work, or a diff versus main", reviewer, StringComparison.Ordinal);
    }

    [Fact]
    public void Author_is_not_the_fixer_and_implement_stays_tdd_then_one_suite()
    {
        var skill = Fixture("SKILL.md");
        var implementer = Fixture("loop-implementer.md");
        var verifier = Fixture("loop-verifier.md");

        Assert.Contains("author of the rejected code is not the fixer", skill, StringComparison.Ordinal);
        Assert.Contains("fresh", implementer, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TDD", implementer, StringComparison.Ordinal);
        Assert.Contains("Do not run the full suite", implementer, StringComparison.Ordinal);
        Assert.Contains("wider test suite once", verifier, StringComparison.Ordinal);
        Assert.Contains("Happy-path-only", implementer, StringComparison.Ordinal);
    }

    [Fact]
    public void Learnings_log_is_append_only_and_read_first()
    {
        var skill = Fixture("SKILL.md");
        var learnings = Fixture("learnings.md");

        Assert.Contains("docs/learnings.md", skill, StringComparison.Ordinal);
        Assert.Contains("Read `docs/learnings.md` first", skill, StringComparison.Ordinal);
        Assert.Contains("append-only", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not a second brain", learnings, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Never edit or delete an earlier entry", learnings, StringComparison.Ordinal);
        Assert.Contains("Model tier:", learnings, StringComparison.Ordinal);
        Assert.Contains("Agents spun:", learnings, StringComparison.Ordinal);
        Assert.Contains("Next tweak:", learnings, StringComparison.Ordinal);
        Assert.DoesNotContain("rewrite skills", learnings, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Verify_gates_are_written_in_the_review_contract()
    {
        var contract = Fixture("review-contract.md");
        var verifier = Fixture("loop-verifier.md");

        Assert.Contains("No confirmed plan", contract, StringComparison.Ordinal);
        Assert.Contains("never covers the criterion", contract, StringComparison.Ordinal);
        Assert.Contains("wider suite fails", contract, StringComparison.Ordinal);
        Assert.Contains("this-change", contract, StringComparison.Ordinal);
        Assert.Contains("pre-existing", contract, StringComparison.Ordinal);
        Assert.Contains("happy-path only", contract, StringComparison.Ordinal);
        Assert.Contains("Mixed .NET and Rust", contract, StringComparison.Ordinal);
        Assert.Contains("No plan is not a pass", verifier, StringComparison.Ordinal);
        Assert.Contains("not a verified pass", verifier, StringComparison.Ordinal);
    }
}
