using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Inventory and generator contracts for the <c>security</c> capability pack.
/// Prompt wording is reviewed, not asserted beyond a few fail bars.
/// </summary>
public sealed class SecurityPackContractTests
{
    private static readonly string[] AgentNames =
        ["security-hunter", "security-recon", "security-validator"];

    private static string PackRoot() =>
        Path.Combine(TestRepository.SourceRoot(), "plugins", "security");

    [Fact]
    public void Security_is_a_capability_pack_not_a_language_pack()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(PackRoot(), "plugin.json")))!;
        Assert.Equal("security", manifest["name"]!.GetValue<string>());
        Assert.Equal("0.1.3", manifest["version"]!.GetValue<string>());

        var keywords = manifest["keywords"]!.AsArray().Select(value => value!.GetValue<string>());
        Assert.Contains("security", keywords);
        Assert.DoesNotContain("language-pack", keywords, StringComparer.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(PackRoot(), "skills", "security-security-review")));
    }

    [Fact]
    public void Security_audit_is_user_invoked_slash_not_model_invoked()
    {
        var skill = ParseFrontmatter(File.ReadAllText(
            Path.Combine(PackRoot(), "skills", "security-audit-md", "SKILL.md")));
        Assert.Equal("security-audit-md", skill.Scalar("name"));
        Assert.Equal("true", skill.Scalar("disable-model-invocation"));
        Assert.Equal("false", skill.Scalar("user-invocable"));
        Assert.Equal("MIT", skill.Scalar("license"));
        Assert.False(skill.Has("metadata"));

        var command = ParseFrontmatter(File.ReadAllText(
            Path.Combine(PackRoot(), "commands", "security-audit.md")));
        Assert.Equal("security-audit", command.Scalar("name"));
        Assert.NotEqual(command.Scalar("name"), skill.Scalar("name"));
        Assert.True(Directory.Exists(Path.Combine(PackRoot(), "skills", "security-audit-md")));
        Assert.False(Directory.Exists(Path.Combine(PackRoot(), "skills", "security-audit")));
    }

    [Fact]
    public void Security_agents_are_fast_readonly_and_matt_tiny()
    {
        foreach (var agent in AgentNames)
        {
            var text = File.ReadAllText(Path.Combine(PackRoot(), "agents", $"{agent}.md"));
            var parsed = ParseFrontmatter(text);
            Assert.Equal(agent, parsed.Scalar("name"));
            Assert.Equal("fast", parsed.Scalar("model"));
            Assert.Equal("true", parsed.Scalar("readonly"));
            var lines = NonEmptyBodyLines(text);
            Assert.True(lines <= 32, $"{agent} body is {lines} lines; cap is 32.");
        }
    }

    [Fact]
    public void Security_agents_generate_for_every_client()
    {
        using var repo = new TestRepository()
            .WithPlugin("security", File.ReadAllText(Path.Combine(PackRoot(), "plugin.json")));

        foreach (var agent in AgentNames)
        {
            repo.WithFile(
                $"plugins/security/agents/{agent}.md",
                File.ReadAllText(Path.Combine(PackRoot(), "agents", $"{agent}.md")));
        }

        repo.WithFile(
            "plugins/security/commands/security-audit.md",
            File.ReadAllText(Path.Combine(PackRoot(), "commands", "security-audit.md")));
        repo.WithRawSkill(
            "security-audit-md",
            File.ReadAllText(Path.Combine(PackRoot(), "skills", "security-audit-md", "SKILL.md")),
            plugin: "security");

        foreach (var reference in Directory.GetFiles(
                     Path.Combine(PackRoot(), "skills", "security-audit-md", "references"), "*.md"))
        {
            repo.WithFile(
                $"plugins/security/skills/security-audit-md/references/{Path.GetFileName(reference)}",
                File.ReadAllText(reference));
        }

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        foreach (var agent in AgentNames)
        {
            Assert.False(string.IsNullOrWhiteSpace(
                run.File($"plugins/security/com.anthropic.claude-code/agents/{agent}.md").Text));
            Assert.False(string.IsNullOrWhiteSpace(
                run.File($"plugins/security/com.openai.codex/agents/{agent}.toml").Text));
            Assert.False(string.IsNullOrWhiteSpace(
                run.File($"plugins/security/com.github.copilot/agents/{agent}.agent.md").Text));
            Assert.True(run.HasFile($"plugins/security/.cursor-plugin/agents/{agent}.md"));
        }

        Assert.True(run.HasFile("plugins/security/com.anthropic.claude-code/commands/security-audit.md"));
        Assert.True(run.HasFile("plugins/security/com.github.copilot/commands/security-audit.md"));
    }

    [Fact]
    public void Audit_does_not_load_squad_and_gate_does_not_invoke_the_audit_slash()
    {
        var skill = File.ReadAllText(Path.Combine(PackRoot(), "skills", "security-audit-md", "SKILL.md"));
        Assert.DoesNotContain("`squad`", skill, StringComparison.Ordinal);
        Assert.Contains("Do not spawn squad reviewers.", skill, StringComparison.Ordinal);

        var gate = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(),
            "plugins", "squad", "agents", "squad-security-reviewer.md"));
        Assert.DoesNotContain("`/security-audit`", gate, StringComparison.Ordinal);
        Assert.Contains(
            "A defense-in-depth gap with no reachable attack is not a finding.",
            gate,
            StringComparison.Ordinal);
        Assert.DoesNotContain("A01_2025-Broken_Access_Control", gate, StringComparison.Ordinal);
        Assert.True(
            NonEmptyBodyLines(gate) <= 44,
            "squad-security-reviewer body exceeded its Matt-tiny cap.");
    }

    [Fact]
    public void Principles_require_adversarial_validation_and_a_reachable_attack()
    {
        var principles = File.ReadAllText(
            Path.Combine(PackRoot(), "skills", "security-audit-md", "references", "principles.md"));
        Assert.Contains("never the agent that found it", principles, StringComparison.Ordinal);
        Assert.Contains("not a finding", principles, StringComparison.Ordinal);
        Assert.Contains("needs_validation", principles, StringComparison.Ordinal);

        var notice = File.ReadAllText(Path.Combine(PackRoot(), "NOTICE.md"));
        Assert.Contains("cloudflare/security-audit-skill", notice, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MIT", notice, StringComparison.Ordinal);
    }

    private static Frontmatter ParseFrontmatter(string text)
    {
        var parsed = Frontmatter.TryParse(text, out var error);
        Assert.True(parsed is not null, error);
        return parsed!;
    }

    private static int NonEmptyBodyLines(string text) =>
        ParseFrontmatter(text).Body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
}
