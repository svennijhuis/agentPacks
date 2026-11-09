using System.Text.Json.Nodes;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Review sequence: one subagent per step, skills on the agents, pins for the three upstream skills.
/// </summary>
public sealed class ReviewSequenceContractTests
{
    private static string Squad(params string[] parts) =>
        File.ReadAllText(Path.Combine([TestRepository.SourceRoot(), "plugins", "squad", .. parts]));

    private static string Repo(params string[] parts) =>
        File.ReadAllText(Path.Combine([TestRepository.SourceRoot(), .. parts]));

    [Fact]
    public void Review_skills_are_pinned_and_unslop_is_not()
    {
        const string sha = "6b0300214a09fd6029cfb7077043125758cd1fd2";
        var root = TestRepository.SourceRoot();
        var catalog = File.ReadAllText(Path.Combine(root, "plugins", "squad", "external-skills.json"));
        Assert.DoesNotContain("unslop", catalog, StringComparison.Ordinal);

        var byName = JsonNode.Parse(catalog)!["sources"]!.AsArray()
            .OfType<JsonObject>()
            .ToDictionary(entry => entry["name"]!.GetValue<string>(), StringComparer.Ordinal);

        Assert.Equal(
            ["blast-radius", "caveman", "caveman-compress", "deslop", "statechart"],
            byName.Keys.OrderBy(name => name, StringComparer.Ordinal));

        foreach (var (name, path) in new[]
                 {
                     ("deslop", "cursor-team-kit/skills/deslop"),
                     ("blast-radius", "pstack/skills/blast-radius"),
                     ("statechart", "pstack/skills/statechart")
                 })
        {
            var entry = byName[name];
            Assert.Equal("https://github.com/cursor/plugins", entry["repository"]!.GetValue<string>());
            Assert.Equal(path, entry["path"]!.GetValue<string>());
            Assert.Equal(sha, entry["commit"]!.GetValue<string>());
            Assert.Equal("MIT", entry["license"]!.GetValue<string>());
            Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", name)));
        }
    }

    [Fact]
    public void Playbook_names_subagents_and_agents_load_the_skills()
    {
        var playbook = Squad("skills", "squad", "playbooks", "review.md");
        Assert.DoesNotContain("Load the `deslop` skill", playbook, StringComparison.Ordinal);
        Assert.DoesNotContain("Load the `blast-radius` skill", playbook, StringComparison.Ordinal);
        Assert.DoesNotContain("Load the `statechart` skill", playbook, StringComparison.Ordinal);
        Assert.DoesNotContain("unslop", playbook, StringComparison.Ordinal);

        var full = playbook.IndexOf("## Full loop", StringComparison.Ordinal);
        var report = playbook.IndexOf("## Report mode", StringComparison.Ordinal);
        Assert.True(full >= 0 && report > full);
        var fullLoop = playbook[full..report];
        var deslop = fullLoop.IndexOf("`squad-deslop`", StringComparison.Ordinal);
        var verifier = fullLoop.IndexOf("`squad-verifier`", StringComparison.Ordinal);
        var blast = fullLoop.IndexOf("`squad-blast-radius`", StringComparison.Ordinal);
        var chart = fullLoop.IndexOf("`squad-statechart`", StringComparison.Ordinal);
        var tester = fullLoop.IndexOf("`squad-tester`", StringComparison.Ordinal);
        Assert.True(deslop < verifier && verifier < blast && blast < chart && chart < tester, fullLoop);

        Assert.Contains("exact name `deslop`", Squad("agents", "squad-deslop.md"), StringComparison.Ordinal);
        Assert.Contains("exact name `blast-radius`", Squad("agents", "squad-blast-radius.md"), StringComparison.Ordinal);
        Assert.Contains("Do not load `unslop`, `why`, `how`, or `arena`.", Squad("agents", "squad-blast-radius.md"), StringComparison.Ordinal);
        Assert.Contains("exact name `statechart`", Squad("agents", "squad-statechart.md"), StringComparison.Ordinal);
        Assert.Contains("at most 5", Squad("agents", "squad-statechart.md"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("`Rules not checked`", Squad("agents", "squad-statechart.md"), StringComparison.Ordinal);
        Assert.Contains("`<lang>-test-patterns`", Squad("agents", "squad-tester.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Commands_follow_the_playbook_and_review_stays_read_only()
    {
        var squad = Squad("commands", "squad.md");
        var review = Squad("commands", "squad-review.md");
        const string order = "spawn `squad-deslop`, then `squad-verifier`, then `squad-blast-radius`, then `squad-statechart` only when `squad-blast-radius` reports `Shared state: yes`";
        Assert.Contains(order, squad, StringComparison.Ordinal);
        Assert.Contains("then `squad-tester`", squad, StringComparison.Ordinal);
        Assert.Contains("Do not load `deslop`, `blast-radius`, or `statechart` in this command.", squad, StringComparison.Ordinal);
        Assert.Contains("If the second fix review is still FAIL, stop and report the open fix list. Do not push.", squad, StringComparison.Ordinal);

        Assert.Contains("Do not spawn `squad-deslop` or `squad-tester`.", review, StringComparison.Ordinal);
        Assert.Contains("Spawn `squad-blast-radius` in report mode", review, StringComparison.Ordinal);
        Assert.Contains("When it reports `Shared state: yes`, spawn `squad-statechart`", review, StringComparison.Ordinal);
        Assert.DoesNotContain("Spawn `squad-deslop`", review, StringComparison.Ordinal);
        Assert.DoesNotContain("Spawn `squad-tester`", review, StringComparison.Ordinal);
    }

    [Fact]
    public void Reviewer_keeps_correctness_and_drops_the_blast_radius_bullet()
    {
        var reviewer = Squad("agents", "squad-reviewer.md");
        Assert.DoesNotContain("Blast radius:", reviewer, StringComparison.Ordinal);
        Assert.Contains("correctness and plan", reviewer, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not repeat a finding already in the blast-radius or statechart report", reviewer, StringComparison.Ordinal);
        Assert.Contains("Automation check from the review contract", reviewer, StringComparison.Ordinal);
        Assert.Contains("Skip a row whose Found by is `squad-tester`", Squad("agents", "squad-implementer.md"), StringComparison.Ordinal);
        Assert.Contains("**Gate:** PASS | FAIL", Squad("skills", "squad", "references", "review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("`Rules not checked` is a handoff note, not a fix row, and it does not fail the gate.", Squad("skills", "squad", "references", "review-contract.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Adr_0002_supersedes_the_tester_ban_and_shares_the_diagram()
    {
        var adr = Repo("docs", "adr", "0002-review-sequence.md");
        Assert.Contains("Status: accepted", adr, StringComparison.Ordinal);
        Assert.Contains("supersedes the locked-v1 rejection of a tester agent and of sequential review stages", adr, StringComparison.Ordinal);
        Assert.Equal(Mermaid(Squad("skills", "squad", "playbooks", "review.md")), Mermaid(adr));
    }

    private static string[] Mermaid(string text)
    {
        var blocks = new List<string>();
        var index = 0;
        while (true)
        {
            var start = text.IndexOf("```mermaid", index, StringComparison.Ordinal);
            if (start < 0)
            {
                return blocks.ToArray();
            }

            var end = text.IndexOf("\n```", start + 10, StringComparison.Ordinal);
            blocks.Add(text[start..end].Replace("\r", "", StringComparison.Ordinal));
            index = end + 4;
        }
    }
}
