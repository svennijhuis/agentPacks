namespace AgentPacks.Cli.Tests;

/// <summary>
/// Reviewer named proof. Named keep/delete list for weekly redundancy:
/// KEEP copies that subagents and separately installed packs need in empty context;
/// DELETE compiler/test clones and same-file restatements a single reader already has.
/// </summary>
public sealed class RedundancyContractTests
{
    /// <summary>
    /// One <c>SourceRoot</c> implementation, shared JSON copy, and agent markdown emit.
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
    }
}
