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
        var command = Fixture("deliver.md");

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
        var command = Fixture("deliver.md");
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
        Assert.Contains("For `/review-diff`", Fixture("loop-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("With `/review-diff`", Fixture("loop-security-reviewer.md"), StringComparison.Ordinal);
        Assert.Contains("With `/review-diff`", Fixture("loop-simplifier.md"), StringComparison.Ordinal);
        Assert.Contains("## Standalone merge report", Fixture("review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("There is no `Verdict`", Fixture("review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("`round number: 1`", Fixture("review-diff.md"), StringComparison.Ordinal);
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
}
