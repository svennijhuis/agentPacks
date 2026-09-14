namespace AgentPacks.Cli.Tests;

/// <summary>
/// Reviewer named proof. Named keep/delete list for weekly redundancy:
/// KEEP copies that subagents and separately installed packs need in empty context;
/// DELETE compiler/test clones and same-file restatements a single reader already has.
/// </summary>
public sealed class RedundancyContractTests
{
    private static readonly string[] SquadAgents =
    [
        "squad-planner",
        "squad-implementer",
        "squad-verifier",
        "squad-reviewer",
        "squad-security-reviewer",
        "squad-simplifier",
        "squad-orchestrator"
    ];

    private static readonly string[] LanguagePacks = ["dotnet", "rust", "typescript"];

    private static readonly string[] SlotSuffixes = ["build", "test-patterns", "review"];

    /// <summary>
    /// Reviewer named proof. KEEP: skill-load preamble and CLAUDE.md on every squad agent;
    /// locked v1 in the skill and both READMEs; malformed re-ask in both commands, the skill,
    /// and the review contract; Internal slot line on every language-pack slot skill.
    /// </summary>
    [Fact]
    public void Flow_required_copies_stay_on_every_isolated_reader()
    {
        var root = TestRepository.SourceRoot();
        var agents = Path.Combine(root, "plugins", "squad", "agents");

        foreach (var agent in SquadAgents)
        {
            var text = File.ReadAllText(Path.Combine(agents, $"{agent}.md"));
            Assert.Contains(
                "Load the `squad` skill with the Skill tool by exact name `squad`",
                text,
                StringComparison.Ordinal);
            Assert.Contains("Never write `/squad` as prose to load it.", text, StringComparison.Ordinal);
            Assert.Contains("Never treat CLAUDE.md", text, StringComparison.Ordinal);
        }

        foreach (var relative in new[]
                 {
                     Path.Combine("plugins", "squad", "skills", "squad", "SKILL.md"),
                     "README.md",
                     Path.Combine("plugins", "squad", "README.md")
                 })
        {
            Assert.Contains(
                "## Locked v1 flow",
                File.ReadAllText(Path.Combine(root, relative)),
                StringComparison.Ordinal);
        }

        foreach (var relative in new[]
                 {
                     Path.Combine("plugins", "squad", "commands", "squad.md"),
                     Path.Combine("plugins", "squad", "commands", "squad-review.md"),
                     Path.Combine("plugins", "squad", "skills", "squad", "SKILL.md")
                 })
        {
            var text = File.ReadAllText(Path.Combine(root, relative));
            Assert.Contains("re-ask that producer **once**", text, StringComparison.Ordinal);
            Assert.Contains("accepted — malformed after re-ask", text, StringComparison.Ordinal);
        }

        Assert.Contains(
            "accepted — malformed after re-ask",
            File.ReadAllText(Path.Combine(
                root, "plugins", "squad", "skills", "squad", "references", "review-contract.md")),
            StringComparison.Ordinal);

        foreach (var pack in LanguagePacks)
        {
            foreach (var slot in SlotSuffixes)
            {
                var skill = File.ReadAllText(Path.Combine(
                    root, "plugins", pack, "skills", $"{pack}-{slot}", "SKILL.md"));
                Assert.Contains(
                    "Internal. Do not run directly — Squad loads by exact Skill name.",
                    skill,
                    StringComparison.Ordinal);
            }
        }

        var addAgent = File.ReadAllText(Path.Combine(root, "docs", "ADD-AGENT.md"));
        Assert.Contains("### Keep versus collapse", addAgent, StringComparison.Ordinal);
        Assert.Contains("Each agent starts with empty context", addAgent, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reviewer named proof. DELETE: one <c>SourceRoot</c> implementation, shared JSON copy
    /// and agent markdown emit, Confidence ≥ 80 stated once in the review contract.
    /// </summary>
    [Fact]
    public void Compiler_and_same_file_copies_are_collapsed()
    {
        var root = TestRepository.SourceRoot();
        var tests = Path.Combine(root, "tools", "AgentPacks.Cli.Tests");
        var clones = Directory.GetFiles(tests, "*.cs")
            .Where(path => !TestRepository.IsGeneratedPath(path))
            .Where(path => Path.GetFileName(path) != "TestRepository.cs")
            .Where(path => File.ReadAllText(path).Contains(
                "private static string " + "SourceRoot()", StringComparison.Ordinal))
            .Select(path => Path.GetFileName(path))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Empty(clones);

        var jsonFile = File.ReadAllText(Path.Combine(root, "tools", "AgentPacks.Cli", "Io", "JsonFile.cs"));
        Assert.Contains("public static void CopyProperties", jsonFile, StringComparison.Ordinal);

        var generator = File.ReadAllText(Path.Combine(
            root, "tools", "AgentPacks.Cli", "Generation", "ClientTreeGenerator.cs"));
        Assert.Contains("private void WriteAgentMarkdown", generator, StringComparison.Ordinal);
        Assert.DoesNotContain("foreach (var field in (string[])", generator, StringComparison.Ordinal);

        var contract = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "review-contract.md"));
        Assert.Equal(1, CountToken(contract, "Only report findings with confidence ≥ 80"));
        Assert.Contains("the bar is in Confidence below", contract, StringComparison.Ordinal);
    }

    private static int CountToken(string text, string token)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0; index += token.Length)
            count++;
        return count;
    }
}
