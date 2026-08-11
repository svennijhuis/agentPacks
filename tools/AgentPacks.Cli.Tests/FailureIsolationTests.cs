namespace AgentPacks.Cli.Tests;

/// <summary>
/// The specification isolates failures: a bad skill is skipped, a bad mcp.json disables only MCP,
/// and other components keep loading. Our validator is stricter about what fails the build, but it
/// must show every finding in one run rather than stopping at the first.
/// </summary>
public class FailureIsolationTests
{
    [Fact]
    public void Malformed_json_in_one_plugin_does_not_hide_findings_in_another()
    {
        using var repo = new TestRepository().WithValidPlugin();

        repo.WithFile("plugins/broken/plugin.json", "{ not json");
        repo.WithFile(
            "plugins/second/plugin.json",
            """
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "Second-Plugin"
            }
            """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Path.Contains("broken") && d.Message.Contains("not valid JSON"));
        Assert.Contains(run.Diagnostics, d => d.Path.Contains("second"));
    }

    [Fact]
    public void Every_finding_is_reported_in_a_single_run()
    {
        using var repo = new TestRepository()
            .WithPlugin()
            .WithRawSkill("first", "---\nname: first\n---\n\nBody.\n")
            .WithSkill("second", name: "mismatched")
            .WithMcp("""
                {
                  "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
                  "mcpServers": {
                    "docs": {
                      "type": "streamable-http",
                      "url": "https://docs.example.com/mcp",
                      "headers": { "Authorization": "Bearer abc" }
                    }
                  }
                }
                """);

        var run = repo.Validate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("non-empty 'description'"));
        Assert.Contains(run.Diagnostics, d => d.Message.Contains("but the skill directory is"));
        Assert.Contains(run.Diagnostics, d => d.Message.Contains("credential-related"));
    }

    [Fact]
    public void Diagnostics_render_in_a_deterministic_order()
    {
        using var repo = new TestRepository()
            .WithPlugin()
            .WithSkill("alpha", name: "wrong-alpha")
            .WithSkill("beta", name: "wrong-beta");

        var first = repo.Validate().Text;
        var second = repo.Validate().Text;

        Assert.Equal(first, second);
        Assert.True(
            first.IndexOf("alpha", StringComparison.Ordinal) < first.IndexOf("beta", StringComparison.Ordinal),
            "findings should be ordered by path");
    }

    [Fact]
    public void Skills_only_plugin_without_agents_or_mcp_is_valid()
    {
        using var repo = new TestRepository().WithPlugin().WithSkill("testing");

        var run = repo.ValidateAndGenerate();

        Assert.Empty(run.Diagnostics);
    }
}
