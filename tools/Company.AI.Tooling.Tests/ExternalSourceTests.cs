using System.Text.Json.Nodes;

namespace Company.AI.Tooling.Tests;

/// <summary>
/// External skills are referenced, never copied: the catalog points clients at the upstream
/// directory at a pinned commit, and this repository stores only the URL and a little metadata.
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

    private static JsonObject ExternalEntry(ValidationRun run) =>
        ((JsonArray)run.File(".claude-plugin/marketplace.json").Content["plugins"]!)
        .OfType<JsonObject>()
        .Single(e => e["name"]!.GetValue<string>() == "code-review");

    [Fact]
    public void An_external_skill_becomes_a_pinned_git_subdir_entry()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource);

        var source = (JsonObject)ExternalEntry(repo.ValidateAndGenerate())["source"]!;

        Assert.Equal("git-subdir", source["source"]!.GetValue<string>());
        Assert.Equal("https://github.com/mattpocock/skills", source["url"]!.GetValue<string>());
        Assert.Equal("skills/engineering/code-review", source["path"]!.GetValue<string>());
        Assert.Equal(Sha, source["sha"]!.GetValue<string>());
    }

    [Fact]
    public void Nothing_from_the_external_skill_is_written_into_this_repository()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource);

        repo.ValidateAndGenerate();

        Assert.False(Directory.Exists(Path.Combine(repo.PluginDirectory(), "skills", "code-review")));
    }

    [Fact]
    public void Only_the_catalog_metadata_is_carried_across()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(OneSource);

        var entry = ExternalEntry(repo.ValidateAndGenerate());

        Assert.Equal("Two-axis review of a diff.", entry["description"]!.GetValue<string>());

        // The upstream directory defines itself; we neither restate its components nor claim
        // authority over them with "strict".
        Assert.Null(entry["skills"]);
        Assert.Null(entry["strict"]);
        Assert.Null(entry["version"]);
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
