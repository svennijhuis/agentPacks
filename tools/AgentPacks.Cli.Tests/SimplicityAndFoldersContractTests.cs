using System.Text.RegularExpressions;

namespace AgentPacks.Cli.Tests;

/// <summary>Contract for simplest-first building, the read-only indexer, and the .NET folder standard.</summary>
public sealed class SimplicityAndFoldersContractTests
{
    private static string Squad(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "squad", name));

    private static string Source(params string[] parts) =>
        File.ReadAllText(Path.Combine([TestRepository.SourceRoot(), .. parts]));

    private static string Layers() => Source("plugins", "dotnet", "standards", "layers.md");

    [Fact]
    public void Implementer_writes_simplest_first_and_self_checks_before_handoff()
    {
        var implementer = Squad("squad-implementer.md");
        Assert.Contains("write the simplest version that meets the criterion, with the fewest new types, files and layers. Reuse before adding. Readable beats clever.", implementer, StringComparison.Ordinal);
        Assert.Contains("can this be less code or easier to read with the same behaviour and performance? If yes, rewrite it first.", implementer, StringComparison.Ordinal);
        Assert.Contains("call `squad-indexer` first", implementer, StringComparison.Ordinal);
    }

    [Fact]
    public void Simplifier_asks_the_same_question_and_shows_the_smaller_diff()
    {
        var simplifier = Squad("squad-simplifier.md");
        Assert.Contains("can this be less code or easier to read with the same behaviour and performance? A finding must show the smaller diff.", simplifier, StringComparison.Ordinal);
        Assert.Contains("Ceiling `medium`", simplifier, StringComparison.Ordinal);
        Assert.Contains("readable > clever", simplifier, StringComparison.Ordinal);
    }

    [Fact]
    public void Indexer_is_fast_and_read_only()
    {
        var text = Squad("squad-indexer.md");
        var indexer = TestRepository.ParseFrontmatter(text);
        Assert.Equal("squad-indexer", indexer.Scalar("name"));
        Assert.Equal("fast", indexer.Scalar("model"));
        Assert.Equal("true", indexer.Scalar("readonly"));
        foreach (var tool in new[] { "read", "grep", "glob" })
            Assert.Contains($"\n  - {tool}\n", text, StringComparison.Ordinal);
        foreach (var tool in new[] { "edit", "write", "bash", "shell" })
            Assert.DoesNotContain($"\n  - {tool}\n", text, StringComparison.Ordinal);
        Assert.Contains("Never return code bodies, opinions, fixes, or verdicts.", text, StringComparison.Ordinal);
        Assert.Contains("`squad-indexer`", Squad("squad-architect.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Only_the_indexer_uses_the_fast_tier()
    {
        var fast = TestRepository.SquadAgentNames
            .Where(name => TestRepository.ParseFrontmatter(Squad($"{name}.md")).Scalar("model") == "fast")
            .ToArray();
        Assert.Equal(["squad-indexer"], fast);
    }

    [Fact]
    public void Docs_name_the_indexer_as_the_only_fast_agent()
    {
        var skill = Squad("SKILL.md");
        var readme = Source("README.md");
        Assert.Contains("`squad-indexer` (`fast`, the only `fast` agent)", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("none uses `fast`", skill, StringComparison.Ordinal);
        Assert.Contains("`squad-indexer` on `fast` (the only `fast` agent)", readme, StringComparison.Ordinal);
        Assert.Contains("`squad-indexer` (`fast`, the only `fast` agent)", Source("plugins", "squad", "README.md"), StringComparison.Ordinal);
        Assert.DoesNotContain("none uses `fast`", Source("plugins", "squad", "README.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Layers_has_no_single_project_skip_and_folders_comes_first()
    {
        var layers = Layers();
        Assert.DoesNotMatch(new Regex("skip[^\\n]*single-project", RegexOptions.IgnoreCase), layers);
        var folders = layers.IndexOf("\n## Folders\n", StringComparison.Ordinal);
        var firstLayer = layers.IndexOf("\n## Dependency rule\n", StringComparison.Ordinal);
        Assert.True(folders > 0 && folders < firstLayer, "## Folders must come before the first layer section");
        Assert.Contains("`## Folders` applies to every project, single-project apps included.", layers, StringComparison.Ordinal);
        Assert.Contains("Single-project app: the same tree, with `Infrastructure/` as a folder inside the project.", layers, StringComparison.Ordinal);
        Assert.Contains("No `.cs` file in a project root except `Program.cs` and `GlobalUsings.cs`", layers, StringComparison.Ordinal);
        Assert.Contains("No global `Services/` or `Models/` dump folders. No `Helpers/` or `Utils/`.", layers, StringComparison.Ordinal);
        Assert.Contains("Create a folder only when a file needs it.", layers, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Prefer `init` over `set`.")]
    [InlineData("Use simple `record` types for DTOs and models.")]
    [InlineData("Use the smallest visibility: `private` or `internal` unless `public` is needed")]
    [InlineData("Seal classes where sensible.")]
    public void Csharp_style_rules_are_stated(string rule) =>
        Assert.Contains(rule, Source("plugins", "dotnet", "standards", "csharp.md"), StringComparison.Ordinal);

    [Theory]
    [InlineData("- `set` where `init` works;")]
    [InlineData("- `public` where `internal` works.")]
    [InlineData("a new `.cs` file in a project root other than `Program.cs` or `GlobalUsings.cs`")]
    public void Reviewer_checklist_flags_style_and_root_files(string rule) =>
        Assert.Contains(rule, Source("plugins", "dotnet", "skills", "dotnet-review", "references", "checklist.md"), StringComparison.Ordinal);

    [Fact]
    public void Reviewer_rule_flags_the_loose_root_file_in_the_fixture_diff()
    {
        var diff = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "dotnet-folders", "loose-root.diff"));
        var added = Regex.Matches(diff, @"^\+\+\+ b/(.+)$", RegexOptions.Multiline).Select(match => match.Groups[1].Value.Trim());
        Assert.Equal(["src/MyApp.Api/OrderService.cs"], added.Where(IsLooseRootFile).ToArray());
    }

    // Mirrors the checklist rule: the file sits directly under a project folder and is not an allowed root file.
    private static bool IsLooseRootFile(string path)
    {
        var parts = path.Split('/');
        var project = parts[^2];
        var file = parts[^1];
        if (!project.StartsWith("MyApp", StringComparison.Ordinal)) return false;
        if (file is "Program.cs" or "GlobalUsings.cs") return false;
        return !(file == "DependencyInjection.cs" && project.EndsWith(".Infrastructure", StringComparison.Ordinal));
    }
}
