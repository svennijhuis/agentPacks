using System.Text.Json.Nodes;

namespace AgentPacks.Cli.Tests;

/// <summary>The authored standards catalog is the only interface maintainers edit.</summary>
public class StandardsGenerationTests
{
    private const string Catalog = """
        {
          "$schema": "../../schema/standards.schema.json",
          "version": 1,
          "documents": {
            "csharp": "standards/csharp.md",
            "testing": "standards/testing.md"
          },
          "consumers": {
            "dotnet-build": ["csharp"],
            "dotnet-review": ["csharp", "testing"]
          }
        }
        """;

    private static TestRepository Repository() =>
        new TestRepository()
            .WithPlugin()
            .WithSkill("dotnet-build")
            .WithSkill("dotnet-review")
            .WithFile("plugins/engineering/standards/csharp.md", "# C#\n\nUse nullable types.\n")
            .WithFile("plugins/engineering/standards/testing.md", "# Testing\n\nTest behaviour.\n");

    [Fact]
    public void Canonical_documents_are_generated_only_for_declared_consumers()
    {
        using var repo = Repository().WithStandards(Catalog);

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.True(run.HasFile("plugins/engineering/skills/dotnet-build/references/standards/csharp.md"));
        Assert.False(run.HasFile("plugins/engineering/skills/dotnet-build/references/standards/testing.md"));
        Assert.True(run.HasFile("plugins/engineering/skills/dotnet-review/references/standards/csharp.md"));
        Assert.True(run.HasFile("plugins/engineering/skills/dotnet-review/references/standards/testing.md"));
    }

    [Fact]
    public void Generated_standard_names_its_canonical_source()
    {
        using var repo = Repository().WithStandards(Catalog);

        var generated = repo.ValidateAndGenerate()
            .File("plugins/engineering/skills/dotnet-review/references/standards/csharp.md")
            .Text;

        Assert.Contains("Generated from standards/csharp.md", generated, StringComparison.Ordinal);
        Assert.Contains("Use nullable types.", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void All_four_providers_share_one_generated_skill_reference_tree()
    {
        using var repo = Repository().WithStandards(Catalog);
        var run = repo.ValidateAndGenerate();

        var claude = (JsonObject)run.File(".claude-plugin/marketplace.json").Content["plugins"]![0]!;
        var copilot = (JsonObject)run.File(".github/plugin/marketplace.json").Content["plugins"]![0]!;
        var codex = run.File("plugins/engineering/.codex-plugin/plugin.json").Content;
        var cursor = run.File("plugins/engineering/.cursor-plugin/plugin.json").Content;

        Assert.Equal("./skills/", claude["skills"]!.GetValue<string>());
        Assert.Equal("./skills/", copilot["skills"]!.GetValue<string>());
        Assert.Equal("./skills/", codex["skills"]!.GetValue<string>());
        Assert.Null(cursor["skills"]); // Cursor uses the standard root skills/ convention.
        Assert.Single(run.Generated, f => f.RelativePath.Replace('\\', '/') ==
            "plugins/engineering/skills/dotnet-build/references/standards/csharp.md");
    }

    [Theory]
    [InlineData("missing.md", "does not exist")]
    [InlineData("../outside.md", "escapes the plugin directory")]
    [InlineData("standards/csharp.txt", "relative .md file")]
    public void Invalid_document_paths_are_rejected(string path, string expected)
    {
        using var repo = Repository().WithStandards(Catalog.Replace("standards/csharp.md", path));

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains(expected, run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Unknown_consumer_is_rejected()
    {
        using var repo = Repository().WithStandards(Catalog.Replace("dotnet-build", "dotnet-missing"));

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("consumer 'dotnet-missing' is not a skill", run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Unknown_document_reference_is_rejected()
    {
        using var repo = Repository().WithStandards(Catalog.Replace("[\"csharp\"]", "[\"unknown\"]"));

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("references unknown document 'unknown'", run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Unused_document_is_rejected_by_policy()
    {
        var withoutTesting = Catalog.Replace("[\"csharp\", \"testing\"]", "[\"csharp\"]");
        using var repo = Repository().WithStandards(withoutTesting);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("document 'testing' has no consumer", run.Text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"version\": 1", "\"version\": 2", "integer 'version': 1")]
    [InlineData("\"version\": 1,", "\"version\": 1, \"extra\": true,", "unknown top-level key 'extra'")]
    [InlineData("[\"csharp\"]", "[\"csharp\", \"csharp\"]", "more than once")]
    public void Catalog_shape_errors_are_rejected(string oldText, string newText, string expected)
    {
        using var repo = Repository().WithStandards(Catalog.Replace(oldText, newText));

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains(expected, run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_then_revalidate_does_not_treat_http_api_rest_paths_as_skills()
    {
        using var repo = Repository()
            .WithFile(
                "plugins/engineering/standards/http-api.md",
                "Nouns in paths (`/users`, `/users/{id}`).\n")
            .WithStandards("""
                {
                  "$schema": "../../schema/standards.schema.json",
                  "version": 1,
                  "documents": {
                    "http-api": "standards/http-api.md"
                  },
                  "consumers": {
                    "dotnet-build": ["http-api"]
                  }
                }
                """);

        var generated = repo.ValidateAndGenerate();
        Assert.False(generated.HasErrors, generated.Text);
        Assert.True(generated.HasFile(
            "plugins/engineering/skills/dotnet-build/references/standards/http-api.md"));

        var again = repo.Validate();
        Assert.DoesNotContain(again.Diagnostics, d => d.Message.Contains("invokes `/users`"));
        Assert.False(again.HasErrors, again.Text);
    }

    [Fact]
    public void Removing_a_mapping_removes_only_generated_standard_references()
    {
        using var repo = Repository().WithStandards(Catalog);
        repo.WithFile(
            "plugins/engineering/skills/dotnet-review/references/notes.md",
            "# Authored notes\n");
        repo.ValidateAndGenerate();

        repo.WithStandards(Catalog.Replace("[\"csharp\", \"testing\"]", "[\"testing\"]"));
        repo.ValidateAndGenerate();

        Assert.False(File.Exists(Path.Combine(
            repo.Root,
            "plugins/engineering/skills/dotnet-review/references/standards/csharp.md")));
        Assert.True(File.Exists(Path.Combine(
            repo.Root,
            "plugins/engineering/skills/dotnet-review/references/notes.md")));
    }

    [Fact]
    public void Shared_path_generates_into_consumer_references_without_a_pack_copy()
    {
        using var repo = Repository()
            .WithFile("shared/standards/http-api.md", "# HTTP API\n\nShared envelope.\n")
            .WithStandards("""
                {
                  "$schema": "../../schema/standards.schema.json",
                  "version": 1,
                  "documents": {
                    "http-api": "shared/standards/http-api.md"
                  },
                  "consumers": {
                    "dotnet-review": ["http-api"]
                  }
                }
                """);

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.True(run.HasFile("plugins/engineering/skills/dotnet-review/references/standards/http-api.md"));
        Assert.False(run.HasFile("plugins/engineering/standards/http-api.md"));

        var generated = run.File("plugins/engineering/skills/dotnet-review/references/standards/http-api.md").Text;
        Assert.Contains("Generated from shared/standards/http-api.md", generated, StringComparison.Ordinal);
        Assert.Contains("Shared envelope.", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void Pack_document_that_reuses_a_shared_filename_is_rejected()
    {
        using var repo = Repository()
            .WithFile("shared/standards/http-api.md", "# Shared\n")
            .WithFile("plugins/engineering/standards/http-api.md", "# Pack copy\n")
            .WithStandards("""
                {
                  "$schema": "../../schema/standards.schema.json",
                  "version": 1,
                  "documents": {
                    "http-api": "standards/http-api.md"
                  },
                  "consumers": {
                    "dotnet-review": ["http-api"]
                  }
                }
                """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("same filename as shared/standards/http-api.md", run.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Byte_identical_authored_standards_are_rejected()
    {
        const string text = "# HTTP API\n\nSame body.\n";
        using var repo = Repository()
            .WithFile("shared/standards/http-api.md", text)
            .WithFile("plugins/engineering/standards/other.md", text)
            .WithStandards("""
                {
                  "$schema": "../../schema/standards.schema.json",
                  "version": 1,
                  "documents": {
                    "other": "standards/other.md"
                  },
                  "consumers": {
                    "dotnet-review": ["other"]
                  }
                }
                """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
        Assert.Contains("has the same content as", run.Text, StringComparison.Ordinal);
        Assert.Contains("shared/standards/", run.Text, StringComparison.Ordinal);
    }
}
