using AgentPacks.Cli.Generation;

namespace AgentPacks.Cli.Tests;

/// <summary>Slot skills authored as SKILL.source.md render through the shared template.</summary>
public class SlotSkillGenerationTests
{
    private static TestRepository Repository(string catalog) =>
        new TestRepository()
            .WithPlugin("dotnet", """
                {
                  "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
                  "name": "dotnet",
                  "description": "Test language pack.",
                  "keywords": ["language-pack"]
                }
                """)
            .WithFile("plugins/dotnet/standards/csharp.md", "# C#\n")
            .WithFile("plugins/dotnet/standards/testing.md", "# Testing\n")
            .WithStandards(catalog, "dotnet");

    [Fact]
    public void Slot_source_renders_contract_lines_and_catalog_order()
    {
        using var repo = Repository("""
            {
              "$schema": "../../schema/standards.schema.json",
              "version": 1,
              "documents": {
                "csharp": "standards/csharp.md",
                "testing": "standards/testing.md"
              },
              "consumers": {
                "dotnet-build": ["csharp", "testing"]
              }
            }
            """)
            .WithSlotSource("dotnet-build", title: ".NET build", commands: "shape and commands")
            .WithFile("plugins/dotnet/skills/dotnet-build/references/commands.md", "# Commands\n")
            .WithLoopSkill("dotnet-test-patterns");

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.True(run.HasFile("plugins/dotnet/skills/dotnet-build/SKILL.md"));

        var text = run.File("plugins/dotnet/skills/dotnet-build/SKILL.md").Text;
        Assert.Contains(LanguagePackContractTests.InternalDoNotRunLine, text, StringComparison.Ordinal);
        Assert.Contains("Loop-only; not as a user entrypoint.", text, StringComparison.Ordinal);
        Assert.Contains("audience: loop", text, StringComparison.Ordinal);
        Assert.Contains("user-invocable: false", text, StringComparison.Ordinal);
        Assert.Contains("Standards in force: `csharp.md`, `testing.md`.", text, StringComparison.Ordinal);
        Assert.Contains("during implement or review:", text, StringComparison.Ordinal);
        Assert.Contains("[shape and commands](references/commands.md)", text, StringComparison.Ordinal);
        Assert.Contains("Good: one fact.", text, StringComparison.Ordinal);
        Assert.False(GeneratedPaths.IsGenerated("skills/dotnet-build/SKILL.md"));
        Assert.True(GeneratedPaths.IsGenerated("skills/dotnet-build/SKILL.md", ["dotnet-build"]));
        Assert.False(GeneratedPaths.IsGenerated("skills/dotnet-build/SKILL.source.md", ["dotnet-build"]));
    }

    [Fact]
    public void Stale_skill_md_beside_a_source_is_ignored_and_regenerated()
    {
        using var repo = Repository("""
            {
              "$schema": "../../schema/standards.schema.json",
              "version": 1,
              "documents": {
                "csharp": "standards/csharp.md",
                "testing": "standards/testing.md"
              },
              "consumers": {
                "dotnet-review": ["testing", "csharp"]
              }
            }
            """)
            .WithSlotSource("dotnet-review", title: ".NET review")
            .WithFile("plugins/dotnet/skills/dotnet-review/references/checklist.md", "# Checklist\nProcess findings in this order.\n")
            .WithFile("plugins/dotnet/skills/dotnet-review/SKILL.md", "# stale\n")
            .WithLoopSkill("dotnet-build")
            .WithLoopSkill("dotnet-test-patterns");

        var first = repo.ValidateAndGenerate();
        Assert.False(first.HasErrors, first.Text);
        var rendered = first.File("plugins/dotnet/skills/dotnet-review/SKILL.md").Text;
        Assert.DoesNotContain("# stale", rendered, StringComparison.Ordinal);
        Assert.Contains("Standards in force: `testing.md`, `csharp.md`.", rendered, StringComparison.Ordinal);
        Assert.Contains("[the checklist](references/checklist.md)", rendered, StringComparison.Ordinal);

        var second = repo.ValidateAndGenerate();
        Assert.False(second.HasErrors, second.Text);
        Assert.Equal(rendered, second.File("plugins/dotnet/skills/dotnet-review/SKILL.md").Text);
    }

    [Fact]
    public void Slot_source_without_a_catalog_consumer_is_rejected()
    {
        using var repo = new TestRepository()
            .WithPlugin("dotnet", """
                {
                  "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
                  "name": "dotnet",
                  "description": "Test language pack.",
                  "keywords": ["language-pack"]
                }
                """)
            .WithSlotSource("dotnet-build", title: ".NET build")
            .WithLoopSkill("dotnet-test-patterns");

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("consumes no document", run.Text, StringComparison.Ordinal);
    }
}
