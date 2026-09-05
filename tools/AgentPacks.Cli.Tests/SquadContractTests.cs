namespace AgentPacks.Cli.Tests;

/// <summary>Guards the authored and generated Squad behavioral contracts.</summary>
public class SquadContractTests
{
    private const string Manifest = """
        {
          "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
          "name": "squad",
          "description": "Test Squad."
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
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "squad", name));

    [Fact]
    public void All_seven_agents_are_preserved_in_every_generated_agent_client()
    {
        using var repo = new TestRepository().WithPlugin("squad", Manifest);
        foreach (var agent in AgentNames)
            repo.WithFile($"plugins/squad/agents/{agent}.md", Fixture($"{agent}.md"));

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.Equal(7, AgentNames.Length);
        foreach (var agent in AgentNames)
        {
            Assert.Contains($"name: {agent}", Fixture($"{agent}.md"), StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/squad/com.anthropic.claude-code/agents/{agent}.md").Text));
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/squad/com.openai.codex/agents/{agent}.toml").Text));
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/squad/com.github.copilot/agents/{agent}.agent.md").Text));
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
        var command = Fixture("squad.md");

        foreach (var agent in AgentNames)
            Assert.Contains(agent, command, StringComparison.Ordinal);

        Assert.Contains("main agent implements and verifies directly", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not call the planner, implementer, verifier, orchestrator, or reviewers", command, StringComparison.Ordinal);
        Assert.Contains("do not create a plan", command, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("small-change route bypasses this agent", Fixture("loop-implementer.md"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("small-change route bypasses this agent", Fixture("loop-verifier.md"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Gated_agents_spawn_only_when_their_gate_says()
    {
        var skill = Fixture("SKILL.md");
        var simplifier = Fixture("loop-simplifier.md");
        var security = Fixture("loop-security-reviewer.md");
        var orchestrator = Fixture("loop-orchestrator.md");
        var verifier = Fixture("loop-verifier.md");

        Assert.Contains("`loop-planner` | full change only | grill/plan", skill, StringComparison.Ordinal);
        Assert.Contains("`loop-implementer` | full change only | build", skill, StringComparison.Ordinal);
        Assert.Contains("`loop-verifier` | after implement/fix", skill, StringComparison.Ordinal);
        Assert.Contains("`not verified` is not a pass", skill, StringComparison.Ordinal);
        Assert.Contains("`loop-reviewer` | every review phase | correctness + plan/spec", skill, StringComparison.Ordinal);
        Assert.Contains("`loop-simplifier` | every review phase | reuse, quality, efficiency in one spawn", skill, StringComparison.Ordinal);
        Assert.Contains("`loop-security-reviewer` | trust boundary only | OWASP gate", skill, StringComparison.Ordinal);
        Assert.Contains("`loop-orchestrator` | merge only | verdict / ≤2 fixes", skill, StringComparison.Ordinal);
        Assert.Contains("spawn none of these", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("loop-tester", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("On-Call", skill, StringComparison.Ordinal);

        Assert.Contains("reuse, quality, efficiency", simplifier, StringComparison.Ordinal);
        Assert.Contains("Conditional security gate", security, StringComparison.Ordinal);
        Assert.Contains("Merge step", orchestrator, StringComparison.Ordinal);
        Assert.Contains("No plan is not a pass", verifier, StringComparison.Ordinal);
        Assert.Equal(7, AgentNames.Length);
        Assert.DoesNotContain("loop-tester", AgentNames);
        Assert.DoesNotContain(AgentNames, name => name.Contains("on-call", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Main_agent_fans_reviewers_out_and_orchestrator_only_merges_completed_reports()
    {
        var command = Fixture("squad.md");
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
        var command = Fixture("squad.md");
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
    public void Two_user_invoked_entrypoints_are_squad_and_review()
    {
        var skill = Fixture("SKILL.md");
        var squad = Fixture("squad.md");
        var review = Fixture("review.md");

        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.Contains("Two entrypoints only", skill, StringComparison.Ordinal);
        Assert.Contains("`/squad`", skill, StringComparison.Ordinal);
        Assert.Contains("`/review`", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("`/squad` or `/build`", skill, StringComparison.Ordinal);
        Assert.Contains("name: squad", squad, StringComparison.Ordinal);
        Assert.Contains("name: review", review, StringComparison.Ordinal);
        Assert.DoesNotContain("name: build", squad + review, StringComparison.Ordinal);
        Assert.DoesNotContain("name: deliver", squad + review, StringComparison.Ordinal);
        Assert.DoesNotContain("name: review-diff", squad + review, StringComparison.Ordinal);
    }

    [Fact]
    public void Squad_commands_are_exactly_squad_and_review()
    {
        var directory = Path.Combine(SourceRoot(), "plugins", "squad", "commands");
        var names = Directory.GetFiles(directory, "*.md")
            .Select(path => Path.GetFileName(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["review.md", "squad.md"], names);
        Assert.DoesNotContain("build.md", names, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("name: review", File.ReadAllText(Path.Combine(directory, "review.md")),
            StringComparison.Ordinal);
        Assert.Contains("name: squad", File.ReadAllText(Path.Combine(directory, "squad.md")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Skills_are_loaded_by_exact_tool_name_never_slash_prose()
    {
        var combined = string.Join('\n',
            Fixture("SKILL.md"),
            Fixture("squad.md"),
            Fixture("review.md"),
            Fixture("loop-planner.md"),
            Fixture("loop-implementer.md"),
            Fixture("loop-verifier.md"),
            Fixture("loop-reviewer.md"),
            Fixture("loop-simplifier.md"));

        Assert.Contains("Skill tool by exact name", combined, StringComparison.Ordinal);
        Assert.Contains("Never write `/squad`", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Load `/squad`", combined, StringComparison.Ordinal);
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
    public void Optional_decisions_drop_box_is_read_not_eager_memory()
    {
        var skill = Fixture("SKILL.md");
        var contract = Fixture("planning-contract.md");
        var learnings = Fixture("learnings.md");

        Assert.Contains("docs/decisions.md", skill, StringComparison.Ordinal);
        Assert.Contains("do not write it", skill, StringComparison.Ordinal);
        Assert.Contains("docs/decisions.md", contract, StringComparison.Ordinal);
        Assert.Contains("human-readable drop-box", contract, StringComparison.Ordinal);
        Assert.Contains("not eager memory", contract, StringComparison.Ordinal);
        Assert.Contains("Never create, edit, or append that file", contract, StringComparison.Ordinal);
        Assert.Contains("Decisions table", contract, StringComparison.Ordinal);
        Assert.Contains("Human drop-box:", contract, StringComparison.Ordinal);
        Assert.Contains("optional human drop-box", learnings, StringComparison.Ordinal);
        Assert.Contains("not a vibe", learnings, StringComparison.Ordinal);
        Assert.DoesNotContain("do not write `decisions.md`", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void Loop_agents_restore_operational_steps_not_empty_tiny()
    {
        var implementer = Fixture("loop-implementer.md");
        var planner = Fixture("loop-planner.md");
        var verifier = Fixture("loop-verifier.md");
        var reviewer = Fixture("loop-reviewer.md");
        var simplifier = Fixture("loop-simplifier.md");
        var security = Fixture("loop-security-reviewer.md");
        var orchestrator = Fixture("loop-orchestrator.md");

        Assert.Contains("references/standards/", implementer, StringComparison.Ordinal);
        Assert.Contains("Standards in force", implementer, StringComparison.Ordinal);
        Assert.Contains("Standards followed", implementer, StringComparison.Ordinal);
        Assert.Contains("You are not the author of the rejected code", implementer, StringComparison.Ordinal);
        Assert.Contains("Good:", implementer, StringComparison.Ordinal);
        Assert.Contains("docs/decisions.md", planner, StringComparison.Ordinal);
        Assert.Contains("find facts yourself", planner, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Visit applicable branches", planner, StringComparison.Ordinal);
        Assert.Contains("not covered", verifier, StringComparison.Ordinal);
        Assert.Contains("this-change", verifier, StringComparison.Ordinal);
        Assert.Contains("Do not edit a test to make it pass", verifier, StringComparison.Ordinal);
        Assert.Contains("Dual-axis", reviewer, StringComparison.Ordinal);
        Assert.Contains("cite the document", reviewer, StringComparison.Ordinal);
        Assert.Contains("references/standards/", reviewer, StringComparison.Ordinal);
        Assert.Contains("references/standards/", simplifier, StringComparison.Ordinal);
        Assert.Contains("Deletion test", simplifier, StringComparison.Ordinal);
        Assert.Contains("cite", security, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("input-error", orchestrator, StringComparison.Ordinal);
        Assert.Contains("not verified", orchestrator, StringComparison.Ordinal);
        Assert.Contains("blocks `pass`", orchestrator, StringComparison.Ordinal);
    }

    /// <summary>
    /// The locked v1 plan is visible in-repo, not implied by code. README, plugin README, and
    /// the orchestrator skill must carry the same numbered block so a reader sees it immediately.
    /// </summary>
    [Fact]
    public void Readme_mirrors_the_orchestrator_numbered_flow()
    {
        const string flow = """
            /squad (user-invoked orchestrator)
            1. Read and apply learnings.md (append-only): prefer passed skips/tiers; avoid what failed
            2. Orient codebase (applicable stacks only)
            3. Small change? → main agent only, spawn nobody → verify → append learnings → hand off uncommitted
            4. Else grill/plan rounds (facts via subagent; decisions = human) → write plan
            5. Gate spins: implementer → verifier → reviewers in parallel (correctness + plan/spec; security ONLY if trust boundary)
            6. Orchestrator merges ≤2 fix rounds → hand off uncommitted → append learnings

            /review
            Pin vs PR / uncommitted / main → same gated dual-axis reviewers (no plan/fix loop) → append learnings

            Always
            models.source.json tiers (default inherit); load only contracted <lang>-* by Skill name; coworker docs = real dotnet test/validate on a fixture.

            Not in v1
            second skill pack, Matt catalog dump, eager fan-out, self-improve graphs, auto skill rewrite, redoing PR #6.
            """;

        var root = SourceRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var pluginReadme = File.ReadAllText(Path.Combine(root, "plugins", "squad", "README.md"));
        var skill = Fixture("SKILL.md");

        Assert.Contains(flow, readme, StringComparison.Ordinal);
        Assert.Contains(flow, pluginReadme, StringComparison.Ordinal);
        Assert.Contains(flow, skill, StringComparison.Ordinal);
        Assert.Contains("Apply the latest same-entrypoint entry", skill, StringComparison.Ordinal);
        Assert.Contains("PR", Fixture("review.md"), StringComparison.Ordinal);
        Assert.Contains("uncommitted", Fixture("review.md"), StringComparison.Ordinal);
        Assert.Contains("main", Fixture("review.md"), StringComparison.Ordinal);

        var coworkerDocs = File.ReadAllText(Path.Combine(root, "docs", "ADD-SKILL.md"));
        Assert.Contains("Do not publish to the marketplace branch", coworkerDocs, StringComparison.Ordinal);
        Assert.Contains("without merging to `main`", coworkerDocs, StringComparison.Ordinal);
        Assert.Contains("dotnet test tools/AgentPacks.slnx", coworkerDocs, StringComparison.Ordinal);
        Assert.Contains("validate-all --out", coworkerDocs, StringComparison.Ordinal);
    }

    [Fact]
    public void Coworker_local_dev_docs_name_validate_test_and_three_client_installs()
    {
        var root = SourceRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var docs = File.ReadAllText(Path.Combine(root, "docs", "ADD-SKILL.md"));
        foreach (var text in new[] { readme, docs })
        {
            Assert.Contains("dotnet run --project tools/AgentPacks.Cli -- validate", text, StringComparison.Ordinal);
            Assert.Contains("dotnet test tools/AgentPacks.Cli.Tests", text, StringComparison.Ordinal);
            Assert.Contains("dotnet run --project plugins/dotnet/mcp/DotnetSolutionMcp.csproj -- --list-tools", text, StringComparison.Ordinal);
            Assert.Contains("claude --plugin-dir", text, StringComparison.Ordinal);
            Assert.Contains("~/.cursor/plugins/local", text, StringComparison.Ordinal);
            Assert.Contains("copilot plugin marketplace add /tmp/agentpacks-marketplace", text, StringComparison.Ordinal);
            Assert.Contains("/squad", text, StringComparison.Ordinal);
            Assert.Contains("/review", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void User_facing_surfaces_do_not_say_delivery_loop()
    {
        var root = SourceRoot();
        var leftovers = new List<string>();

        foreach (var directory in Directory.GetDirectories(Path.Combine(root, "plugins")))
        {
            var name = Path.GetFileName(directory) ?? directory;
            if (name.Contains("delivery", StringComparison.OrdinalIgnoreCase))
                leftovers.Add($"plugin directory '{name}'");
        }

        var commands = Directory.GetFiles(
            Path.Combine(root, "plugins", "squad", "commands"), "*.md");
        leftovers.AddRange(commands
            .Select(path => Path.GetFileName(path) ?? path)
            .Where(name => name.Contains("delivery", StringComparison.OrdinalIgnoreCase)
                || name.Equals("build.md", StringComparison.OrdinalIgnoreCase))
            .Select(name => $"command '{name}'"));

        var surfaces = new List<string> { Path.Combine(root, "README.md") };
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "plugins"), "README.md",
            SearchOption.AllDirectories));
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "plugins"), "plugin.json",
            SearchOption.AllDirectories));
        surfaces.AddRange(Directory.GetFiles(
            Path.Combine(root, "plugins", "squad", "commands"), "*.md"));
        surfaces.Add(Path.Combine(root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "docs"), "ADD-*.md"));

        foreach (var path in surfaces.Distinct(StringComparer.Ordinal))
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(
                    File.ReadAllText(path),
                    @"\bdelivery\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                leftovers.Add(Path.GetRelativePath(root, path));
            }
        }

        Assert.False(
            leftovers.Count > 0,
            "leftover delivery* on user-facing surfaces: " + string.Join(", ", leftovers));
        Assert.Equal(["review.md", "squad.md"],
            commands.Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "delivery-loop")));
        Assert.DoesNotContain("delivery-loop@", File.ReadAllText(Path.Combine(root, "README.md")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Loop_agent_bodies_stay_tiny()
    {
        foreach (var agent in AgentNames)
        {
            var text = Fixture($"{agent}.md");
            var body = BodyAfterFrontmatter(text);
            var lines = body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
            var cap = agent == "loop-security-reviewer" ? 40 : 28;
            Assert.True(lines <= cap, $"{agent} body is {lines} lines; cap is {cap}.");
        }
    }

    /// <summary>
    /// Closing frontmatter fence only. LastIndexOf("---") is wrong: the OWASP table uses
    /// <c>|---|---|</c> and would drop the operational steps above it.
    /// </summary>
    private static string BodyAfterFrontmatter(string text)
    {
        const string fence = "---";
        var start = text.IndexOf(fence, StringComparison.Ordinal);
        Assert.True(start >= 0, "missing opening frontmatter fence");
        var end = text.IndexOf(fence, start + fence.Length, StringComparison.Ordinal);
        Assert.True(end > start, "missing closing frontmatter fence");
        return text[(end + fence.Length)..];
    }

    /// <summary>Po 17 fail bar: no user-facing delivery*, no essay-length agent bodies.</summary>
    [Fact]
    public void No_user_facing_delivery_star_and_no_essay_agents()
    {
        User_facing_surfaces_do_not_say_delivery_loop();
        Loop_agent_bodies_stay_tiny();
        Assert.DoesNotContain("loop-tester", string.Join('\n', AgentNames), StringComparison.Ordinal);
    }

    [Fact]
    public void Loop_agents_use_per_role_tiers_implementer_standard_others_fast()
    {
        Assert.Contains("model: standard", Fixture("loop-implementer.md"), StringComparison.Ordinal);
        foreach (var agent in AgentNames.Where(name => name != "loop-implementer"))
        {
            Assert.Contains("model: fast", Fixture($"{agent}.md"), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Learnings_log_is_append_only_and_read_first()
    {
        var skill = Fixture("SKILL.md");
        var learnings = Fixture("learnings.md");

        Assert.Contains("docs/learnings.md", skill, StringComparison.Ordinal);
        Assert.Contains("Read and apply", skill, StringComparison.Ordinal);
        Assert.Contains("docs/learnings.md", skill, StringComparison.Ordinal);
        Assert.Contains("append-only", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("failed", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must-run", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not a second brain", learnings, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Never edit or delete an earlier entry", learnings, StringComparison.Ordinal);
        Assert.Contains("Provider:", learnings, StringComparison.Ordinal);
        Assert.Contains("Model tier:", learnings, StringComparison.Ordinal);
        Assert.Contains("demotes one tier", learnings, StringComparison.Ordinal);
        Assert.Contains("Agents spun:", learnings, StringComparison.Ordinal);
        Assert.Contains("Next tweak:", learnings, StringComparison.Ordinal);
        Assert.Contains("does not rewrite skills", learnings, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void Pull_request_ci_stays_one_job_no_matrix()
    {
        var root = SourceRoot();
        var workflows = Directory.GetFiles(Path.Combine(root, ".github", "workflows"), "*.yml");
        Assert.Equal(
            ["drift.yml", "publish-marketplace.yml", "validate.yml"],
            workflows.Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));

        foreach (var path in workflows)
        {
            var name = Path.GetFileName(path) ?? path;
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("strategy:", text, StringComparison.Ordinal);
            Assert.DoesNotContain("matrix:", text, StringComparison.Ordinal);
            Assert.False(
                name.Contains("mcp", StringComparison.OrdinalIgnoreCase),
                $"workflow '{name}' is backlog MCP CI");
            if (!name.Equals("validate.yml", StringComparison.Ordinal))
                Assert.DoesNotContain("pull_request:", text, StringComparison.Ordinal);
        }

        var validate = File.ReadAllText(Path.Combine(root, ".github", "workflows", "validate.yml"));
        Assert.Contains("pull_request:", validate, StringComparison.Ordinal);
        Assert.Equal(1, CountToken(validate, "runs-on:"));
        Assert.Contains("dotnet test", validate, StringComparison.Ordinal);
        Assert.Contains("validate-all --out", validate, StringComparison.Ordinal);
    }

    private static int CountToken(string text, string token)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0; index += token.Length)
            count++;
        return count;
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
