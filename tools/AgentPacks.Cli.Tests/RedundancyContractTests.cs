namespace AgentPacks.Cli.Tests;

/// <summary>
/// Reviewer named proof. Named keep/delete list for weekly redundancy:
/// KEEP copies that subagents and separately installed packs need in empty context;
/// DELETE compiler/test clones and same-file restatements a single reader already has.
/// </summary>
public sealed class RedundancyContractTests
{
    /// <summary>
    /// One <c>SourceRoot</c> implementation, shared JSON copy, agent markdown emit,
    /// marketplace hooks declaration, and catalog entry identity checks.
    /// </summary>
    [Fact]
    public void Compiler_and_same_file_copies_are_collapsed()
    {
        var root = TestRepository.SourceRoot();
        var tests = Path.Combine(root, "tools", "AgentPacks.Cli.Tests");
        Assert.Empty(ClonedHelpers(tests, "private static string " + "SourceRoot()"));
        Assert.Empty(ClonedHelpers(tests, "private static Frontmatter " + "ParseFrontmatter"));
        Assert.Empty(ClonedHelpers(tests, "private static int " + "NonEmptyBodyLines"));
        Assert.Empty(ClonedHelpers(tests, "private static int " + "CountToken"));
        Assert.Empty(ClonedHelpers(tests, "private static int " + "Occurrences("));

        var jsonFile = File.ReadAllText(Path.Combine(root, "tools", "AgentPacks.Cli", "Io", "JsonFile.cs"));
        Assert.Contains("public static void CopyProperties", jsonFile, StringComparison.Ordinal);

        var generator = File.ReadAllText(Path.Combine(
            root, "tools", "AgentPacks.Cli", "Generation", "ClientTreeGenerator.cs"));
        Assert.Contains("private void WriteAgentMarkdown", generator, StringComparison.Ordinal);
        Assert.DoesNotContain("foreach (var field in (string[])", generator, StringComparison.Ordinal);

        var hooks = File.ReadAllText(Path.Combine(
            root, "tools", "AgentPacks.Cli", "Generation", "HookGenerator.cs"));
        Assert.Contains("public static bool EmitsHooksFile", hooks, StringComparison.Ordinal);

        var claude = File.ReadAllText(Path.Combine(
            root, "tools", "AgentPacks.Cli", "Generation", "ClaudeCompatGenerator.cs"));
        var copilot = File.ReadAllText(Path.Combine(
            root, "tools", "AgentPacks.Cli", "Generation", "CopilotMarketplaceGenerator.cs"));
        Assert.DoesNotContain("Frontmatter?.Scalar(\"alwaysApply\")", claude, StringComparison.Ordinal);
        Assert.DoesNotContain("Frontmatter?.Scalar(\"alwaysApply\")", copilot, StringComparison.Ordinal);

        var compatibility = File.ReadAllText(Path.Combine(
            root, "tools", "AgentPacks.Cli", "Validation", "CompatibilityValidator.cs"));
        Assert.Contains("private string? ReadUniqueMarketplaceName", compatibility, StringComparison.Ordinal);
        Assert.Equal(1, TestRepository.CountOccurrences(compatibility, "collide after normalization"));
    }

    /// <summary>
    /// Same-file Confidence restatement and a thin <c>/scenarios</c> command (skill has the steps).
    /// </summary>
    [Fact]
    public void Same_reader_markdown_copies_are_collapsed()
    {
        var root = TestRepository.SourceRoot();
        var contract = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "review-contract.md"));
        var fieldRules = contract[..contract.IndexOf("## Mechanical vs judgment", StringComparison.Ordinal)];
        Assert.Contains("| Confidence | See Confidence below |", fieldRules, StringComparison.Ordinal);
        Assert.DoesNotContain("Score 0–100", fieldRules, StringComparison.Ordinal);
        Assert.DoesNotContain("≥ 80", fieldRules, StringComparison.Ordinal);
        Assert.Contains("## Confidence", contract, StringComparison.Ordinal);
        Assert.Contains("confidence ≥ 80", contract, StringComparison.Ordinal);

        var command = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "scenarios-md", "SKILL.md"));
        Assert.Contains("scenarios-md", command, StringComparison.Ordinal);
        Assert.DoesNotContain("OpenAPI", command, StringComparison.Ordinal);
        Assert.Contains("OpenAPI", skill, StringComparison.Ordinal);
        Assert.True(
            command.Length < skill.Length / 2,
            $"/scenarios command is {command.Length} chars; keep it thin like pack-check. Skill is {skill.Length}.");
    }

    private static string[] ClonedHelpers(string tests, string token) =>
        Directory.GetFiles(tests, "*.cs")
            .Where(path => !TestRepository.IsGeneratedPath(path))
            .Where(path => Path.GetFileName(path) != "TestRepository.cs")
            .Where(path => File.ReadAllText(path).Contains(token, StringComparison.Ordinal))
            .Select(path => Path.GetFileName(path))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
}
