using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Inventory and generator contracts for the <c>pack-author</c> capability pack.
/// Prompt wording is reviewed; these bars are the ones a reviewer can fail.
/// </summary>
public sealed class PackAuthorContractTests
{
    private static string PackRoot() =>
        Path.Combine(TestRepository.SourceRoot(), "plugins", "pack-author");

    [Fact]
    public void Names_are_plugin_command_and_skill()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(PackRoot(), "plugin.json")))!;
        Assert.Equal("pack-author", manifest["name"]!.GetValue<string>());
        Assert.Equal("0.1.3", manifest["version"]!.GetValue<string>());
        Assert.DoesNotContain(
            "language-pack",
            manifest["keywords"]!.AsArray().Select(value => value!.GetValue<string>()));

        var command = ParseFrontmatter(File.ReadAllText(Path.Combine(PackRoot(), "commands", "pack-author.md")));
        var skill = ParseFrontmatter(File.ReadAllText(
            Path.Combine(PackRoot(), "skills", "pack-author-md", "SKILL.md")));

        var commandName = command.Scalar("name");
        var skillName = skill.Scalar("name");
        Assert.Equal("pack-author", commandName);
        Assert.Equal("pack-author-md", skillName);
        Assert.Equal("pack-author-md", Path.GetFileName(Path.Combine(PackRoot(), "skills", "pack-author-md")));
        Assert.False(skillName!.Contains('.', StringComparison.Ordinal));
        Assert.NotEqual(commandName, skillName);
        Assert.NotEqual("pack-author", skillName);
        Assert.False(File.Exists(Path.Combine(PackRoot(), "commands", "author.md")));
    }

    [Fact]
    public void Skill_is_a_user_invoked_router_for_three_modes()
    {
        var skillPath = Path.Combine(PackRoot(), "skills", "pack-author-md", "SKILL.md");
        var skill = ParseFrontmatter(File.ReadAllText(skillPath));
        var description = skill.Scalar("description") ?? string.Empty;

        Assert.Equal("true", skill.Scalar("disable-model-invocation"));
        Assert.Equal("false", skill.Scalar("user-invocable"));
        Assert.False(skill.Has("metadata"));
        Assert.StartsWith("When ", description, StringComparison.Ordinal);
        Assert.EndsWith(".", description, StringComparison.Ordinal);
        Assert.True(description.Length <= 120, $"description is {description.Length} characters.");

        var lines = skill.Body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
        Assert.True(lines <= 40, $"router is {lines} non-blank lines.");
        Assert.Contains("`make`", skill.Body, StringComparison.Ordinal);
        Assert.Contains("`review`", skill.Body, StringComparison.Ordinal);
        Assert.Contains("`change`", skill.Body, StringComparison.Ordinal);

        foreach (var href in new[]
                 {
                     "references/style.md",
                     "references/make.md",
                     "references/review.md",
                     "references/change.md"
                 })
        {
            Assert.Contains(href, skill.Body, StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(PackRoot(), "skills", "pack-author-md", href)));
        }
    }

    [Fact]
    public void Review_is_feedback_and_change_is_human_gated()
    {
        var references = Path.Combine(PackRoot(), "skills", "pack-author-md", "references");
        var style = File.ReadAllText(Path.Combine(references, "style.md"));
        var make = File.ReadAllText(Path.Combine(references, "make.md"));
        var review = File.ReadAllText(Path.Combine(references, "review.md"));
        var change = File.ReadAllText(Path.Combine(references, "change.md"));

        foreach (var principle in new[] { "Predictability", "Leading words", "Prune no-ops", "Progressive disclosure" })
            Assert.Contains(principle, style, StringComparison.Ordinal);

        Assert.Contains("skills/example-helper/", style, StringComparison.Ordinal);
        Assert.Contains("+-- SKILL.md", style, StringComparison.Ordinal);
        Assert.Contains("+-- references/", style, StringComparison.Ordinal);
        Assert.Contains("When ", style, StringComparison.Ordinal);
        Assert.Contains("40 non-blank", style, StringComparison.Ordinal);
        Assert.DoesNotContain("mattpocock", style, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("github.com/mattpocock", make, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("ADD-SKILL.md", make, StringComparison.Ordinal);
        Assert.Contains("ADD-AGENT.md", make, StringComparison.Ordinal);
        Assert.Contains("ADD-HOOK.md", make, StringComparison.Ordinal);
        Assert.Contains("ADD-RULE.md", make, StringComparison.Ordinal);
        Assert.Contains("ADD-CLIENT-EXTENSION.md", make, StringComparison.Ordinal);
        Assert.Contains("PLAN.md", make, StringComparison.Ordinal);
        Assert.Contains("validate-all --out", make, StringComparison.Ordinal);

        Assert.Contains("PASS", review, StringComparison.Ordinal);
        Assert.Contains("FAIL", review, StringComparison.Ordinal);
        Assert.Contains("Do not edit the target.", review, StringComparison.Ordinal);

        Assert.Contains("At most two rounds.", change, StringComparison.Ordinal);
        Assert.Contains("Do not auto-rewrite.", change, StringComparison.Ordinal);
        Assert.Contains("docs/suggestions.md", change, StringComparison.Ordinal);
        Assert.Contains("Do not start a third round.", change, StringComparison.Ordinal);
    }

    [Fact]
    public void V1_scaffolds_a_skill_and_an_agent_only()
    {
        Assert.False(File.Exists(Path.Combine(PackRoot(), "hooks.source.json")));
        Assert.False(Directory.Exists(Path.Combine(PackRoot(), "rules")));
        Assert.False(Directory.Exists(Path.Combine(PackRoot(), "scripts")));

        var commands = Directory.GetFiles(Path.Combine(PackRoot(), "commands"), "*.md")
            .Select(path => Path.GetFileName(path))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["pack-author.md"], commands);

        var agents = Directory.GetFiles(Path.Combine(PackRoot(), "agents"), "*.md")
            .Select(path => Path.GetFileName(path))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["pack-author-reviewer.md"], agents);

        var agentText = File.ReadAllText(Path.Combine(PackRoot(), "agents", "pack-author-reviewer.md"));
        var agent = ParseFrontmatter(agentText);
        Assert.Equal("fast", agent.Scalar("model"));
        Assert.Equal("true", agent.Scalar("readonly"));
        var lines = agent.Body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
        Assert.True(lines <= 32, $"reviewer body is {lines} lines.");
        Assert.DoesNotContain("\n  - write\n", agentText, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  - edit\n", agentText, StringComparison.Ordinal);
    }

    [Fact]
    public void Agents_and_command_generate_for_every_provider()
    {
        using var repo = new TestRepository()
            .WithPlugin("pack-author", File.ReadAllText(Path.Combine(PackRoot(), "plugin.json")));

        repo.WithFile(
            "plugins/pack-author/commands/pack-author.md",
            File.ReadAllText(Path.Combine(PackRoot(), "commands", "pack-author.md")));
        repo.WithFile(
            "plugins/pack-author/agents/pack-author-reviewer.md",
            File.ReadAllText(Path.Combine(PackRoot(), "agents", "pack-author-reviewer.md")));
        repo.WithRawSkill(
            "pack-author-md",
            File.ReadAllText(Path.Combine(PackRoot(), "skills", "pack-author-md", "SKILL.md")),
            plugin: "pack-author");

        foreach (var reference in Directory.GetFiles(
                     Path.Combine(PackRoot(), "skills", "pack-author-md", "references"), "*.md"))
        {
            repo.WithFile(
                $"plugins/pack-author/skills/pack-author-md/references/{Path.GetFileName(reference)}",
                File.ReadAllText(reference));
        }

        foreach (var doc in new[]
                 {
                     "ADD-SKILL.md",
                     "ADD-AGENT.md",
                     "ADD-HOOK.md",
                     "ADD-RULE.md",
                     "ADD-CLIENT-EXTENSION.md",
                     "PLAN.md"
                 })
        {
            repo.WithFile(
                $"docs/{doc}",
                File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "docs", doc)));
        }

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.False(string.IsNullOrWhiteSpace(
            run.File("plugins/pack-author/com.anthropic.claude-code/agents/pack-author-reviewer.md").Text));
        Assert.False(string.IsNullOrWhiteSpace(
            run.File("plugins/pack-author/com.openai.codex/agents/pack-author-reviewer.toml").Text));
        Assert.False(string.IsNullOrWhiteSpace(
            run.File("plugins/pack-author/com.github.copilot/agents/pack-author-reviewer.agent.md").Text));
        Assert.True(run.HasFile("plugins/pack-author/.cursor-plugin/agents/pack-author-reviewer.md"));

        Assert.True(run.HasFile("plugins/pack-author/com.anthropic.claude-code/commands/pack-author.md"));
        Assert.False(run.HasFile("plugins/pack-author/com.anthropic.claude-code/commands/author.md"));
        Assert.False(run.HasFile("plugins/pack-author/com.openai.codex/commands/author.md"));
        Assert.True(run.HasFile("plugins/pack-author/com.github.copilot/commands/author.md"));
        Assert.False(run.HasFile("plugins/pack-author/com.github.copilot/commands/pack-author.md"));
        Assert.Contains(
            "name: \"author\"",
            run.File("plugins/pack-author/com.github.copilot/commands/author.md").Text,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "name: \"pack-author\"",
            run.File("plugins/pack-author/com.github.copilot/commands/author.md").Text,
            StringComparison.Ordinal);
        Assert.Contains(
            "allow_implicit_invocation: false",
            run.File("plugins/pack-author/skills/pack-author-md/agents/openai.yaml").Text,
            StringComparison.Ordinal);

        Assert.Equal(
            "Pack Author",
            run.File("plugins/pack-author/.cursor-plugin/plugin.json").Content["displayName"]!.GetValue<string>());
    }

    [Fact]
    public void Catalog_registers_the_fifth_slash()
    {
        var root = TestRepository.SourceRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var plan = File.ReadAllText(Path.Combine(root, "docs", "PLAN.md"));
        var note = File.ReadAllText(Path.Combine(root, "docs", "PACK-AUTHOR.md"));

        Assert.Contains("/pack-author", readme, StringComparison.Ordinal);
        Assert.Contains("/pack-author:author", readme, StringComparison.Ordinal);
        Assert.Contains("fifth slash is intentional", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("/author", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("$author", readme, StringComparison.Ordinal);
        Assert.Contains("`pack-author`", plan, StringComparison.Ordinal);
        Assert.DoesNotContain("/author", plan, StringComparison.Ordinal);
        Assert.Contains("Lane A", note, StringComparison.Ordinal);
        Assert.Contains("/pack-author:author", note, StringComparison.Ordinal);
        Assert.Contains("$pack-author", note, StringComparison.Ordinal);
        Assert.DoesNotContain("/author", note, StringComparison.Ordinal);
        Assert.DoesNotContain("$author", note, StringComparison.Ordinal);

        var packReadme = File.ReadAllText(Path.Combine(root, "plugins", "pack-author", "README.md"));
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "pack-author", "skills", "pack-author-md", "SKILL.md"));
        var command = File.ReadAllText(Path.Combine(root, "plugins", "pack-author", "commands", "pack-author.md"));
        foreach (var text in new[] { packReadme, skill, command })
        {
            Assert.DoesNotContain("/author", text, StringComparison.Ordinal);
            Assert.DoesNotContain("$author", text, StringComparison.Ordinal);
        }

        var contribute = Path.Combine(root, "docs", "CONTRIBUTE.md");
        if (File.Exists(contribute))
        {
            var text = File.ReadAllText(contribute);
            Assert.Contains("/pack-author", text, StringComparison.Ordinal);
            Assert.DoesNotContain("No new slash.", text, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains("CONTRIBUTE.md", note, StringComparison.Ordinal);
            Assert.Contains("#63", note, StringComparison.Ordinal);
        }
    }

    private static Frontmatter ParseFrontmatter(string text)
    {
        var parsed = Frontmatter.TryParse(text, out var error);
        Assert.True(parsed is not null, error);
        return parsed!;
    }
}
