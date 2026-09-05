namespace AgentPacks.Cli.Tests;

/// <summary>
/// Portable tiers map to real client ids. Cursor must never receive opus/sonnet/haiku, and
/// Copilot/Codex must emit a model field instead of dropping it.
/// </summary>
public sealed class ModelCatalogTests
{
    [Fact]
    public void Built_in_catalog_matches_the_authored_source_file()
    {
        var root = SourceRoot();
        var catalog = File.ReadAllText(Path.Combine(root, "models.source.json"));

        Assert.Contains("\"default\": \"inherit\"", catalog, StringComparison.Ordinal);
        Assert.Contains("\"composer-2\"", catalog, StringComparison.Ordinal);
        Assert.Contains("\"grok-4.5\"", catalog, StringComparison.Ordinal);
        Assert.Contains("\"claude-opus-5\"", catalog, StringComparison.Ordinal);
        Assert.DoesNotContain("\"cursor\": \"sonnet\"", catalog, StringComparison.Ordinal);
        Assert.DoesNotContain("\"cursor\": \"opus\"", catalog, StringComparison.Ordinal);
        Assert.DoesNotContain("\"cursor\": \"haiku\"", catalog, StringComparison.Ordinal);
    }

    [Fact]
    public void Claude_receives_the_mapped_tier_and_copilot_and_codex_emit_model()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "model: standard\nreadonly: true\ntools:\n  - read");

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);

        var claude = run.File("plugins/engineering/com.anthropic.claude-code/agents/reviewer.md").Text;
        var copilot = run.File("plugins/engineering/com.github.copilot/agents/reviewer.agent.md").Text;
        var codex = run.File("plugins/engineering/com.openai.codex/agents/reviewer.toml").Text;

        Assert.Contains("model: \"sonnet\"", claude, StringComparison.Ordinal);
        Assert.Contains("model: \"gpt-5\"", copilot, StringComparison.Ordinal);
        Assert.Contains("model = \"inherit\"", codex, StringComparison.Ordinal);

        var cursor = run.File("plugins/engineering/.cursor-plugin/agents/reviewer.md").Text;
        Assert.Contains("model: \"grok-4.5\"", cursor, StringComparison.Ordinal);
        Assert.DoesNotContain("model: \"sonnet\"", cursor, StringComparison.Ordinal);
    }

    [Fact]
    public void Codex_stays_inherit_even_when_the_authored_tier_is_frontier()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithAgent("writer", extraFrontmatter: "model: frontier\nreadonly: false\ntools:\n  - read\n  - write");

        var run = repo.ValidateAndGenerate();
        var toml = run.File("plugins/engineering/com.openai.codex/agents/writer.toml").Text;

        Assert.False(run.HasErrors, run.Text);
        Assert.Contains("model = \"inherit\"", toml, StringComparison.Ordinal);
        Assert.DoesNotContain("model = \"opus\"", toml, StringComparison.Ordinal);
    }

    [Fact]
    public void A_claude_only_alias_is_rejected_as_an_authored_model()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "model: sonnet\nreadonly: true\ntools:\n  - read");

        var run = repo.Validate();

        Assert.True(run.HasErrors, run.Text);
        Assert.Contains("not portable", run.Text, StringComparison.Ordinal);
        Assert.Contains("Claude ids", run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void User_invoked_skills_generate_the_codex_policy_dialect()
    {
        using var repo = new TestRepository()
            .WithPlugin()
            .WithSkill("squad", extraFrontmatter: "disable-model-invocation: true");

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        var yaml = run.File("plugins/engineering/skills/squad/agents/openai.yaml").Text;
        Assert.Contains("allow_implicit_invocation: false", yaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Copilot_and_codex_never_drop_the_model_field()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithAgent("reviewer", extraFrontmatter: "model: inherit\nreadonly: true\ntools:\n  - read");

        var run = repo.ValidateAndGenerate();
        var copilot = run.File("plugins/engineering/com.github.copilot/agents/reviewer.agent.md").Text;
        var codex = run.File("plugins/engineering/com.openai.codex/agents/reviewer.toml").Text;

        Assert.Contains("model:", copilot, StringComparison.Ordinal);
        Assert.Contains("model =", codex, StringComparison.Ordinal);
    }

    [Fact]
    public void Authored_squad_skill_is_user_invoked()
    {
        var skill = File.ReadAllText(Path.Combine(
            SourceRoot(), "plugins", "squad", "skills", "squad", "SKILL.md"));

        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
    }

    private static string SourceRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "plugins")) &&
                Directory.Exists(Path.Combine(directory.FullName, "tools", "AgentPacks.Cli")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the agentPacks source root.");
    }
}
