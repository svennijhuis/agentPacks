using System.Text.Json.Nodes;
using Company.AI.Tooling.Cli;
using Company.AI.Tooling.Io;

namespace Company.AI.Tooling.Tests;

public class GenerationTests
{
    private const string StdioAndRemote = """
        {
          "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
          "mcpServers": {
            "validator": {
              "type": "stdio",
              "command": "./bin/validator",
              "args": ["--data", "${PLUGIN_DATA}/validator", "--pattern", "./*.cs"],
              "env": { "CONFIG": "${PLUGIN_ROOT}/config.json" },
              "cwd": "${PLUGIN_ROOT}"
            },
            "deployment-api": {
              "type": "streamable-http",
              "url": "https://deploy.example.com/mcp?path=./ignored",
              "headers": { "X-Tenant": "public-tenant" }
            },
            "legacy": {
              "type": "sse",
              "url": "https://legacy.example.com/mcp"
            }
          }
        }
        """;

    [Fact]
    public void Marketplace_entry_omits_version_so_updates_resolve_from_the_commit_sha()
    {
        using var repo = new TestRepository().WithPlugin(manifest: """
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "engineering",
              "version": "0.1.0",
              "description": "Test plugin."
            }
            """).WithSkill("testing");

        var entry = FirstEntry(repo.ValidateAndGenerate());

        Assert.Null(entry["version"]);
        Assert.Equal("./plugins/engineering", entry["source"]!.GetValue<string>());
        Assert.False(entry["strict"]!.GetValue<bool>());
    }

    [Fact]
    public void Component_paths_are_declared_only_when_the_directories_exist()
    {
        using var repo = new TestRepository().WithPlugin().WithSkill("testing");

        var entry = FirstEntry(repo.ValidateAndGenerate());

        Assert.Equal("./skills/", entry["skills"]!.GetValue<string>());
        Assert.Null(entry["mcpServers"]);
    }

    [Fact]
    public void Mcp_json_is_generated_and_pointed_at_by_the_entry()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);

        var run = repo.ValidateAndGenerate();
        var entry = FirstEntry(run);

        Assert.Equal("./.mcp.json", entry["mcpServers"]!.GetValue<string>());
        Assert.True(run.HasFile("plugins/engineering/.mcp.json"));
        Assert.True(File.Exists(Path.Combine(repo.PluginDirectory(), ".mcp.json")));
    }

    [Fact]
    public void Empty_mcp_servers_produce_no_generated_file()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp("""
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
              "mcpServers": {}
            }
            """);

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasFile("plugins/engineering/.mcp.json"));
        Assert.Null(FirstEntry(run)["mcpServers"]);
    }

    [Fact]
    public void Conversion_rewrites_only_the_fields_the_specification_expands()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);

        var servers = (JsonObject)repo.ValidateAndGenerate()
            .File("plugins/engineering/.mcp.json").Content["mcpServers"]!;

        var stdio = servers["validator"]!;
        var remote = servers["deployment-api"]!;

        // command: plugin-relative path resolved, no placeholder expansion involved.
        Assert.Equal("${CLAUDE_PLUGIN_ROOT}/bin/validator", stdio["command"]!.GetValue<string>());

        // args and env values expand; an opaque "./*.cs" argument is left exactly as authored.
        Assert.Equal("${CLAUDE_PLUGIN_DATA}/validator", stdio["args"]![1]!.GetValue<string>());
        Assert.Equal("./*.cs", stdio["args"]![3]!.GetValue<string>());
        Assert.Equal("${CLAUDE_PLUGIN_ROOT}/config.json", stdio["env"]!["CONFIG"]!.GetValue<string>());
        Assert.Equal("${CLAUDE_PLUGIN_ROOT}", stdio["cwd"]!.GetValue<string>());

        // URLs and headers are never reinterpreted, even when they contain "./".
        Assert.Equal("https://deploy.example.com/mcp?path=./ignored", remote["url"]!.GetValue<string>());
        Assert.Equal("public-tenant", remote["headers"]!["X-Tenant"]!.GetValue<string>());
    }

    [Fact]
    public void Transports_are_mapped_to_the_values_claude_uses()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);

        var servers = (JsonObject)repo.ValidateAndGenerate()
            .File("plugins/engineering/.mcp.json").Content["mcpServers"]!;

        Assert.Equal("stdio", servers["validator"]!["type"]!.GetValue<string>());
        Assert.Equal("http", servers["deployment-api"]!["type"]!.GetValue<string>());
        Assert.Equal("sse", servers["legacy"]!["type"]!.GetValue<string>());
    }

    [Fact]
    public void Dropping_the_last_mcp_server_deletes_the_generated_file()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);
        repo.ValidateAndGenerate();

        var generated = Path.Combine(repo.PluginDirectory(), ".mcp.json");
        Assert.True(File.Exists(generated));

        repo.WithMcp("""
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
              "mcpServers": {}
            }
            """);

        repo.ValidateAndGenerate();

        // Leaving it behind would keep Claude loading servers the source no longer declares.
        Assert.False(File.Exists(generated));
    }

    [Fact]
    public void Check_reports_a_leftover_generated_mcp_file()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);
        repo.ValidateAndGenerate();

        repo.WithMcp("""
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
              "mcpServers": {}
            }
            """);

        var run = repo.ValidateAndGenerate(new CommandOptions { Check = true });

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("left over from a previous generation"));
    }

    [Fact]
    public void A_portable_dotted_plugin_name_fails_marketplace_compatibility()
    {
        using var repo = new TestRepository().WithPlugin(manifest: """
            {
              "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
              "name": "company.platform"
            }
            """);

        var run = repo.ValidateAndGenerate();

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("not for a Claude or Copilot"));
    }

    [Fact]
    public void Generation_is_deterministic()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);

        var first = JsonFile.Serialize(repo.ValidateAndGenerate().File(".claude-plugin/marketplace.json").Content);
        var second = JsonFile.Serialize(repo.ValidateAndGenerate().File(".claude-plugin/marketplace.json").Content);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Check_reports_a_hand_edited_generated_file()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);

        repo.ValidateAndGenerate();
        File.WriteAllText(Path.Combine(repo.Root, ".claude-plugin", "marketplace.json"), "{}\n");

        var run = repo.ValidateAndGenerate(new CommandOptions { Check = true });

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("does not match what the source generates"));
    }

    [Fact]
    public void Check_reports_a_missing_generated_file()
    {
        using var repo = new TestRepository().WithValidPlugin();

        var run = repo.ValidateAndGenerate(new CommandOptions { Check = true });

        Assert.Contains(run.Diagnostics, d => d.Message.Contains("is missing"));
    }

    [Fact]
    public void Out_writes_outside_the_repository_and_leaves_the_source_tree_clean()
    {
        using var repo = new TestRepository().WithValidPlugin().WithMcp(StdioAndRemote);
        var output = Path.Combine(Path.GetTempPath(), "company-ai-out", Guid.NewGuid().ToString("N"));

        try
        {
            repo.ValidateAndGenerate(new CommandOptions { OutputRoot = output });

            Assert.True(File.Exists(Path.Combine(output, ".claude-plugin", "marketplace.json")));
            Assert.False(File.Exists(Path.Combine(repo.Root, ".claude-plugin", "marketplace.json")));
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static JsonObject FirstEntry(ValidationRun run) =>
        (JsonObject)run.File(".claude-plugin/marketplace.json").Content["plugins"]![0]!;
}
