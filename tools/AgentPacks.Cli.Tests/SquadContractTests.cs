using System.Text.Json.Nodes;
using AgentPacks.Cli.Importing;
using AgentPacks.Cli.Loading;

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
        "squad-planner",
        "squad-implementer",
        "squad-verifier",
        "squad-reviewer",
        "squad-security-reviewer",
        "squad-simplifier",
        "squad-orchestrator"
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
        var planner = Fixture("squad-planner.md");
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
        Assert.Contains("small-change route bypasses this agent", Fixture("squad-implementer.md"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("small-change route bypasses this agent", Fixture("squad-verifier.md"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Gated_agents_spawn_only_when_their_gate_says()
    {
        var skill = Fixture("SKILL.md");
        var simplifier = Fixture("squad-simplifier.md");
        var security = Fixture("squad-security-reviewer.md");
        var orchestrator = Fixture("squad-orchestrator.md");
        var verifier = Fixture("squad-verifier.md");

        Assert.Contains("`squad-planner` | full change only | grill/plan", skill, StringComparison.Ordinal);
        Assert.Contains("`squad-implementer` | full change only | build", skill, StringComparison.Ordinal);
        Assert.Contains("`squad-verifier` | after implement/fix", skill, StringComparison.Ordinal);
        Assert.Contains("`not verified` is not a pass", skill, StringComparison.Ordinal);
        Assert.Contains("`squad-reviewer` | every review phase | correctness + plan/spec", skill, StringComparison.Ordinal);
        Assert.Contains("`squad-simplifier` | every review phase | reuse, quality, efficiency in one spawn", skill, StringComparison.Ordinal);
        Assert.Contains("`squad-security-reviewer` | trust boundary only | OWASP gate", skill, StringComparison.Ordinal);
        Assert.Contains("`squad-orchestrator` | merge only | verdict / ≤2 fixes", skill, StringComparison.Ordinal);
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
        var orchestrator = Fixture("squad-orchestrator.md");

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
        var orchestrator = Fixture("squad-orchestrator.md");
        var contract = Fixture("review-contract.md");

        Assert.Contains("round number", orchestrator, StringComparison.Ordinal);
        Assert.Contains("plan path", orchestrator, StringComparison.Ordinal);
        Assert.Contains("squad-verifier", orchestrator, StringComparison.Ordinal);
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
        var implementer = Fixture("squad-implementer.md");
        var verifier = Fixture("squad-verifier.md");

        Assert.Contains("target paths, the existing diff, and acceptance criteria", skill, StringComparison.Ordinal);
        Assert.Contains("Rust-only", skill, StringComparison.Ordinal);
        Assert.Contains(".NET-only", skill, StringComparison.Ordinal);
        Assert.Contains("cross-language scope loads both", skill, StringComparison.Ordinal);
        Assert.Contains("never request or load a pack for code outside the change", skill, StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-build`", implementer, StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-test-patterns`", verifier, StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-review`", Fixture("squad-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-security-review`", Fixture("squad-security-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("every applicable stack's `<lang>-build`", Fixture("squad-simplifier.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Orchestrator_normalizes_usable_noncanonical_reports_without_another_agent_call()
    {
        var orchestrator = Fixture("squad-orchestrator.md");
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
        var standalone = Fixture("squad-review.md");
        var orchestrator = Fixture("squad-orchestrator.md");
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
        var security = Fixture("squad-security-reviewer.md");

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
        Assert.Contains("For `/squad-review`", Fixture("squad-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("With `/squad-review`", Fixture("squad-security-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("With `/squad-review`", Fixture("squad-simplifier.md"), StringComparison.Ordinal);
        Assert.Contains("## Standalone merge report", Fixture("review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("There is no `Verdict`", Fixture("review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("`round number: 1`", Fixture("squad-review.md"), StringComparison.Ordinal);
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
        Assert.Contains("references/planning-contract.md", Fixture("squad-planner.md"), StringComparison.Ordinal);
        Assert.Contains("references/review-contract.md", Fixture("SKILL.md"), StringComparison.Ordinal);

        foreach (var agent in new[] { "squad-orchestrator.md", "squad-reviewer.md", "squad-security-reviewer.md", "squad-simplifier.md" })
            Assert.Contains("references/review-contract.md", Fixture(agent), StringComparison.Ordinal);
    }

    [Fact]
    public void Two_user_invoked_entrypoints_are_squad_and_review()
    {
        var skill = Fixture("SKILL.md");
        var squad = Fixture("squad.md");
        var review = Fixture("squad-review.md");

        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.Contains("Two entrypoints only", skill, StringComparison.Ordinal);
        Assert.Contains("`/squad`", skill, StringComparison.Ordinal);
        Assert.Contains("`/squad-review`", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("`/squad` or `/build`", skill, StringComparison.Ordinal);
        Assert.Contains("name: squad", squad, StringComparison.Ordinal);
        Assert.Contains("name: squad-review", review, StringComparison.Ordinal);
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

        Assert.Equal(["scenarios.md", "squad-review.md", "squad.md"], names);
        Assert.DoesNotContain("build.md", names, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("squad-smoke.md", names, StringComparer.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(directory, "review.md")));
        Assert.Contains("name: squad-review", File.ReadAllText(Path.Combine(directory, "squad-review.md")),
            StringComparison.Ordinal);
        Assert.Contains("name: squad", File.ReadAllText(Path.Combine(directory, "squad.md")),
            StringComparison.Ordinal);
        Assert.Contains("name: scenarios", File.ReadAllText(Path.Combine(directory, "scenarios.md")),
            StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(directory, "http-scenarios.md")));
    }

    [Fact]
    public void Skills_are_loaded_by_exact_tool_name_never_slash_prose()
    {
        var combined = string.Join('\n',
            Fixture("SKILL.md"),
            Fixture("squad.md"),
            Fixture("squad-review.md"),
            Fixture("squad-planner.md"),
            Fixture("squad-implementer.md"),
            Fixture("squad-verifier.md"),
            Fixture("squad-reviewer.md"),
            Fixture("squad-simplifier.md"));

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
        var review = Fixture("squad-review.md");
        var reviewer = Fixture("squad-reviewer.md");

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
        var implementer = Fixture("squad-implementer.md");
        var verifier = Fixture("squad-verifier.md");

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
        var implementer = Fixture("squad-implementer.md");
        var planner = Fixture("squad-planner.md");
        var verifier = Fixture("squad-verifier.md");
        var reviewer = Fixture("squad-reviewer.md");
        var simplifier = Fixture("squad-simplifier.md");
        var security = Fixture("squad-security-reviewer.md");
        var orchestrator = Fixture("squad-orchestrator.md");

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

            /squad-review
            Pin vs PR / uncommitted / main → same gated dual-axis reviewers (no plan/fix loop) → append learnings → one save-markdown ask

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
        Assert.Contains("PR", Fixture("squad-review.md"), StringComparison.Ordinal);
        Assert.Contains("uncommitted", Fixture("squad-review.md"), StringComparison.Ordinal);
        Assert.Contains("main", Fixture("squad-review.md"), StringComparison.Ordinal);

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
            Assert.DoesNotContain("DotnetSolutionMcp", text, StringComparison.Ordinal);
            Assert.Contains("claude --plugin-dir", text, StringComparison.Ordinal);
            Assert.Contains("~/.cursor/plugins/local", text, StringComparison.Ordinal);
            Assert.Contains("copilot plugin marketplace add /tmp/agentpacks-marketplace", text, StringComparison.Ordinal);
            Assert.Contains("/squad", text, StringComparison.Ordinal);
            Assert.Contains("/squad-review", text, StringComparison.Ordinal);
            Assert.DoesNotMatch(@"(?<![A-Za-z0-9-])/review(?![A-Za-z0-9-])", text);
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
        surfaces.Add(Path.Combine(root, "plugins", "squad", "skills", "learnings-digest", "SKILL.md"));
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
        Assert.Equal(["scenarios.md", "squad-review.md", "squad.md"],
            commands.Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "delivery-loop")));
        Assert.DoesNotContain("delivery-loop@", File.ReadAllText(Path.Combine(root, "README.md")),
            StringComparison.Ordinal);
    }

    /// <summary>Po 22 fail bar: agents are squad-*; user-facing surfaces have no loop-* agents.</summary>
    [Fact]
    public void No_user_facing_loop_star_agents_are_squad_star()
    {
        var root = SourceRoot();
        var agentsDir = Path.Combine(root, "plugins", "squad", "agents");
        var names = Directory.GetFiles(agentsDir, "*.md")
            .Select(path => Path.GetFileNameWithoutExtension(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
        [
            "squad-implementer",
            "squad-orchestrator",
            "squad-planner",
            "squad-reviewer",
            "squad-security-reviewer",
            "squad-simplifier",
            "squad-verifier"
        ], names);
        foreach (var name in names)
        {
            Assert.StartsWith("squad-", name, StringComparison.Ordinal);
            Assert.Contains($"name: {name}", File.ReadAllText(Path.Combine(agentsDir, $"{name}.md")),
                StringComparison.Ordinal);
        }

        var leftovers = new List<string>();
        var surfaces = new List<string> { Path.Combine(root, "README.md") };
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "plugins"), "README.md",
            SearchOption.AllDirectories));
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "plugins"), "plugin.json",
            SearchOption.AllDirectories));
        surfaces.AddRange(Directory.GetFiles(
            Path.Combine(root, "plugins", "squad", "commands"), "*.md"));
        surfaces.Add(Path.Combine(root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        surfaces.Add(Path.Combine(root, "plugins", "squad", "skills", "learnings-digest", "SKILL.md"));
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "docs"), "ADD-*.md"));
        surfaces.AddRange(Directory.GetFiles(agentsDir, "*.md"));

        var pattern = @"\bloop-(planner|implementer|verifier|reviewer|simplifier|security-reviewer|orchestrator|tester)\b";
        foreach (var path in surfaces.Distinct(StringComparer.Ordinal))
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(
                    File.ReadAllText(path),
                    pattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                leftovers.Add(Path.GetRelativePath(root, path));
            }
        }

        Assert.False(
            leftovers.Count > 0,
            "leftover loop-* agents on user-facing surfaces: " + string.Join(", ", leftovers));
        Assert.False(File.Exists(Path.Combine(agentsDir, "loop-planner.md")));
    }

    [Fact]
    public void Loop_agent_bodies_stay_tiny()
    {
        foreach (var agent in AgentNames)
        {
            var text = Fixture($"{agent}.md");
            var body = BodyAfterFrontmatter(text);
            var lines = body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
            var cap = agent == "squad-security-reviewer" ? 44 : 32;
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

    /// <summary>
    /// Po 19 fail bar: usable gates/ops (not too thin to run), not essay-length,
    /// exactly /squad + /squad-review, authored MCP files are empty scaffolds.
    /// </summary>
    [Fact]
    public void Usable_gates_not_essays_exactly_two_commands_and_empty_mcp_scaffolds()
    {
        Loop_agents_restore_operational_steps_not_empty_tiny();
        Loop_agent_bodies_stay_tiny();
        Squad_commands_are_exactly_squad_and_review();

        Loop_agent_bodies_stay_above_the_thin_floor();
        Plugin_mcp_files_are_empty_scaffolds();
    }

    /// <summary>
    /// Po 34: leftover mcp_only_in_dotnet name is gone; the bar is empty mcp scaffolds.
    /// </summary>
    [Fact]
    public void Empty_mcp_scaffolds_test_is_not_named_dotnet_only()
    {
        var leftover = typeof(SquadContractTests).Assembly.GetTypes()
            .SelectMany(type => type.GetMethods(
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic))
            .Where(method => method.Name.Contains("mcp_only_in_dotnet", StringComparison.OrdinalIgnoreCase))
            .Select(method => $"{method.DeclaringType!.Name}.{method.Name}")
            .ToArray();
        Assert.False(
            leftover.Length > 0,
            "leftover mcp_only_in_dotnet: " + string.Join(", ", leftover));

        Plugin_mcp_files_are_empty_scaffolds();
    }

    /// <summary>
    /// Po 34: small-change spawns nobody; at most two fix rounds; gates are hard stops.
    /// Does not grow the squad skill with new sections.
    /// </summary>
    [Fact]
    public void Anti_loop_small_change_spawn_none_and_two_fix_rounds_max()
    {
        var skill = Fixture("SKILL.md");
        var command = Fixture("squad.md");
        var contract = Fixture("review-contract.md");
        var orchestrator = Fixture("squad-orchestrator.md");

        Assert.Contains("Small change: spawn none of these", skill, StringComparison.Ordinal);
        Assert.Contains("spawn nobody", skill, StringComparison.Ordinal);
        Assert.Contains(
            "Do not call the planner, implementer, verifier, orchestrator, or reviewers",
            command,
            StringComparison.Ordinal);

        Assert.Contains("≤2 fix rounds", skill, StringComparison.Ordinal);
        Assert.Contains("most two fix rounds", skill, StringComparison.Ordinal);
        Assert.Contains("at most two fix rounds", command, StringComparison.Ordinal);

        Assert.Contains("end the current", skill, StringComparison.Ordinal);
        Assert.Contains("Do not obtain another report, invoke merge again", skill, StringComparison.Ordinal);
        Assert.Contains("end the loop without retrying", command, StringComparison.Ordinal);
        Assert.Contains("Do not launch, retry, or hand work to another agent", orchestrator, StringComparison.Ordinal);
        Assert.Contains(
            "No plan write, verdict, retry, fix round, or agent handoff is allowed",
            contract,
            StringComparison.Ordinal);
        Assert.Contains("no plan/fix loop", skill, StringComparison.Ordinal);

        var headings = skill.Split('\n')
            .Where(line => line.StartsWith("## ", StringComparison.Ordinal))
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
        Assert.Equal(
        [
            "## Locked v1 flow",
            "## Route",
            "## Gated agents",
            "## Skills",
            "## Stacks",
            "## Plan",
            "## Implement, verify, review",
            "## Advisor-lite",
            "## Worktree"
        ], headings);

        var packCheck = File.ReadAllText(Path.Combine(SourceRoot(), "plugins", "pack-check", "README.md"));
        Assert.Contains("`/pack-check` is setup-only", packCheck, StringComparison.Ordinal);
        Assert.Contains("`/squad` already runs this check", packCheck, StringComparison.Ordinal);
    }

    /// <summary>
    /// Po 20 fail bar: simplifier is report-only (no edits), three axes in one
    /// agent, Matt-clear ops on every loop agent, exactly /squad+/squad-review, MCP
    /// authored MCP files are empty scaffolds.
    /// </summary>
    [Fact]
    public void Simplifier_is_report_only_one_agent_three_axes()
    {
        var simplifier = Fixture("squad-simplifier.md");
        Assert.Contains("readonly: true", simplifier, StringComparison.Ordinal);
        Assert.Contains("One agent, three axes", simplifier, StringComparison.Ordinal);
        Assert.Contains("reuse, quality, efficiency", simplifier, StringComparison.Ordinal);
        Assert.Contains("Report only", simplifier, StringComparison.Ordinal);
        Assert.Contains("Do not edit", simplifier, StringComparison.Ordinal);
        Assert.Contains("preserve behaviour", simplifier, StringComparison.Ordinal);
        Assert.Contains("Clarity > fewer lines", simplifier, StringComparison.Ordinal);
        Assert.Contains("over-simplify", simplifier, StringComparison.Ordinal);
        Assert.Contains("nested-clever", simplifier, StringComparison.Ordinal);
        Assert.Contains("Diff-scope only", simplifier, StringComparison.Ordinal);
        Assert.Contains("changed code", simplifier, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name", simplifier, StringComparison.Ordinal);
        Assert.Contains("CLAUDE.md", simplifier, StringComparison.Ordinal);
        Assert.Contains("Never treat CLAUDE.md as the stack standard", simplifier, StringComparison.Ordinal);
        Assert.Contains("2. Standards:", simplifier, StringComparison.Ordinal);
        Assert.Contains("`references/standards/`", simplifier, StringComparison.Ordinal);
        Assert.Contains("readable > clever", simplifier, StringComparison.Ordinal);
        Assert.Contains("Good:", simplifier, StringComparison.Ordinal);
        Assert.Contains("Bad:", simplifier, StringComparison.Ordinal);
        Assert.DoesNotContain("auto-edit", simplifier, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("loop-reuse", simplifier, StringComparison.Ordinal);
        Assert.DoesNotContain("loop-quality", simplifier, StringComparison.Ordinal);
        Assert.DoesNotContain("loop-efficiency", simplifier, StringComparison.Ordinal);
        Assert.DoesNotContain("- write", simplifier, StringComparison.Ordinal);
        Assert.DoesNotContain("- edit", simplifier, StringComparison.Ordinal);

        foreach (var agent in AgentNames)
            Assert.Contains("Standards:", Fixture($"{agent}.md"), StringComparison.Ordinal);

        Assert.Contains("Do not implement or verify", Fixture("squad-planner.md"), StringComparison.Ordinal);
        Assert.Contains("Good:", Fixture("squad-planner.md"), StringComparison.Ordinal);
        Assert.Contains("the code around the change", Fixture("squad-implementer.md"), StringComparison.Ordinal);
        Assert.Contains("Good:", Fixture("squad-implementer.md"), StringComparison.Ordinal);
        Assert.Contains("Do not edit source", Fixture("squad-verifier.md"), StringComparison.Ordinal);
        Assert.Contains("Good:", Fixture("squad-verifier.md"), StringComparison.Ordinal);
        Assert.Contains("Report only", Fixture("squad-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("Good:", Fixture("squad-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("Do not edit", Fixture("squad-security-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("Good:", Fixture("squad-security-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("never source code", Fixture("squad-orchestrator.md"), StringComparison.Ordinal);
        Assert.Contains("Good:", Fixture("squad-orchestrator.md"), StringComparison.Ordinal);

        Loop_agent_bodies_stay_tiny();
        Loop_agent_bodies_stay_above_the_thin_floor();
        Squad_commands_are_exactly_squad_and_review();
        Plugin_mcp_files_are_empty_scaffolds();
    }

    private static void Loop_agent_bodies_stay_above_the_thin_floor()
    {
        foreach (var agent in AgentNames)
        {
            var lines = BodyAfterFrontmatter(Fixture($"{agent}.md"))
                .Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
            var floor = agent == "squad-security-reviewer" ? 18 : 10;
            Assert.True(lines >= floor, $"{agent} body is {lines} lines; too thin (floor {floor}).");
        }
    }

    private void Plugin_mcp_files_are_empty_scaffolds()
    {
        var root = SourceRoot();
        var authored = Directory.GetFiles(root, "mcp.json", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["plugins/dotnet/mcp.json", "plugins/squad/mcp.json"], authored);
        Assert.Empty(Directory.GetFiles(Path.Combine(root, "plugins"), ".mcp.json",
            SearchOption.AllDirectories));

        foreach (var relative in authored)
        {
            var mcp = JsonNode.Parse(File.ReadAllText(Path.Combine(root, relative)))!;
            Assert.Empty(mcp["mcpServers"]!.AsObject());
        }
    }

    private static bool IsGeneratedPath(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    /// <summary>Po 24 fail bar: root README documents the Codex agent TOML copy one-liner.</summary>
    [Fact]
    public void Readme_has_codex_agent_toml_copy_one_liner()
    {
        var readme = File.ReadAllText(Path.Combine(SourceRoot(), "README.md"));
        Assert.Contains(
            "cp plugins/squad/com.openai.codex/agents/*.toml .codex/agents/",
            readme,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Po 46 fail bar: root README maps the three user slashes plus setup
    /// <c>/pack-check</c> near Using the plugins. Fails leftover
    /// "Two Squad entrypoints" / two-command wording and a missing row.
    /// </summary>
    [Fact]
    public void Readme_lists_three_user_commands_plus_pack_check()
    {
        var root = SourceRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var pluginReadme = File.ReadAllText(Path.Combine(root, "plugins", "squad", "README.md"));

        var usingPlugins = readme.IndexOf("## Using the plugins", StringComparison.Ordinal);
        Assert.True(usingPlugins >= 0, "root README is missing ## Using the plugins");

        var squadRow = "| `/squad` | Dev | build factory, code-only hand-off |";
        var map = readme.IndexOf(squadRow, StringComparison.Ordinal);
        Assert.True(map > usingPlugins, "command map must sit under Using the plugins");
        Assert.True(map - usingPlugins < 400, "command map must be the first thing under Using the plugins");

        Assert.Contains("| Command | Who | What |", readme, StringComparison.Ordinal);
        Assert.Contains(squadRow, readme, StringComparison.Ordinal);
        Assert.Contains("| `/squad-review` | Dev | code/diff report |", readme, StringComparison.Ordinal);
        Assert.Contains("| `/scenarios` | Office tester | `docs/smoke/*.md` only |", readme, StringComparison.Ordinal);
        Assert.Contains("| `/pack-check` | Setup | not a Squad flow |", readme, StringComparison.Ordinal);

        foreach (var text in new[] { readme, pluginReadme })
        {
            Assert.DoesNotContain("Two Squad entrypoints", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Two user-invoked entrypoints", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Two entrypoints", text, StringComparison.Ordinal);
            Assert.DoesNotContain("two commands only", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Po 25 fail bar: learnings-digest is a user-invoked Matt-tiny skill, not a third slash
    /// command, and it must not rewrite skills.
    /// </summary>
    [Fact]
    public void Learnings_digest_is_user_invoked_matt_tiny_no_rewrite()
    {
        var root = SourceRoot();
        var skillPath = Path.Combine(root, "plugins", "squad", "skills", "learnings-digest", "SKILL.md");
        Assert.True(File.Exists(skillPath));
        var skill = File.ReadAllText(skillPath);
        var body = BodyAfterFrontmatter(skill);
        var lines = body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));

        Assert.Contains("name: learnings-digest", skill, StringComparison.Ordinal);
        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.Contains("Do not rewrite skills", skill, StringComparison.Ordinal);
        Assert.Contains("append-only", skill, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("audience: loop", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("rewrite the skill", skill, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("edit SKILL.md", skill, StringComparison.Ordinal);
        Assert.Contains("Good:", skill, StringComparison.Ordinal);
        Assert.Contains("Bad:", skill, StringComparison.Ordinal);
        Assert.True(lines <= 16, $"learnings-digest body is {lines} lines; Matt-tiny cap is 16.");
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "learnings-digest.md")));
        Squad_commands_are_exactly_squad_and_review();
    }

    /// <summary>Po 26 fail bar: worktree lifecycle lives only in the /squad skill.</summary>
    [Fact]
    public void Worktree_note_only_in_squad_skill()
    {
        Worktree_cleanup_preserves_uncommitted_or_externally_owned_work();

        var root = SourceRoot();
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        Assert.Contains("## Worktree", skill, StringComparison.Ordinal);
        Assert.Contains("git worktree remove <exact-path>", skill, StringComparison.Ordinal);

        var leftovers = new List<string>();
        var surfaces = new List<string>
        {
            Path.Combine(root, "plugins", "squad", "commands", "squad.md"),
            Path.Combine(root, "plugins", "squad", "commands", "squad-review.md"),
            Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"),
            Path.Combine(root, "plugins", "squad", "README.md"),
            Path.Combine(root, "plugins", "squad", "skills", "learnings-digest", "SKILL.md")
        };
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "plugins", "squad", "agents"), "*.md"));

        foreach (var path in surfaces)
        {
            var text = File.ReadAllText(path);
            if (text.Contains("git worktree", StringComparison.Ordinal)
                || text.Contains("worktree lifecycle", StringComparison.OrdinalIgnoreCase)
                || text.Contains("## Worktree", StringComparison.Ordinal))
            {
                leftovers.Add(Path.GetRelativePath(root, path));
            }
        }

        Assert.False(
            leftovers.Count > 0,
            "worktree lifecycle leaked outside the squad skill: " + string.Join(", ", leftovers));
    }

    /// <summary>
    /// Po 27 fail bar: pin only caveman + caveman-compress at the locked SHA; /squad loads
    /// caveman by exact Skill name. No full catalog. Authored tree stays URL records only.
    /// </summary>
    [Fact]
    public void Caveman_external_pins_and_squad_invokes_by_exact_name()
    {
        const string sha = "5184b3d11ac6a1acb7d44b9bfaa31698157cff97";
        var root = SourceRoot();
        var manifest = JsonNode.Parse(File.ReadAllText(
            Path.Combine(root, "plugins", "squad", "external-skills.json")))!;
        var sources = manifest["sources"]!.AsArray();
        Assert.Equal(2, sources.Count);

        var byName = sources.OfType<JsonObject>().ToDictionary(
            entry => entry["name"]!.GetValue<string>(),
            StringComparer.Ordinal);
        Assert.Equal(["caveman", "caveman-compress"],
            byName.Keys.OrderBy(name => name, StringComparer.Ordinal));

        foreach (var (name, path) in new[]
                 {
                     ("caveman", "skills/caveman"),
                     ("caveman-compress", "skills/caveman-compress")
                 })
        {
            var entry = byName[name];
            Assert.Equal("https://github.com/JuliusBrussee/caveman", entry["repository"]!.GetValue<string>());
            Assert.Equal(path, entry["path"]!.GetValue<string>());
            Assert.Equal(sha, entry["commit"]!.GetValue<string>());
            Assert.Equal("MIT", entry["license"]!.GetValue<string>());
        }

        var skill = Fixture("SKILL.md");
        Assert.Contains("Skill tool by exact name `caveman`", skill, StringComparison.Ordinal);
        Assert.Contains("Never write `/caveman`", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("Load `/caveman`", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("grill-me", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("writing-for-agents", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("caveman-help", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("caveman-review", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("mattpocock", skill, StringComparison.OrdinalIgnoreCase);

        var catalog = File.ReadAllText(Path.Combine(root, "plugins", "squad", "external-skills.json"));
        Assert.DoesNotContain("grill-me", catalog, StringComparison.Ordinal);
        Assert.DoesNotContain("writing-for-agents", catalog, StringComparison.Ordinal);
        Assert.DoesNotContain("grilling", catalog, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "caveman")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "caveman-compress")));
    }

    /// <summary>
    /// Po 39 fail bar: slash picker blurbs stay short. Names stay /squad +
    /// /squad-review + sibling /scenarios. Fails a fourth command or a long
    /// essay description.
    /// </summary>
    [Fact]
    public void Squad_and_squad_review_command_blurbs_are_short_user_friendly()
    {
        var squad = Fixture("squad.md");
        var review = Fixture("squad-review.md");

        Assert.Equal(
            "Plan → build → verify → review (gated). Uncommitted hand-off.",
            FrontmatterDescription(squad));
        Assert.Equal(
            "Report-only review of a PR / uncommitted / vs main.",
            FrontmatterDescription(review));
        Assert.Equal(
            "From changed code, write docs/smoke/<slug>.md. No product-code edits.",
            FrontmatterDescription(File.ReadAllText(
                Path.Combine(SourceRoot(), "plugins", "squad", "commands", "scenarios.md"))));

        Squad_commands_are_exactly_squad_and_review();
    }

    /// <summary>
    /// Po 39 fail bar: learnings-digest is Skill-tool only, like caveman.
    /// <c>user-invocable: false</c>. Exact name <c>learnings-digest</c> still loads.
    /// Fails a third slash or a removed skill.
    /// </summary>
    [Fact]
    public void Learnings_digest_is_not_user_invocable()
    {
        var root = SourceRoot();
        var skillPath = Path.Combine(root, "plugins", "squad", "skills", "learnings-digest", "SKILL.md");
        Assert.True(File.Exists(skillPath), "learnings-digest skill must stay; do not remove it.");
        var skill = File.ReadAllText(skillPath);

        Assert.Contains("name: learnings-digest", skill, StringComparison.Ordinal);
        Assert.Contains("user-invocable: false", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("user-invocable: true", skill, StringComparison.Ordinal);
        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("disable-model-invocation: false", skill, StringComparison.Ordinal);

        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "learnings-digest.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "digest.md")));
        Squad_commands_are_exactly_squad_and_review();
    }

    /// <summary>
    /// Po 38 fail bar: Claude discovers one <c>/squad</c> and one <c>/squad-review</c>.
    /// Root <c>commands/</c> stays for Cursor; <c>strict: true</c> keeps it from pairing
    /// with <c>com.anthropic.claude-code/commands/</c>. Fails if both trees are still
    /// discoverable twins.
    /// </summary>
    [Fact]
    public void Claude_package_ships_one_squad_and_one_squad_review_command()
    {
        var sourceCommands = Path.Combine(SourceRoot(), "plugins", "squad", "commands");
        using var repo = new TestRepository().WithPlugin(
            "squad",
            File.ReadAllText(Path.Combine(SourceRoot(), "plugins", "squad", "plugin.json")));

        foreach (var path in Directory.GetFiles(sourceCommands, "*.md"))
        {
            repo.WithFile(
                $"plugins/squad/commands/{Path.GetFileName(path)}",
                File.ReadAllText(path));
        }

        repo.WithSkill(
            "squad",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var entry = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .Single(plugin => plugin["name"]!.GetValue<string>() == "squad");

        Assert.True(entry["strict"]!.GetValue<bool>());
        Assert.Null(entry["version"]);
        Assert.Equal("./com.anthropic.claude-code/commands/", entry["commands"]![0]!.GetValue<string>());

        var pluginDirectory = repo.PluginDirectory("squad");
        var names = DiscoverableClaudeCommandNames(pluginDirectory, entry);
        Assert.Equal(["scenarios", "squad", "squad-review"], names.OrderBy(name => name, StringComparer.Ordinal));

        var rootNames = CommandNames(Path.Combine(pluginDirectory, "commands"));
        var claudeNames = CommandNames(
            Path.Combine(pluginDirectory, "com.anthropic.claude-code", "commands"));
        Assert.Equal(rootNames, claudeNames);
        Assert.Equal(claudeNames.Count, names.Count);

        var twins = (JsonObject)entry.DeepClone();
        twins["strict"] = false;
        var loose = DiscoverableClaudeCommandNames(pluginDirectory, twins);
        Assert.True(
            loose.Count > names.Count,
            "root commands/ and com.anthropic.claude-code/commands/ must still both exist; strict is what hides the Cursor twin.");
        Assert.Equal(2, loose.Count(name => name == "squad"));
    }

    /// <summary>
    /// Po 41 fail bar: Copilot hides <c>/plugin:command</c> when the names match.
    /// Squad's Copilot factory rematerializes as <c>run</c> so the picker is
    /// <c>/squad:run</c>. Authored, Claude, and Cursor keep <c>squad</c>.
    /// Sibling <c>scenarios</c> stays that name. pack-check stays
    /// <c>pack-check</c>.
    /// </summary>
    [Fact]
    public void Copilot_factory_command_name_differs_from_plugin_name()
    {
        var root = SourceRoot();
        using var repo = new TestRepository().WithPlugin(
            "squad",
            File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")));

        foreach (var path in Directory.GetFiles(Path.Combine(root, "plugins", "squad", "commands"), "*.md"))
        {
            repo.WithFile(
                $"plugins/squad/commands/{Path.GetFileName(path)}",
                File.ReadAllText(path));
        }

        repo.WithSkill(
            "squad",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");

        repo.WithPlugin(
            "pack-check",
            File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "plugin.json")));
        repo.WithFile(
            "plugins/pack-check/commands/pack-check.md",
            File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "commands", "pack-check.md")));
        repo.WithSkill("pack-check", plugin: "pack-check");

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var pluginName = JsonNode.Parse(
            File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")))!
            ["name"]!.GetValue<string>();
        Assert.Equal("squad", pluginName);

        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/run.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/squad-review.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/squad.md"));

        var factory = run.File("plugins/squad/com.github.copilot/commands/run.md").Text;
        Assert.Contains("name: \"run\"", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", factory, StringComparison.Ordinal);
        Assert.NotEqual("run", pluginName);

        var copilotNames = CommandNames(
            Path.Combine(repo.PluginDirectory("squad"), "com.github.copilot", "commands"));
        Assert.Equal(
            new HashSet<string>(["run", "scenarios", "squad-review"], StringComparer.Ordinal),
            copilotNames);

        var claudeEntry = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .Single(plugin => plugin["name"]!.GetValue<string>() == "squad");
        var claudeNames = DiscoverableClaudeCommandNames(repo.PluginDirectory("squad"), claudeEntry);
        Assert.Equal(["scenarios", "squad", "squad-review"], claudeNames.OrderBy(name => name, StringComparer.Ordinal));
        Assert.DoesNotContain("run", claudeNames);
        Assert.Equal(1, claudeNames.Count(name => name == "squad"));

        var rootNames = CommandNames(Path.Combine(repo.PluginDirectory("squad"), "commands"));
        Assert.Equal(
            new HashSet<string>(["scenarios", "squad", "squad-review"], StringComparer.Ordinal),
            rootNames);

        Assert.True(run.HasFile("plugins/pack-check/com.github.copilot/commands/pack-check.md"));
        Assert.False(run.HasFile("plugins/pack-check/com.github.copilot/commands/run.md"));

        Squad_commands_are_exactly_squad_and_review();
    }

    /// <summary>
    /// Po 38 fail bar: caveman and caveman-compress pins are Skill-tool only. Publication
    /// writes <c>user-invocable: false</c>. Authored tree stays URL records; no vendored
    /// caveman catalog.
    /// </summary>
    [Fact]
    public void Caveman_pins_are_not_user_invocable()
    {
        var root = SourceRoot();
        var catalog = File.ReadAllText(Path.Combine(root, "plugins", "squad", "external-skills.json"));
        var names = JsonNode.Parse(catalog)!["sources"]!.AsArray()
            .OfType<JsonObject>()
            .Select(entry => entry["name"]!.GetValue<string>())
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["caveman", "caveman-compress"], names);
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "caveman")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "caveman-compress")));
        Assert.DoesNotContain("grill-me", catalog, StringComparison.Ordinal);
        Assert.DoesNotContain("caveman-help", catalog, StringComparison.Ordinal);

        foreach (var name in names)
        {
            var fetched = Path.Combine(Path.GetTempPath(), "agentpacks-caveman-fetched", Guid.NewGuid().ToString("N"));
            var target = Path.Combine(Path.GetTempPath(), "agentpacks-caveman-target", Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(fetched);
                var extra = name == "caveman-compress" ? "user-invocable: true\n" : string.Empty;
                File.WriteAllText(
                    Path.Combine(fetched, "SKILL.md"),
                    $"---\nname: {name}\ndescription: Compressed communication.\n{extra}---\n\nStand-in pin.\n");

                ExternalSourceMaterializer.InstallFetchedSkill(
                    fetched,
                    target,
                    new ExternalSourceEntry
                    {
                        Name = name,
                        Repository = "https://github.com/JuliusBrussee/caveman",
                        Path = $"skills/{name}",
                        Commit = "5184b3d11ac6a1acb7d44b9bfaa31698157cff97",
                        License = "MIT",
                        PluginDirectory = Path.Combine(root, "plugins", "squad")
                    });

                var skill = File.ReadAllText(Path.Combine(target, "SKILL.md"));
                Assert.Contains("user-invocable: false", skill, StringComparison.Ordinal);
                Assert.DoesNotContain("user-invocable: true", skill, StringComparison.Ordinal);
                Assert.DoesNotContain("grill-me", skill, StringComparison.Ordinal);
            }
            finally
            {
                if (Directory.Exists(fetched)) Directory.Delete(fetched, recursive: true);
                if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
            }
        }
    }

    /// <summary>
    /// Po 38 fail bar: user-facing surfaces say <c>/squad-review</c>, not bare
    /// <c>/review</c> or <c>code-review</c>. Squad ships those two plus sibling
    /// <c>/scenarios</c>. Fails leftover <c>commands/review.md</c>.
    /// </summary>
    [Fact]
    public void User_facing_surfaces_say_squad_review_not_bare_review()
    {
        var root = SourceRoot();
        var commandDir = Path.Combine(root, "plugins", "squad", "commands");
        var commandNames = Directory.GetFiles(commandDir, "*.md")
            .Select(path => Path.GetFileName(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["scenarios.md", "squad-review.md", "squad.md"], commandNames);
        Assert.False(File.Exists(Path.Combine(commandDir, "review.md")));
        Assert.False(File.Exists(Path.Combine(commandDir, "code-review.md")));

        var command = File.ReadAllText(Path.Combine(commandDir, "squad-review.md"));
        Assert.Contains("name: squad-review", command, StringComparison.Ordinal);
        Assert.DoesNotContain("name: review\n", command, StringComparison.Ordinal);
        Assert.DoesNotContain("code-review", command, StringComparison.Ordinal);
        Assert.Contains("Save report as markdown?", command, StringComparison.Ordinal);

        var leftovers = new List<string>();
        var surfaces = new List<string>
        {
            Path.Combine(root, "README.md"),
            Path.Combine(root, "plugins", "squad", "README.md"),
            Path.Combine(root, "plugins", "squad", "plugin.json"),
            Path.Combine(root, "plugins", "squad", "skills", "squad", "SKILL.md"),
            Path.Combine(root, "plugins", "squad", "skills", "learnings-digest", "SKILL.md"),
            Path.Combine(root, "plugins", "squad", "skills", "squad", "references", "learnings.md"),
            Path.Combine(root, "docs", "ADD-SKILL.md"),
            Path.Combine(root, "docs", "PLAN.md"),
            Path.Combine(root, "docs", "CLAUDE-PRIVATE-REPO.md")
        };
        surfaces.AddRange(Directory.GetFiles(commandDir, "*.md"));
        surfaces.AddRange(Directory.GetFiles(Path.Combine(root, "plugins", "squad", "agents"), "*.md"));

        var bareReview = new System.Text.RegularExpressions.Regex(
            @"(?<![A-Za-z0-9-])/review(?![A-Za-z0-9-])");

        foreach (var path in surfaces.Distinct(StringComparer.Ordinal))
        {
            var text = File.ReadAllText(path);
            if (bareReview.IsMatch(text))
                leftovers.Add(Path.GetRelativePath(root, path));
        }

        Assert.False(
            leftovers.Count > 0,
            "leftover bare /review on user-facing surfaces: " + string.Join(", ", leftovers));

        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var skill = Fixture("SKILL.md");
        Assert.Contains("/squad-review", readme, StringComparison.Ordinal);
        Assert.Contains("/squad-review", skill, StringComparison.Ordinal);
        Assert.Contains("/squad-review", File.ReadAllText(
            Path.Combine(root, "plugins", "squad", "plugin.json")), StringComparison.Ordinal);
        Assert.DoesNotContain("/code-review", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("/code-review", skill, StringComparison.Ordinal);
        Squad_commands_are_exactly_squad_and_review();
    }

    /// <summary>
    /// Po 37 fail bar: squad skill is not a slash twin. Copilot <c>user-invocable: false</c>
    /// plus Claude <c>disable-model-invocation: true</c>. Slash entry is
    /// <c>commands/squad.md</c> only. Skill tool still loads by exact name <c>squad</c>.
    /// Fails if the skill twin still lists as slash or disable-model-invocation drops.
    /// </summary>
    [Fact]
    public void Squad_skill_is_not_user_invocable_command_is_the_only_slash()
    {
        var skill = Fixture("SKILL.md");
        var command = Fixture("squad.md");
        var review = Fixture("squad-review.md");

        Assert.Contains("user-invocable: false", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("user-invocable: true", skill, StringComparison.Ordinal);
        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("disable-model-invocation: false", skill, StringComparison.Ordinal);
        Assert.Contains("name: squad", skill, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name", skill, StringComparison.Ordinal);
        Assert.Contains("`squad`", skill, StringComparison.Ordinal);

        Assert.Contains("name: squad", command, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name `squad`", command, StringComparison.Ordinal);
        Squad_commands_are_exactly_squad_and_review();

        Assert.DoesNotContain("user-invocable", review, StringComparison.Ordinal);
        Assert.Contains("name: squad-review", review, StringComparison.Ordinal);
        Assert.Contains("Save report as markdown?", review, StringComparison.Ordinal);

        var root = SourceRoot();
        foreach (var relative in new[]
                 {
                     Path.Combine("plugins", "dotnet", "skills", "dotnet-build", "SKILL.md"),
                     Path.Combine("plugins", "rust", "skills", "rust-build", "SKILL.md"),
                     Path.Combine("plugins", "typescript", "skills", "typescript-build", "SKILL.md")
                 })
        {
            var text = File.ReadAllText(Path.Combine(root, relative));
            Assert.Contains("audience: loop", text, StringComparison.Ordinal);
            Assert.Contains("Internal. Do not run directly", text, StringComparison.Ordinal);
            Assert.DoesNotContain("disable-model-invocation: true", text, StringComparison.Ordinal);
        }

        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "squad-skill.md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "com.github.copilot", "skills", "squad")));
    }

    /// <summary>
    /// Po 36 fail bar: end of /squad-review asks once to save markdown. Yes writes
    /// docs/reviews/&lt;slug&gt;.md and still shows IDE/CLI. No stays IDE/CLI only.
    /// Fails if /squad-review starts a fix round, grills more than that one ask, or writes
    /// docs/decisions.md.
    /// </summary>
    [Fact]
    public void Review_asks_save_markdown_yes_writes_file_no_stays_ide_only()
    {
        var review = Fixture("squad-review.md");
        var contract = Fixture("review-contract.md");
        var skill = Fixture("SKILL.md");
        var squad = Fixture("squad.md");
        var reviewSurfaces = string.Join('\n', review, contract);

        Assert.Equal(1, CountToken(review, "Save report as markdown?"));
        Assert.Contains("Save report as markdown?", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("Save report as markdown?", squad, StringComparison.Ordinal);

        Assert.Contains("docs/reviews/<slug>.md", reviewSurfaces, StringComparison.Ordinal);
        Assert.Contains("IDE/CLI", review, StringComparison.Ordinal);
        Assert.Contains("IDE/CLI only", review, StringComparison.Ordinal);
        Assert.Contains("write no report file", review, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("still show the findings in the IDE/CLI", review, StringComparison.Ordinal);

        Assert.Contains("Do not assign a Squad verdict", review, StringComparison.Ordinal);
        Assert.Contains("Do not start a fix round", review, StringComparison.Ordinal);
        Assert.Contains("There is no `Verdict`", contract, StringComparison.Ordinal);
        Assert.Contains("no plan/fix loop", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("**Verdict:**", review, StringComparison.Ordinal);
        Assert.DoesNotContain("Invoke `squad-implementer`", review, StringComparison.Ordinal);
        Assert.DoesNotContain("fresh implementer", review, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Never write `docs/decisions.md`", review, StringComparison.Ordinal);
        Assert.Contains("(never", contract, StringComparison.Ordinal);
        Assert.Contains("`docs/decisions.md`", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("append `docs/decisions.md`", reviewSurfaces, StringComparison.OrdinalIgnoreCase);
        foreach (var text in new[] { review, contract })
        {
            for (var index = 0;
                 (index = text.IndexOf("docs/decisions.md", index, StringComparison.Ordinal)) >= 0;
                 index += "docs/decisions.md".Length)
            {
                var start = Math.Max(0, index - 24);
                var window = text[start..Math.Min(text.Length, index + "docs/decisions.md".Length)];
                Assert.True(
                    window.Contains("Never write", StringComparison.Ordinal)
                    || window.Contains("never", StringComparison.OrdinalIgnoreCase),
                    "docs/decisions.md mentioned without a prohibition: " + window);
            }
        }

        Assert.Equal(1, CountToken(review, "?"));
        Assert.DoesNotContain("Planning round", review, StringComparison.Ordinal);
        Assert.DoesNotContain("❓", review, StringComparison.Ordinal);
        Assert.DoesNotContain("Ask the whole frontier", review, StringComparison.Ordinal);
        Assert.DoesNotContain("Advisor-lite", review, StringComparison.Ordinal);
        Assert.Contains("Do not ask anything else", review, StringComparison.Ordinal);
        Assert.Contains("exactly one question", contract, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No other question, grill, verdict, or fix round", contract, StringComparison.Ordinal);

        var headings = skill.Split('\n')
            .Where(line => line.StartsWith("## ", StringComparison.Ordinal))
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
        Assert.Equal(
        [
            "## Locked v1 flow",
            "## Route",
            "## Gated agents",
            "## Skills",
            "## Stacks",
            "## Plan",
            "## Implement, verify, review",
            "## Advisor-lite",
            "## Worktree"
        ], headings);

        Squad_commands_are_exactly_squad_and_review();
    }

    /// <summary>Po 28 fail bar: review-contract drops findings below 80 confidence.</summary>
    [Fact]
    public void Review_contract_drops_findings_below_80_confidence()
    {
        var contract = Fixture("review-contract.md");
        Assert.Contains("Only report findings with confidence ≥ 80", contract, StringComparison.Ordinal);
        Assert.Contains("Drop low-confidence noise", contract, StringComparison.Ordinal);
        Assert.Contains("Do not emit a confidence column", contract, StringComparison.Ordinal);
        Assert.Contains("confidence ≥ 80", Fixture("squad-reviewer.md"), StringComparison.Ordinal);
    }

    /// <summary>
    /// Po 29 fail bar: /squad-only advisor-lite at plan-confirm, stuck, and handoff.
    /// Stronger models.source.json tier. No advisor slash command.
    /// </summary>
    [Fact]
    public void Squad_advisor_lite_at_confirm_stuck_handoff_no_slash()
    {
        var skill = Fixture("SKILL.md");
        var command = Fixture("squad.md");
        var review = Fixture("squad-review.md");
        var root = SourceRoot();

        Assert.Contains("## Advisor-lite", skill, StringComparison.Ordinal);
        Assert.Contains("plan-confirm", skill, StringComparison.Ordinal);
        Assert.Contains("stuck", skill, StringComparison.Ordinal);
        Assert.Contains("before handoff", skill, StringComparison.Ordinal);
        Assert.Contains("read-only", skill, StringComparison.Ordinal);
        Assert.Contains("models.source.json", skill, StringComparison.Ordinal);
        Assert.Contains("stronger", skill, StringComparison.Ordinal);
        Assert.Contains("`inherit` consults `standard`", skill, StringComparison.Ordinal);
        Assert.Contains("`/squad` only", skill, StringComparison.Ordinal);
        Assert.Contains("Advisor-lite", command, StringComparison.Ordinal);
        Assert.Contains("plan-confirm", command + skill, StringComparison.Ordinal);
        Assert.DoesNotContain("Advisor-lite", review, StringComparison.Ordinal);
        Assert.DoesNotContain("name: advisor", command + review, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "advisor.md")));
        Squad_commands_are_exactly_squad_and_review();
    }

    /// <summary>
    /// Po 30 fail bar: verifier names behavioral + edge coverage and fails happy-path-only.
    /// Reuses the existing executable happy-path gate.
    /// </summary>
    [Fact]
    public void Verifier_rejects_happy_path_only_coverage()
    {
        new VerificationEvidenceTests().Happy_path_only_agent_written_tests_are_rejected();

        var verifier = Fixture("squad-verifier.md");
        var contract = Fixture("review-contract.md");
        Assert.Contains("behavioral + edge coverage", verifier, StringComparison.Ordinal);
        Assert.Contains("Reject happy-path-only coverage", verifier, StringComparison.Ordinal);
        Assert.Contains("behavioral and edge cases named", contract, StringComparison.Ordinal);
        Assert.Contains("happy-path only", contract, StringComparison.Ordinal);
    }

    /// <summary>
    /// Po 32 fail bar: planning-contract requires a happy/edge/fail + unit vs integration
    /// matrix per business criterion. Implementer TDDs it; verifier proves coverage.
    /// </summary>
    [Fact]
    public void Planning_contract_requires_the_test_plan_matrix()
    {
        var contract = Fixture("planning-contract.md");
        Assert.Contains("## Test plan matrix", contract, StringComparison.Ordinal);
        Assert.Contains("| Criterion | Happy | Edge | Fail | Kind |", contract, StringComparison.Ordinal);
        Assert.Contains("unit or integration", contract, StringComparison.Ordinal);
        Assert.Contains("business criterion", contract, StringComparison.Ordinal);
        Assert.Contains("TDDs that matrix", contract, StringComparison.Ordinal);
        Assert.Contains("happy-path-only", contract, StringComparison.Ordinal);

        Assert.Contains("test-plan matrix", Fixture("squad-planner.md"), StringComparison.Ordinal);
        Assert.Contains("test-plan matrix", Fixture("squad-implementer.md"), StringComparison.Ordinal);
        Assert.Contains("TDD", Fixture("squad-implementer.md"), StringComparison.Ordinal);
        Assert.Contains("Happy-path-only", Fixture("squad-implementer.md"), StringComparison.Ordinal);
        Assert.Contains("Do not run the full suite", Fixture("squad-implementer.md"), StringComparison.Ordinal);
        Assert.Contains("test-plan matrix", Fixture("squad-verifier.md"), StringComparison.Ordinal);

        new VerificationEvidenceTests().Happy_path_only_agent_written_tests_are_rejected();
    }

    /// <summary>
    /// Reviewer named proof (item 43). Smoke-matrix template covers
    /// happy/edge/fail/auth/timeout/5xx. Planning-contract, verifier, and the
    /// reviewer checklist cite it. App-filled Bruno collections stay out.
    /// </summary>
    [Fact]
    public void Squad_smoke_matrix_template_covers_happy_edge_fail_auth_timeout_5xx()
    {
        var root = SourceRoot();
        var matrix = File.ReadAllText(Path.Combine(root, "plugins", "squad", "references", "smoke-matrix.md"));
        AssertMattTiny(matrix, "smoke-matrix.md");
        AssertNoSecretsOrFilledCollections(matrix, "smoke-matrix.md");
        foreach (var token in new[] { "Happy", "Edge", "Fail", "Auth", "Timeout", "5xx" })
            Assert.Contains(token, matrix, StringComparison.Ordinal);
        Assert.Contains("app repo", matrix, StringComparison.Ordinal);
        Assert.Contains("Kind `smoke`", matrix, StringComparison.Ordinal);

        var contract = Fixture("planning-contract.md");
        Assert.Contains("smoke-matrix", contract, StringComparison.Ordinal);
        Assert.Contains("../../../references/smoke-matrix.md", contract, StringComparison.Ordinal);
        Assert.Contains("happy/edge/fail/auth/timeout/5xx", contract, StringComparison.Ordinal);
        Assert.Contains("unit or integration", contract, StringComparison.Ordinal);

        var verifier = Fixture("squad-verifier.md");
        Assert.Contains("smoke-matrix.md", verifier, StringComparison.Ordinal);
        Assert.Contains("happy/edge/fail/auth/timeout/5xx", verifier, StringComparison.Ordinal);

        var checklist = File.ReadAllText(Path.Combine(root, "plugins", "squad", "rules", "review-checklist.mdc"));
        Assert.Contains("Report all edges", checklist, StringComparison.Ordinal);
        Assert.Contains("../references/smoke-matrix.md", checklist, StringComparison.Ordinal);
        Assert.Contains("happy/edge/fail/auth/timeout/5xx", checklist, StringComparison.Ordinal);

        Assert.Contains("smoke-matrix", Fixture("squad-planner.md"), StringComparison.Ordinal);
        Assert.Contains("plugins/*/references",
            File.ReadAllText(Path.Combine(root, ".github", "workflows", "publish-marketplace.yml")),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Reviewer named proof (item 43). Language <c>*-test-patterns</c> ship
    /// Matt-tiny examples that distinguish local integration from deployed smoke.
    /// URLs and secrets stay in the app repo. <c>standards.source.json</c> is
    /// untouched.
    /// </summary>
    [Fact]
    public void Lang_test_patterns_local_vs_deployed_smoke_examples()
    {
        var root = SourceRoot();
        foreach (var pack in new[] { "dotnet", "typescript", "rust" })
        {
            var skillDir = Path.Combine(root, "plugins", pack, "skills", $"{pack}-test-patterns");
            var smoke = File.ReadAllText(Path.Combine(skillDir, "references", "examples", "deployed-smoke.md"));
            AssertMattTiny(smoke, $"{pack} deployed-smoke.md");
            AssertNoSecretsOrFilledCollections(smoke, $"{pack} deployed-smoke.md");
            Assert.Contains("Local", smoke, StringComparison.Ordinal);
            Assert.Contains("deployed", smoke, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("app repo", smoke, StringComparison.Ordinal);
            Assert.Contains("SMOKE_BASE_URL", smoke, StringComparison.Ordinal);

            var skill = File.ReadAllText(Path.Combine(skillDir, "SKILL.md"));
            Assert.Contains("references/examples/deployed-smoke.md", skill, StringComparison.Ordinal);

            var examples = Path.Combine(skillDir, "references", "examples");
            Assert.True(
                File.Exists(Path.Combine(examples, "http-integration.md"))
                || File.Exists(Path.Combine(examples, "integration.md")),
                $"{pack} is missing a local integration example");

            var catalog = JsonNode.Parse(File.ReadAllText(
                Path.Combine(root, "plugins", pack, "standards.source.json")))!;
            var mapped = catalog["consumers"]![$"{pack}-test-patterns"]!.AsArray()
                .Select(value => value!.GetValue<string>())
                .ToArray();
            Assert.Equal(["testing"], mapped);
        }
    }

    /// <summary>
    /// Reviewer named proof (item 43). In-pack only: no new plugin, no new slash.
    /// <c>/squad-review</c> stays a code-diff report. Checklist may link the
    /// smoke-matrix path; it is not a command.
    /// </summary>
    [Fact]
    public void Smoke_matrix_not_user_slash_no_new_plugin()
    {
        var root = SourceRoot();
        Assert.Equal(
            ["dotnet", "git", "pack-check", "rust", "squad", "typescript"],
            Directory.GetDirectories(Path.Combine(root, "plugins"))
                .Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));

        Squad_commands_are_exactly_squad_and_review();
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "smoke.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "squad-smoke.md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "smoke")));
        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "rules", "review-checklist.mdc")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "review-checklist.md")));

        var review = Fixture("squad-review.md");
        Assert.Contains("Report-only", review, StringComparison.Ordinal);
        Assert.Contains("Do not assign a Squad verdict", review, StringComparison.Ordinal);
        Assert.DoesNotContain("Bruno", review, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Postman", review, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("smoke runner", review, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TST", review, StringComparison.Ordinal);

        foreach (var file in Directory.GetFiles(Path.Combine(root, "plugins"), "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(file) ?? file;
            Assert.DoesNotContain("bruno", name, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("postman", name, StringComparison.OrdinalIgnoreCase);
        }

        Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Reviewer fail bar (PR #16). Local Key Vault / secrets / unauthenticated
    /// protected resources: mention and ask to continue.
    /// </summary>
    [Fact]
    public void Http_scenarios_local_unauth_kv_ask_continue()
    {
        new HttpScenariosContractTests().Http_scenarios_local_unauth_kv_ask_continue();
    }

    /// <summary>
    /// Po lock. <c>/squad</c> owns local Key Vault / secret-store unauth:
    /// expected on local, ask continue, no invented tokens, no hard-fail
    /// without ask.
    /// </summary>
    [Fact]
    public void Squad_local_secrets_unauth_ask_continue()
    {
        var skill = Fixture("SKILL.md");
        Assert.Contains("Local-secrets rule", skill, StringComparison.Ordinal);
        Assert.Contains("Key Vault", skill, StringComparison.Ordinal);
        Assert.Contains("secret store", skill, StringComparison.Ordinal);
        Assert.Contains("often expected", skill, StringComparison.Ordinal);
        Assert.Contains("not a product fail", skill, StringComparison.Ordinal);
        Assert.Contains("ask: continue?", skill, StringComparison.Ordinal);
        Assert.Contains("workaround / mock / skip secret path", skill, StringComparison.Ordinal);
        Assert.Contains("Do not invent tokens", skill, StringComparison.Ordinal);
        Assert.Contains("Do not hard-fail the whole run without ask", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("sk-", skill, StringComparison.Ordinal);
    }

    private static void AssertMattTiny(string text, string label)
    {
        var lines = text.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
        Assert.True(lines <= 16, $"{label} is {lines} lines; Matt-tiny cap is 16.");
    }

    private static void AssertNoSecretsOrFilledCollections(string text, string label)
    {
        Assert.DoesNotContain("Bruno", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Postman", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sk-", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer ", text, StringComparison.Ordinal);
        Assert.DoesNotContain("password=", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("api_key", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("app repo", text, StringComparison.Ordinal);
        Assert.False(string.IsNullOrEmpty(label));
    }

    [Fact]
    public void Loop_agents_use_per_role_tiers_implementer_standard_others_fast()
    {
        Assert.Contains("model: standard", Fixture("squad-implementer.md"), StringComparison.Ordinal);
        foreach (var agent in AgentNames.Where(name => name != "squad-implementer"))
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
        var verifier = Fixture("squad-verifier.md");

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

    /// <summary>
    /// Claude loads declared command directories always, and root <c>commands/</c> only when
    /// the marketplace entry is not strict. That is the dual-tree bug this proof exists to catch.
    /// </summary>
    private static List<string> DiscoverableClaudeCommandNames(string pluginDirectory, JsonObject entry)
    {
        var directories = new List<string>();

        switch (entry["commands"])
        {
            case JsonArray declared:
                directories.AddRange(declared
                    .Select(node => node?.GetValue<string>())
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => Path.Combine(pluginDirectory, path!.TrimStart('.', '/', '\\'))));
                break;
            case JsonValue single:
                directories.Add(Path.Combine(pluginDirectory, single.GetValue<string>().TrimStart('.', '/', '\\')));
                break;
        }

        if (entry["strict"]?.GetValue<bool>() is not true)
        {
            directories.Add(Path.Combine(pluginDirectory, "commands"));
        }

        return directories
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.GetFiles(directory, "*.md"))
            .Select(path => Path.GetFileNameWithoutExtension(path)!)
            .ToList();
    }

    private static HashSet<string> CommandNames(string directory) =>
        Directory.Exists(directory)
            ? Directory.GetFiles(directory, "*.md")
                .Select(path => Path.GetFileNameWithoutExtension(path)!)
                .ToHashSet(StringComparer.Ordinal)
            : [];

    private static string FrontmatterDescription(string markdown)
    {
        var line = markdown.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .FirstOrDefault(candidate => candidate.StartsWith("description:", StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(line), "missing description:");
        return line!["description:".Length..].Trim();
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
