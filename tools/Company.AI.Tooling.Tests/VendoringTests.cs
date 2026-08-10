using Company.AI.Tooling.Loading;
using Company.AI.Tooling.Vendoring;

namespace Company.AI.Tooling.Tests;

/// <summary>
/// Drift detection runs offline, so these tests never touch the network. Fetching itself is
/// exercised by running 'vendor' locally when a source is actually approved.
/// </summary>
public class VendoringTests
{
    private const string Sha = "0b8f4c2d9e1a6f3b7c5d2e8a4f6b1c3d5e7a9b0c";
    private const string OtherSha = "1122334455667788990011223344556677889900";

    private static string Sources(string commit = Sha, string? plugin = null) =>
        $$"""
        {
          "sources": [
            {
              "name": "pdf-processing",
              "repository": "https://github.com/example/skills",
              "path": "skills/pdf-processing",
              "commit": "{{commit}}",
              "license": "Apache-2.0",
              "owner": "platform-team"{{(plugin is null ? "" : $",\n      \"plugin\": \"{plugin}\"")}}
            }
          ]
        }
        """;

    /// <summary>Writes a vendored skill exactly as the vendor command would have.</summary>
    private static void PlaceVendoredSkill(TestRepository repo, string commit = Sha)
    {
        repo.WithSkill("pdf-processing", description: "Extracts text from PDFs. Use for PDF work.");

        var target = Path.Combine(repo.PluginDirectory(), "skills", "pdf-processing");
        var manifest = new VendorManifest(
            "pdf-processing",
            "https://github.com/example/skills",
            "skills/pdf-processing",
            commit,
            VendorManifest.HashDirectory(target));

        Io.JsonFile.Write(Path.Combine(target, ExternalSources.MarkerFileName), manifest.ToJson());
    }

    [Fact]
    public void Check_passes_when_vendored_content_matches_the_pin()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(Sources());
        PlaceVendoredSkill(repo);

        var run = repo.VendorCheck();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void Check_reports_a_source_that_was_never_vendored()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(Sources());

        var run = repo.VendorCheck();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("has not been vendored"));
    }

    [Fact]
    public void Check_reports_a_bumped_commit_that_has_not_been_re_vendored()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(Sources(OtherSha));
        PlaceVendoredSkill(repo);

        var run = repo.VendorCheck();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("but external/sources.json pins"));
    }

    [Fact]
    public void Check_reports_local_edits_to_vendored_content()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources(Sources());
        PlaceVendoredSkill(repo);

        File.AppendAllText(
            Path.Combine(repo.PluginDirectory(), "skills", "pdf-processing", "SKILL.md"),
            "\nLocally edited.\n");

        var run = repo.VendorCheck();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("modified since it was vendored"));
    }

    [Fact]
    public void Check_reports_a_vendored_directory_whose_source_entry_was_removed()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources("""{ "sources": [] }""");
        PlaceVendoredSkill(repo);

        var run = repo.VendorCheck();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("no entry in external/sources.json"));
    }

    [Fact]
    public void A_source_must_name_its_plugin_when_the_repository_has_several()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithPlugin("second-plugin", manifest: """
                {
                  "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
                  "name": "second-plugin"
                }
                """)
            .WithExternalSources(Sources());

        var run = repo.VendorCheck();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("must set 'plugin'"));
    }

    [Fact]
    public void A_source_naming_a_missing_plugin_is_reported()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithExternalSources(Sources(plugin: "does-not-exist"));

        var run = repo.VendorCheck();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("which does not exist"));
    }

    [Fact]
    public void Vendored_skills_are_validated_like_any_other_skill()
    {
        using var repo = new TestRepository().WithPlugin().WithExternalSources(Sources());

        // A vendored skill whose frontmatter name does not match its directory is still a finding.
        repo.WithRawSkill("pdf-processing", "---\nname: pdf\ndescription: Mismatched name.\n---\n\nBody.\n");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("but the skill directory is"));
    }

    [Fact]
    public void Content_hash_ignores_the_marker_file_but_tracks_everything_else()
    {
        using var repo = new TestRepository().WithValidPlugin();
        repo.WithSkill("pdf-processing");

        var target = Path.Combine(repo.PluginDirectory(), "skills", "pdf-processing");
        var before = VendorManifest.HashDirectory(target);

        File.WriteAllText(Path.Combine(target, ExternalSources.MarkerFileName), """{"ignored":true}""");
        Assert.Equal(before, VendorManifest.HashDirectory(target));

        Directory.CreateDirectory(Path.Combine(target, "references"));
        File.WriteAllText(Path.Combine(target, "references", "REFERENCE.md"), "New reference.");
        Assert.NotEqual(before, VendorManifest.HashDirectory(target));
    }
}
