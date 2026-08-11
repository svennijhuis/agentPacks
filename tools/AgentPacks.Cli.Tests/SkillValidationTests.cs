using AgentPacks.Cli.Validation;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Agent Skills publishes no JSON Schema, so every rule here is hand-written and worth pinning.
/// </summary>
public class SkillValidationTests
{
    [Fact]
    public void Frontmatter_name_must_match_the_directory_name()
    {
        using var repo = new TestRepository().WithPlugin().WithSkill("dotnet-review", name: "review-dotnet");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("but the skill directory is"));
    }

    [Fact]
    public void A_dotted_name_is_legal_for_a_plugin_but_illegal_for_a_skill()
    {
        using var repo = new TestRepository().WithPlugin(manifest: """
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "acme.tools"
            }
            """).WithSkill("acme.tools");

        var run = repo.Validate();

        // The manifest name passes; the identically named skill does not.
        Assert.Contains(run.Diagnostics, d => d.Message.Contains("may not contain periods"));
        Assert.DoesNotContain(run.Diagnostics, d => d.Path.EndsWith("plugin.json"));
    }

    [Theory]
    [InlineData("has--double")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("Upper-Case")]
    public void Invalid_skill_names_are_rejected(string name)
    {
        using var repo = new TestRepository().WithPlugin().WithRawSkill(
            name,
            $"---\nname: {name}\ndescription: A valid description.\n---\n\nBody.\n");

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void Description_of_exactly_the_maximum_length_is_accepted()
    {
        using var repo = new TestRepository()
            .WithPlugin()
            .WithSkill("dotnet-review", description: new string('a', 1024));

        var run = repo.Validate();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void Description_one_character_over_the_maximum_is_rejected()
    {
        using var repo = new TestRepository()
            .WithPlugin()
            .WithSkill("dotnet-review", description: new string('a', 1025));

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("1025 characters"));
    }

    [Fact]
    public void Missing_description_is_reported()
    {
        using var repo = new TestRepository().WithPlugin().WithRawSkill(
            "dotnet-review",
            "---\nname: dotnet-review\n---\n\nBody.\n");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("non-empty 'description'"));
    }

    [Fact]
    public void Compatibility_over_five_hundred_characters_is_rejected()
    {
        using var repo = new TestRepository()
            .WithPlugin()
            .WithSkill("dotnet-review", extraFrontmatter: $"compatibility: {new string('a', 501)}");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("'compatibility' is 501 characters"));
    }

    [Fact]
    public void Metadata_must_map_strings_to_strings()
    {
        using var repo = new TestRepository().WithPlugin().WithRawSkill(
            "dotnet-review",
            """
            ---
            name: dotnet-review
            description: A valid description.
            metadata:
              nested:
                too: deep
            ---

            Body.
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("'metadata' must be a mapping"));
    }

    [Fact]
    public void Real_yaml_is_parsed_including_quoted_colons_multiline_and_comments()
    {
        using var repo = new TestRepository().WithPlugin().WithRawSkill(
            "dotnet-review",
            """
            ---
            # A comment the naive parser would have mangled.
            name: dotnet-review
            description: >-
              Reviews C# changes: correctness, async usage and testability.
              Use when reviewing a pull request.
            license: "Proprietary: see LICENSE.txt"
            metadata:
              author: platform-team
              version: "1.0"
            ---

            Body.
            """);

        var run = repo.Validate();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void A_directory_without_skill_md_is_company_policy_not_a_conformance_failure()
    {
        using var repo = new TestRepository().WithValidPlugin().WithEmptySkillDirectory("not-a-skill");

        var run = repo.Validate();

        var diagnostic = Assert.Single(run.Diagnostics);
        Assert.Equal(DiagnosticKind.CompanyPolicy, diagnostic.Kind);
    }

    [Fact]
    public void Duplicate_skill_names_are_rejected()
    {
        using var repo = new TestRepository()
            .WithPlugin()
            .WithSkill("testing")
            .WithRawSkill("testing-copy", "---\nname: testing\ndescription: Duplicate name.\n---\n\nBody.\n");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("already used by"));
    }

}
