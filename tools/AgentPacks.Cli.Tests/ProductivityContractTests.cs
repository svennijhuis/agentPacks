using System.Text.Json.Nodes;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Inventory for the productivity plugin: one slash, one adapted skill, six pins.
/// </summary>
public sealed class ProductivityContractTests
{
    private const string MattSha = "4ae2317ddb22b046e26455e4f9c9b3aaaf62212d";
    private const string CursorSha = "ae5650febf4320793844c12c4adaa937f417f8da";

    private static string PackRoot() => TestRepository.PluginRoot("productivity");

    [Fact]
    public void Productivity_ships_one_command_and_one_entrypoint_skill()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(PackRoot(), "plugin.json")))!;
        Assert.Equal("productivity", manifest["name"]!.GetValue<string>());
        Assert.Equal("0.1.5", manifest["version"]!.GetValue<string>());
        Assert.DoesNotContain("language-pack", manifest["keywords"]!.ToJsonString(), StringComparison.Ordinal);

        var command = TestRepository.ParseFrontmatter(
            File.ReadAllText(Path.Combine(PackRoot(), "commands", "tech-design.md")));
        var skill = TestRepository.ParseFrontmatter(
            File.ReadAllText(Path.Combine(PackRoot(), "skills", "tech-design", "SKILL.md")));

        Assert.Equal("tech-design", command.Scalar("name"));
        Assert.Equal("tech-design", skill.Scalar("name"));
        Assert.Equal("true", skill.Scalar("disable-model-invocation"));
        Assert.Equal("false", skill.Scalar("user-invocable"));
        Assert.StartsWith("When ", skill.Scalar("description"));
        Assert.Contains("tech-design", command.Body, StringComparison.Ordinal);

        var commands = Directory.GetFiles(Path.Combine(PackRoot(), "commands"), "*.md")
            .Select(path => Path.GetFileName(path))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["tech-design.md"], commands);
        Assert.False(Directory.Exists(Path.Combine(PackRoot(), "agents")));
    }

    [Fact]
    public void Wayfinder_copy_names_its_source_and_what_changed()
    {
        var skill = File.ReadAllText(Path.Combine(PackRoot(), "skills", "tech-design", "SKILL.md"));
        var notice = File.ReadAllText(Path.Combine(PackRoot(), "skills", "tech-design", "NOTICE.md"));

        Assert.Contains("skills/engineering/wayfinder", skill, StringComparison.Ordinal);
        Assert.Contains(MattSha, skill, StringComparison.Ordinal);
        Assert.Contains("What we changed:", skill, StringComparison.Ordinal);
        Assert.Contains("Advice alone never closes a gap.", skill, StringComparison.Ordinal);
        Assert.Contains("NOTICE.md", skill, StringComparison.Ordinal);
        Assert.Contains("MIT License", notice, StringComparison.Ordinal);
        Assert.Contains(MattSha, notice, StringComparison.Ordinal);
        Assert.Contains("docs/design/<name>/", skill, StringComparison.Ordinal);
    }

    [Fact]
    public void Six_model_invocable_pins_stay_url_records()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(
            Path.Combine(PackRoot(), "external-skills.json")))!;
        var sources = manifest["sources"]!.AsArray();
        Assert.Equal(6, sources.Count);

        var byName = sources.OfType<JsonObject>().ToDictionary(
            entry => entry["name"]!.GetValue<string>(),
            StringComparer.Ordinal);
        Assert.Equal(
            ["codebase-design", "deslop", "domain-modeling", "grilling", "research", "writing-for-agents"],
            byName.Keys.OrderBy(name => name, StringComparer.Ordinal));

        foreach (var (name, path) in new[]
                 {
                     ("domain-modeling", "skills/engineering/domain-modeling"),
                     ("codebase-design", "skills/engineering/codebase-design"),
                     ("research", "skills/engineering/research"),
                     ("grilling", "skills/productivity/grilling"),
                     ("writing-for-agents", "skills/productivity/writing-for-agents")
                 })
        {
            var entry = byName[name];
            Assert.Equal("https://github.com/mattpocock/skills", entry["repository"]!.GetValue<string>());
            Assert.Equal(path, entry["path"]!.GetValue<string>());
            Assert.Equal(MattSha, entry["commit"]!.GetValue<string>());
            Assert.Equal("MIT", entry["license"]!.GetValue<string>());
            Assert.False(Directory.Exists(Path.Combine(PackRoot(), "skills", name)));
        }

        var deslop = byName["deslop"];
        Assert.Equal("https://github.com/cursor/plugins", deslop["repository"]!.GetValue<string>());
        Assert.Equal("cursor-team-kit/skills/deslop", deslop["path"]!.GetValue<string>());
        Assert.Equal(CursorSha, deslop["commit"]!.GetValue<string>());
        Assert.Equal("MIT", deslop["license"]!.GetValue<string>());
        Assert.False(Directory.Exists(Path.Combine(PackRoot(), "skills", "deslop")));
    }

    [Fact]
    public void Readme_carries_the_loop_and_status_diagrams()
    {
        var readme = File.ReadAllText(Path.Combine(PackRoot(), "README.md"));
        Assert.Contains("## Start", readme, StringComparison.Ordinal);
        Assert.Contains("## Resume", readme, StringComparison.Ordinal);
        Assert.Contains("## Writing for humans and agents", readme, StringComparison.Ordinal);
        Assert.Contains("```mermaid", readme, StringComparison.Ordinal);
        Assert.Contains("flowchart TD", readme, StringComparison.Ordinal);
        Assert.Contains("stateDiagram-v2", readme, StringComparison.Ordinal);
        Assert.Contains("waiting-on-input", readme, StringComparison.Ordinal);
        Assert.Contains("Advice alone never closes a gap", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("\u2014", readme);
        Assert.DoesNotContain("\u2013", readme);
    }
}
