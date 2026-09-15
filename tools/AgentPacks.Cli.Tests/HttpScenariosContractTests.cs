using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Command inventory and generator contracts for <c>/scenarios</c>. Prompt wording is reviewed,
/// not asserted.
/// </summary>
public sealed class HttpScenariosContractTests
{
    [Fact]
    public void User_commands_stay_squad_squad_review_pack_check_plus_http_scenarios()
    {
        var root = TestRepository.SourceRoot();
        var commands = Directory.GetFiles(Path.Combine(root, "plugins"), "*.md", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}commands{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetFileNameWithoutExtension(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["pack-check", "scenarios", "security-audit", "squad", "squad-review"], commands);
        Assert.Equal(
            ["dotnet", "git", "pack-check", "rust", "security", "squad", "typescript"],
            Directory.GetDirectories(Path.Combine(root, "plugins"))
                .Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));

        var skill = ParseFrontmatter(File.ReadAllText(ScenariosSkillPath(root)));
        Assert.Equal("scenarios-md", skill.Scalar("name"));
        Assert.Equal("true", skill.Scalar("disable-model-invocation"));
        Assert.Equal("false", skill.Scalar("user-invocable"));
        var metadata = skill.StringMap("metadata");
        Assert.True(metadata is null || !metadata.ContainsKey("audience"));

        using var repo = SquadCommandRepo(root);
        repo.WithPlugin(
            "pack-check",
            File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "plugin.json")));
        repo.WithFile(
            "plugins/pack-check/commands/pack-check.md",
            File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "commands", "pack-check.md")));
        repo.WithSkill("pack-check", plugin: "pack-check");

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.True(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/run.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/squad.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/run.md"));
        Assert.True(run.HasFile("plugins/pack-check/com.github.copilot/commands/pack-check.md"));

        var copilot = run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text;
        Assert.Contains("name: \"scenarios\"", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"http-scenarios\"", copilot, StringComparison.Ordinal);

        var factory = run.File("plugins/squad/com.github.copilot/commands/run.md").Text;
        Assert.Contains("name: \"run\"", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", factory, StringComparison.Ordinal);

        var claudeEntry = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .Single(plugin => plugin["name"]!.GetValue<string>() == "squad");
        Assert.True(claudeEntry["strict"]!.GetValue<bool>());
    }

    [Fact]
    public void User_slash_is_scenarios_not_http_scenarios()
    {
        var root = TestRepository.SourceRoot();
        var commandPath = Path.Combine(root, "plugins", "squad", "commands", "scenarios.md");
        var skillPath = ScenariosSkillPath(root);

        Assert.True(File.Exists(commandPath));
        Assert.True(File.Exists(skillPath));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "scenarios")));

        Assert.Equal("scenarios", ParseFrontmatter(File.ReadAllText(commandPath)).Scalar("name"));
        Assert.Equal("scenarios-md", ParseFrontmatter(File.ReadAllText(skillPath)).Scalar("name"));
        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "references", "smoke-matrix.md")));
    }

    [Fact]
    public void Copilot_ships_http_scenarios_command() => Copilot_ships_scenarios_command();

    [Fact]
    public void Copilot_ships_scenarios_command() => Copilot_http_scenarios_renamed_to_scenarios();

    [Fact]
    public void Copilot_http_scenarios_renamed_to_scenarios()
    {
        var root = TestRepository.SourceRoot();
        using var repo = SquadCommandRepo(root);
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/squad.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/run.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/squad-review.md"));
        Assert.False(File.Exists(Path.Combine(
            root, "plugins", "squad", "com.github.copilot", "commands", "http-scenarios.md")));
    }

    [Fact]
    public void All_trees_command_is_scenarios()
    {
        var root = TestRepository.SourceRoot();
        using var repo = SquadCommandRepo(root);
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md")));
        Assert.Equal(
            "scenarios",
            ParseFrontmatter(File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md")))
                .Scalar("name"));

        Assert.True(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/http-scenarios.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.Equal(
            "scenarios",
            ParseFrontmatter(run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text).Scalar("name"));
        Assert.False(run.HasFile("plugins/squad/com.openai.codex/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/.cursor-plugin/commands/http-scenarios.md"));

        var leftover = run.Generated
            .Where(file => file.RelativePath.Replace('\\', '/').Contains("/commands/", StringComparison.Ordinal)
                && file.RelativePath.Contains("http-scenarios", StringComparison.Ordinal))
            .Select(file => file.RelativePath)
            .ToArray();
        Assert.Empty(leftover);
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios")));
    }

    [Fact]
    public void Copilot_scenarios_command_name_differs_from_skill()
    {
        var root = TestRepository.SourceRoot();
        var command = ParseFrontmatter(File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md")));
        var skill = ParseFrontmatter(File.ReadAllText(ScenariosSkillPath(root)));

        Assert.Equal("scenarios", command.Scalar("name"));
        Assert.Equal("scenarios-md", skill.Scalar("name"));
        Assert.NotEqual(command.Scalar("name"), skill.Scalar("name"));
        Assert.True(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "scenarios-md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "scenarios")));
        Assert.Equal("false", skill.Scalar("user-invocable"));

        using var repo = SquadCommandRepo(root);
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var copilot = ParseFrontmatter(run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text);
        Assert.Equal("scenarios", copilot.Scalar("name"));
        Assert.NotEqual("scenarios-md", copilot.Scalar("name"));

        var factory = run.File("plugins/squad/com.github.copilot/commands/run.md").Text;
        Assert.Contains("name: \"run\"", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", factory, StringComparison.Ordinal);
    }

    [Fact]
    public void Slash_stays_scenarios_or_squad_scenarios()
    {
        User_slash_is_scenarios_not_http_scenarios();
        Copilot_http_scenarios_renamed_to_scenarios();
    }

    private static TestRepository SquadCommandRepo(string root)
    {
        var repo = new TestRepository()
            .WithPlugin(
                "squad",
                File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")))
            .WithCopiedDirectory(root, "plugins/squad/commands", "*.md");

        repo.WithSkill(
            "squad",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");
        repo.WithSkill(
            "scenarios-md",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");

        return repo;
    }

    private static string ScenariosSkillPath(string root) =>
        Path.Combine(root, "plugins", "squad", "skills", "scenarios-md", "SKILL.md");

    private static Frontmatter ParseFrontmatter(string text)
    {
        var parsed = Frontmatter.TryParse(text, out var error);
        Assert.True(parsed is not null, error);
        return parsed!;
    }
}
