namespace AgentPacks.Cli.Tests;

/// <summary>
/// Structural manifest rules come from the vendored schema; these tests prove the schema is
/// actually wired in and that the semantics we add on top fire.
/// </summary>
public class ManifestValidationTests
{
    [Fact]
    public void Valid_source_produces_no_diagnostics()
    {
        using var repo = new TestRepository().WithValidPlugin();

        var run = repo.Validate();

        Assert.Empty(run.Diagnostics);
    }

    [Fact]
    public void Missing_plugin_json_is_reported()
    {
        using var repo = new TestRepository();
        Directory.CreateDirectory(repo.PluginDirectory());

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Path.EndsWith("plugin.json") && d.Message.Contains("missing"));
    }

    [Fact]
    public void Unknown_top_level_field_fails_even_though_clients_tolerate_it()
    {
        using var repo = new TestRepository().WithPlugin(manifest: """
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "engineering",
              "hooks": "./hooks/"
            }
            """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Theory]
    [InlineData("My-Plugin")]
    [InlineData("-start")]
    [InlineData("has--double")]
    [InlineData("too.many..dots")]
    public void Invalid_plugin_names_are_rejected(string name)
    {
        using var repo = new TestRepository().WithPlugin(manifest: $$"""
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "{{name}}"
            }
            """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void Author_must_be_an_object_not_a_string()
    {
        using var repo = new TestRepository().WithPlugin(manifest: """
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "engineering",
              "author": "Company Developer Platform"
            }
            """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void Keywords_must_be_an_array_of_strings()
    {
        using var repo = new TestRepository().WithPlugin(manifest: """
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "engineering",
              "keywords": [1, 2]
            }
            """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void Unsupported_schema_version_is_named_clearly_and_never_fetched()
    {
        using var repo = new TestRepository().WithPlugin(manifest: """
            {
              "$schema": "https://agent-plugins.org/schemas/2.0.0/plugin.schema.json",
              "name": "engineering"
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("unsupported schema"));
    }

    [Fact]
    public void Mcp_json_must_target_the_same_specification_version_as_the_manifest()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithMcp("""
                {
                  "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
                  "mcpServers": {}
                }
                """);

        var run = repo.Validate();

        Assert.True(run.HasErrors);
    }

    [Fact]
    public void Duplicate_plugin_names_across_directories_are_rejected()
    {
        using var repo = new TestRepository()
            .WithValidPlugin()
            .WithPlugin("second-copy");

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("already used by"));
    }

    [Fact]
    public void External_sources_file_is_optional()
    {
        using var repo = new TestRepository().WithValidPlugin().WithoutExternalSources();

        var run = repo.Validate();

        Assert.False(run.HasErrors);
    }

    [Fact]
    public void External_source_must_pin_an_exact_commit_sha()
    {
        using var repo = new TestRepository().WithValidPlugin().WithExternalSources("""
            {
              "sources": [
                {
                  "name": "some-skill",
                  "repository": "https://github.com/example/skills",
                  "path": "skills/some-skill",
                  "license": "MIT",
                  "owner": "platform-team",
                  "commit": "main"
                }
              ]
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("40-character Git commit SHA"));
    }

    [Fact]
    public void Symlink_escaping_the_plugin_root_is_rejected()
    {
        using var repo = new TestRepository().WithValidPlugin();

        repo.WithSymlink("escape.txt", Path.Combine(Path.GetTempPath(), "outside-the-package.txt"));

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("resolves outside the plugin root"));
    }

    [Fact]
    public void A_file_only_client_extension_directory_is_tolerated()
    {
        using var repo = new TestRepository().WithValidPlugin();

        repo.WithFile(
            "plugins/engineering/com.example.client/hooks/hooks.json",
            "{}\n");

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors);
        Assert.DoesNotContain("com.example.client", run.File(".claude-plugin/marketplace.json").Content.ToJsonString());
    }
}
