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
        var implementer = Squad("squad-agent.md");
        Assert.Contains("write the simplest version that meets the criterion, with the fewest new types, files and layers. Reuse before adding. Readable beats clever.", implementer, StringComparison.Ordinal);
        Assert.Contains("can this be less code or easier to read with the same behaviour and performance? If yes, rewrite it first.", implementer, StringComparison.Ordinal);
        Assert.Contains("start from the `file pointers` `file:line` list the spawn passes", implementer, StringComparison.Ordinal);
        Assert.Contains("Do not spawn peers.", implementer, StringComparison.Ordinal);
        Assert.Contains("no new root `.cs` in the Api (host) or Infrastructure project except `Program.cs`, `GlobalUsings.cs`, and `DependencyInjection.cs` at the Infrastructure root", implementer, StringComparison.Ordinal);
        Assert.DoesNotContain("except the allowed ones", implementer, StringComparison.Ordinal);
    }

    [Fact]
    public void Simplifier_asks_the_same_question_and_shows_the_smaller_diff()
    {
        var simplifier = Squad("squad-agent.md");
        Assert.Contains("can this be less code or easier to read with the same behaviour and performance? A finding must show the smaller diff.", simplifier, StringComparison.Ordinal);
        Assert.Contains("Ceiling `medium`", simplifier, StringComparison.Ordinal);
        Assert.Contains("readable > clever", simplifier, StringComparison.Ordinal);
    }

    [Fact]
    public void Indexer_is_fast_and_read_only()
    {
        var text = Squad("squad-agent.md");
        Assert.Contains("- Spawn no `squad-*` agents. Start from the `file pointers` `file:line` list the spawn passes", text, StringComparison.Ordinal);
        Assert.Contains("start from the `file pointers` `file:line` list the spawn passes", text, StringComparison.Ordinal);
        var command = Squad("squad.md");
        Assert.Contains("the main agent spawns `file pointers` with the concrete question", command, StringComparison.Ordinal);
        Assert.Contains("spawn `file pointers` with that gap question", command, StringComparison.Ordinal);
        Assert.DoesNotContain("spawns `file pointers` for `file:line` locations", command, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex("spawns? `file pointers`(?! (with|first with))"), command);
        var contract = Source("plugins", "squad", "skills", "squad-mode", "references", "planning-contract.md");
        Assert.Contains("The main agent spawns `file pointers` with that gap question (the files, functions, types or callers it names), then reruns `architect role` once", contract, StringComparison.Ordinal);
        Assert.Contains("All four agents remain portable", Source("plugins", "squad", "README.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Only_the_indexer_uses_the_fast_tier()
    {
        var fast = TestRepository.SquadAgentNames
            .Where(name => TestRepository.ParseFrontmatter(Squad($"{name}.md")).Scalar("model") == "fast")
            .ToArray();
        Assert.Empty(fast);
    }

    [Fact]
    public void Docs_name_the_indexer_as_the_only_fast_agent()
    {
        var skill = Squad("SKILL.md");
        Assert.Contains("No role uses `fast`.", skill, StringComparison.Ordinal);
        Assert.Contains("No role uses `fast`.", Source("plugins", "squad", "README.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Layers_has_no_single_project_skip_and_folders_comes_first()
    {
        var layers = Layers();
        Assert.DoesNotMatch(new Regex("skip[^\\n]*single-project", RegexOptions.IgnoreCase), layers);
        var folders = layers.IndexOf("\n## Folders\n", StringComparison.Ordinal);
        var firstLayer = layers.IndexOf("\n## Dependency rule\n", StringComparison.Ordinal);
        Assert.True(folders > 0 && folders < firstLayer, "## Folders must come before the first layer section");
        Assert.Contains("`## Folders` applies to every app, single-project apps included.", layers, StringComparison.Ordinal);
        Assert.Contains("Single-project app: the same tree, with `Infrastructure/` as a folder inside the project.", layers, StringComparison.Ordinal);
        Assert.Contains("In the Api (host) project and the Infrastructure project or folder of this tree, no `.cs` file in\n  the root except `Program.cs` and `GlobalUsings.cs`", layers.ReplaceLineEndings("\n"), StringComparison.Ordinal);
        Assert.Contains("follow the layer sections below, as in the RestEase example.", layers, StringComparison.Ordinal);
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
    [InlineData("a new `.cs` file in the Api (host) or Infrastructure project root other than `Program.cs` or `GlobalUsings.cs`")]
    public void Reviewer_checklist_flags_style_and_root_files(string rule) =>
        Assert.Contains(rule, Source("plugins", "dotnet", "skills", "dotnet-review", "references", "checklist.md"), StringComparison.Ordinal);

    [Fact]
    public void Reviewer_rule_flags_the_loose_root_file_in_the_fixture_diff()
    {
        var diff = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "dotnet-folders", "loose-root.diff"));
        var added = Regex.Matches(diff, @"^\+\+\+ b/(.+)$", RegexOptions.Multiline).Select(match => match.Groups[1].Value.Trim());
        Assert.Equal(["src/MyApp.Api/OrderService.cs"], added.Where(IsLooseRootFile).ToArray());
        Assert.False(IsLooseRootFile("src/MyApp.Application/IWeatherService.cs"));
        Assert.False(IsLooseRootFile("src/MyApp.Application/Forecast.cs"));
        Assert.False(IsLooseRootFile("src/MyApp.Infrastructure.Weather/IWeatherApi.cs"));
        Assert.False(IsLooseRootFile("src/MyApp.Infrastructure/DependencyInjection.cs"));
        Assert.True(IsLooseRootFile("src/MyApp.Infrastructure/LooseRepo.cs"));
    }

    // Mirrors the checklist: only Api (host) and Infrastructure project roots, not Application / vendor-infra.
    private static bool IsLooseRootFile(string path)
    {
        var parts = path.Split('/');
        var project = parts[^2];
        var file = parts[^1];
        var isApi = project.EndsWith(".Api", StringComparison.Ordinal)
            || project.EndsWith(".Web", StringComparison.Ordinal)
            || project.EndsWith(".Host", StringComparison.Ordinal);
        var isInfra = project.EndsWith(".Infrastructure", StringComparison.Ordinal);
        if (!isApi && !isInfra) return false;
        if (file is "Program.cs" or "GlobalUsings.cs") return false;
        return !(file == "DependencyInjection.cs" && isInfra);
    }
}
