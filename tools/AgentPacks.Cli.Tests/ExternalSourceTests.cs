namespace AgentPacks.Cli.Tests;

using AgentPacks.Cli.Importing;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

/// <summary>
/// External skills remain URL records in authored source. Publication materializes them into the
/// portable plugin before the Claude marketplace is generated from that completed package.
/// </summary>
public class ExternalSourceTests
{
    private const string Sha = "84fdeffd12f2ee307994d1eb6feb48173b6e0502";

    private const string OneSource = $$"""
        {
          "sources": [
            {
              "name": "code-review",
              "description": "Two-axis review of a diff.",
              "repository": "https://github.com/mattpocock/skills",
              "path": "skills/engineering/code-review",
              "commit": "{{Sha}}",
              "license": "MIT"
            }
          ]
        }
        """;

    [Fact]
    public void External_urls_do_not_become_separate_Claude_plugins()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource);

        var plugins = repo.ValidateAndGenerate()
            .File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray();

        Assert.Single(plugins);
        Assert.Equal("engineering", plugins[0]!["name"]!.GetValue<string>());
    }

    [Fact]
    public void Local_generation_does_not_fetch_external_urls()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource);

        repo.ValidateAndGenerate();

        Assert.False(Directory.Exists(Path.Combine(repo.PluginDirectory(), "skills", "code-review")));
    }

    [Fact]
    public void Source_ownership_is_inferred_from_the_containing_plugin()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource);

        var run = repo.Validate();
        var source = ExternalSources.Load(run.Context).Single();

        Assert.Equal(repo.PluginDirectory(), source.PluginDirectory);
        Assert.EndsWith(
            "plugins/engineering/external-skills.json",
            source.SourceFile.Replace('\\', '/'));
    }

    [Fact]
    public void Generated_external_content_is_verified_offline()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource)
            .WithSkill("code-review");
        var target = Path.Combine(repo.PluginDirectory(), "skills", "code-review");
        var marker = new ExternalSourceMarker(
            "code-review",
            "https://github.com/mattpocock/skills",
            "skills/engineering/code-review",
            Sha,
            ExternalSourceMarker.HashDirectory(target));
        JsonFile.Write(Path.Combine(target, ExternalSourceMarker.FileName), marker.ToJson());

        Assert.Empty(repo.CheckExternalSources().Diagnostics);
    }

    [Fact]
    public void Materialization_never_overwrites_an_authored_skill()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource)
            .WithSkill("code-review");

        var run = repo.MaterializeExternalSources();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("over an authored skill"));
    }

    [Fact]
    public void A_source_pinned_to_a_branch_is_rejected()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources("""
            {
              "sources": [
                {
                  "name": "code-review",
                  "repository": "https://github.com/mattpocock/skills",
                  "path": "skills/engineering/code-review",
                  "commit": "main",
                  "license": "MIT"
                }
              ]
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("40-character Git commit SHA"));
    }

    [Fact]
    public void A_source_without_a_license_is_rejected()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources($$"""
            {
              "sources": [
                {
                  "name": "code-review",
                  "repository": "https://github.com/mattpocock/skills",
                  "path": "skills/engineering/code-review",
                  "commit": "{{Sha}}"
                }
              ]
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("'license'"));
    }

    [Fact]
    public void Import_targets_cannot_escape_the_plugin_or_use_local_urls()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources($$"""
            {
              "sources": [
                {
                  "name": "../escape",
                  "repository": "file:///tmp/skills",
                  "path": "../outside",
                  "commit": "{{Sha}}",
                  "license": "MIT"
                }
              ]
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("invalid skill name"));
        Assert.Contains(run.Diagnostics, d => d.Message.Contains("repository-relative path"));
        Assert.Contains(run.Diagnostics, d => d.Message.Contains("HTTPS repository URL"));
    }

    [Fact]
    public void A_skill_may_invoke_a_skill_that_is_referenced_externally()
    {
        using var repo = new TestRepository().WithPlugin().WithExternalSources(OneSource);

        repo.WithRawSkill(
            "review-flow",
            "---\nname: review-flow\ndescription: Runs the review.\n---\n\nRun a `/code-review` pass.\n");

        var run = repo.Validate();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void A_skill_invoking_something_nobody_ships_is_reported()
    {
        using var repo = new TestRepository().WithPlugin();

        repo.WithRawSkill(
            "grill-me",
            "---\nname: grill-me\ndescription: Sharpens a plan.\n---\n\nRun a `/grilling` session.\n");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("invokes `/grilling`"));
    }

    [Fact]
    public void Absolute_filesystem_paths_are_not_mistaken_for_skills()
    {
        using var repo = new TestRepository().WithPlugin();

        repo.WithRawSkill(
            "report",
            "---\nname: report\ndescription: Writes a report.\n---\n\nWrite the report to `/tmp` first.\n");

        var run = repo.Validate();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void A_broken_relative_link_in_an_authored_skill_is_reported()
    {
        using var repo = new TestRepository().WithPlugin();

        repo.WithRawSkill(
            "guide",
            "---\nname: guide\ndescription: A guide.\n---\n\nSee [the reference](references/REFERENCE.md).\n");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("links to 'references/REFERENCE.md'"));
    }
}
