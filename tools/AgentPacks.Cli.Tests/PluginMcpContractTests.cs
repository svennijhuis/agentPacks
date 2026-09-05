using System.Text.Json.Nodes;
using AgentPacks.Cli.Verification;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Authored MCP is local, read-only, and fixture-tested. No hosted service. No swagger generator.
/// </summary>
public sealed class PluginMcpContractTests
{
    [Fact]
    public void Dotnet_mcp_is_local_stdio_dotnet_process_with_no_credentials()
    {
        var mcp = JsonNode.Parse(File.ReadAllText(Path.Combine(SourceRoot(), "plugins", "dotnet", "mcp.json")))!;
        var server = mcp["mcpServers"]!["dotnet-solution"]!;
        var json = mcp.ToJsonString();

        Assert.Equal("stdio", server["type"]!.GetValue<string>());
        Assert.Equal("dotnet", server["command"]!.GetValue<string>());
        Assert.Contains("${PLUGIN_ROOT}/mcp/DotnetSolutionMcp.csproj", json, StringComparison.Ordinal);
        Assert.Null(server["url"]);
        Assert.Null(server["headers"]);
        Assert.DoesNotContain("authorization", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"url\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Authored_dotnet_mcp_validates_and_generates_the_claude_file()
    {
        var root = SourceRoot();
        using var repo = new TestRepository()
            .WithPlugin("dotnet", File.ReadAllText(Path.Combine(root, "plugins", "dotnet", "plugin.json")))
            .WithLoopSkill("dotnet-build")
            .WithLoopSkill("dotnet-test-patterns")
            .WithMcp(File.ReadAllText(Path.Combine(root, "plugins", "dotnet", "mcp.json")), plugin: "dotnet");

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        var generated = run.File("plugins/dotnet/.mcp.json").Content["mcpServers"]!["dotnet-solution"]!;
        Assert.Equal("stdio", generated["type"]!.GetValue<string>());
        Assert.Contains("${CLAUDE_PLUGIN_ROOT}/mcp/DotnetSolutionMcp.csproj", generated.ToJsonString(), StringComparison.Ordinal);
        Assert.Null(generated["url"]);
    }

    [Fact]
    public void Solution_tools_are_three_read_only_lookups_on_local_files()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Fixtures", "dotnet-solution");
        var sln = File.ReadAllText(Path.Combine(dir, "Sample.sln"));
        var slnx = File.ReadAllText(Path.Combine(dir, "Sample.slnx"));
        var csproj = File.ReadAllText(Path.Combine(dir, "Sample.csproj"));
        var props = File.ReadAllText(Path.Combine(dir, "Directory.Packages.props"));

        Assert.Equal(["src/Sample/Sample.csproj"], DotnetSolutionTools.ListProjects(sln));
        Assert.Equal(["src/Sample/Sample.csproj"], DotnetSolutionTools.ListProjects(slnx));

        var packages = DotnetSolutionTools.ListPackages(csproj, props);
        Assert.Contains(packages, p => p.Id == "xunit" && p.Version == "2.9.3");
        Assert.Contains(packages, p => p.Id == "Newtonsoft.Json" && p.Version == "13.0.3");
        Assert.Contains("xunit 2.9.3", DotnetSolutionTools.DescribeProject("src/Sample/Sample.csproj", packages));

        Assert.Equal(["list_projects", "list_packages", "describe_project"], DotnetSolutionTools.ReadOnlyTools);
        Assert.True(DotnetSolutionTools.IsAllowedTool("list_projects"));
        Assert.False(DotnetSolutionTools.IsAllowedTool("apply_fix"));
        Assert.False(DotnetSolutionTools.IsAllowedTool("generate_code"));
        Assert.False(DotnetSolutionTools.IsAllowedTool("restore"));
    }

    [Fact]
    public void Swagger_recipe_keeps_a_filtered_get_set_not_one_tool_per_endpoint()
    {
        var openApi = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Fixtures", "dotnet-solution", "pets.openapi.json"));

        var tools = SwaggerToolFilter.SelectGetTools(openApi, ["pets"]);

        Assert.Equal(2, tools.Count);
        Assert.Contains("listPets", tools);
        Assert.Contains("getPet", tools);
        Assert.DoesNotContain("createPet", tools);
        Assert.DoesNotContain("deletePet", tools);
        Assert.DoesNotContain("listUsers", tools);
        Assert.True(tools.Count < 5);
        Assert.True(tools.Count <= SwaggerToolFilter.MaxTools);
    }

    [Fact]
    public void Dotnet_solution_skill_is_loop_internal_and_names_the_local_tools()
    {
        var skill = File.ReadAllText(Path.Combine(
            SourceRoot(), "plugins", "dotnet", "skills", "dotnet-solution", "SKILL.md"));

        Assert.Contains("audience: loop", skill, StringComparison.Ordinal);
        Assert.Contains("not as a user entrypoint", skill, StringComparison.Ordinal);
        Assert.Contains("list_projects", skill, StringComparison.Ordinal);
        Assert.Contains("list_packages", skill, StringComparison.Ordinal);
        Assert.Contains("describe_project", skill, StringComparison.Ordinal);
        Assert.Contains("No write tools", skill, StringComparison.Ordinal);
        Assert.Contains("No codegen", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.Contains("dotnet sln", skill, StringComparison.Ordinal);
        Assert.Contains("dotnet list", skill, StringComparison.Ordinal);
        Assert.Contains("Local machine only", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", skill, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("127.0.0.1", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("Load `/dotnet-solution`", skill, StringComparison.Ordinal);
    }

    [Fact]
    public void Implementer_loads_optional_solution_skill_by_exact_name()
    {
        var implementer = File.ReadAllText(Path.Combine(
            SourceRoot(), "plugins", "squad", "agents", "loop-implementer.md"));
        Assert.Contains("<lang>-solution", implementer, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name", implementer, StringComparison.Ordinal);
    }

    [Fact]
    public void Authored_skills_agents_and_commands_share_one_frontmatter_shape()
    {
        var root = SourceRoot();
        foreach (var skill in Directory.GetFiles(Path.Combine(root, "plugins"), "SKILL.md",
                     SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(skill);
            var directoryName = Directory.GetParent(skill)!.Name;
            Assert.Contains($"name: {directoryName}", text, StringComparison.Ordinal);
            Assert.Contains("description:", text, StringComparison.Ordinal);
            Assert.Contains("license:", text, StringComparison.Ordinal);
            Assert.DoesNotContain("delivery", text, StringComparison.OrdinalIgnoreCase);
            if (text.Contains("Internal loop skill", StringComparison.Ordinal))
            {
                Assert.Contains("audience: loop", text, StringComparison.Ordinal);
                Assert.Contains("not as a user entrypoint", text, StringComparison.Ordinal);
                Assert.DoesNotContain("disable-model-invocation: true", text, StringComparison.Ordinal);
            }
        }

        foreach (var agent in Directory.GetFiles(Path.Combine(root, "plugins"), "*.md",
                     SearchOption.AllDirectories).Where(path => path.Contains($"{Path.DirectorySeparatorChar}agents{Path.DirectorySeparatorChar}")))
        {
            var text = File.ReadAllText(agent);
            var name = Path.GetFileNameWithoutExtension(agent);
            Assert.Contains($"name: {name}", text, StringComparison.Ordinal);
            Assert.Contains("model:", text, StringComparison.Ordinal);
            Assert.Contains("readonly:", text, StringComparison.Ordinal);
            Assert.Contains("tools:", text, StringComparison.Ordinal);
            Assert.Contains("Skill tool by exact name", text, StringComparison.Ordinal);
            Assert.DoesNotContain("delivery", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("loop-tester", text, StringComparison.Ordinal);
        }

        foreach (var command in Directory.GetFiles(Path.Combine(root, "plugins"), "*.md",
                     SearchOption.AllDirectories).Where(path => path.Contains($"{Path.DirectorySeparatorChar}commands{Path.DirectorySeparatorChar}")))
        {
            var text = File.ReadAllText(command);
            Assert.Contains("name:", text, StringComparison.Ordinal);
            Assert.Contains("Skill tool by exact name", text, StringComparison.Ordinal);
            Assert.DoesNotContain("delivery", text, StringComparison.OrdinalIgnoreCase);
        }

        var addAgent = File.ReadAllText(Path.Combine(root, "docs", "ADD-AGENT.md"));
        Assert.Contains("name: loop-reviewer", addAgent, StringComparison.Ordinal);
        Assert.Contains("model: fast", addAgent, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name", addAgent, StringComparison.Ordinal);
        Assert.DoesNotContain("name: security-reviewer", addAgent, StringComparison.Ordinal);
        Assert.DoesNotContain("delivery", addAgent, StringComparison.OrdinalIgnoreCase);

        var addSkill = File.ReadAllText(Path.Combine(root, "docs", "ADD-SKILL.md"));
        Assert.Contains("audience: loop", addSkill, StringComparison.Ordinal);
        Assert.Contains("not as a user entrypoint", addSkill, StringComparison.Ordinal);
        Assert.Contains("license: UNLICENSED", addSkill, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name", addSkill, StringComparison.Ordinal);
        Assert.DoesNotContain("delivery", addSkill, StringComparison.OrdinalIgnoreCase);

        Assert.False(Directory.Exists(Path.Combine(root, "examples")),
            "Do not add a second examples tree; docs/ADD-*.md plus plugins/ are the examples.");
    }

    [Fact]
    public void Add_mcp_documents_the_swagger_recipe_and_local_test()
    {
        var docs = File.ReadAllText(Path.Combine(SourceRoot(), "docs", "ADD-MCP.md"));
        Assert.Contains("filtered", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("one tool per endpoint", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dotnet test tools/AgentPacks.slnx", docs, StringComparison.Ordinal);
        Assert.DoesNotContain("one tool per path", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("local stdio process", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dotnet sln list", docs, StringComparison.Ordinal);
        Assert.Contains("read-only", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("never a hosted URL", docs, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1:8765", docs, StringComparison.Ordinal);
    }

    [Fact]
    public void Local_solution_cli_lists_tools_without_a_network()
    {
        var project = Path.Combine(SourceRoot(), "plugins", "dotnet", "mcp", "DotnetSolutionMcp.csproj");
        var start = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            ArgumentList = { "run", "--project", project, "--", "--list-tools" }
        };
        using var process = System.Diagnostics.Process.Start(start);
        Assert.NotNull(process);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(60_000);
        Assert.Equal(0, process.ExitCode);
        Assert.Contains("list_projects", output, StringComparison.Ordinal);
        Assert.Contains("list_packages", output, StringComparison.Ordinal);
        Assert.Contains("describe_project", output, StringComparison.Ordinal);
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
