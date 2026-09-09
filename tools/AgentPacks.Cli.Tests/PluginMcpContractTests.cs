using System.Text.Json.Nodes;
using AgentPacks.Cli.Verification;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Authored MCP is empty scaffolds plus a how-to. No shipped server. No swagger generator.
/// </summary>
public sealed class PluginMcpContractTests
{
    /// <summary>
    /// Po 21 fail bar: no DotnetSolutionMcp project; plugin mcpServers are empty scaffolds.
    /// </summary>
    [Fact]
    public void No_dotnet_solution_mcp_project_and_plugin_mcp_servers_are_empty_scaffolds()
    {
        var root = TestRepository.SourceRoot();
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "dotnet", "mcp")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "dotnet", "mcp", "DotnetSolutionMcp.csproj")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "dotnet", "mcp", "Program.cs")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "dotnet", "mcp", "RoslynLookup.cs")));

        var leftovers = Directory.GetFiles(root, "*DotnetSolutionMcp*", SearchOption.AllDirectories)
            .Where(path => !TestRepository.IsGeneratedPath(path))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .ToArray();
        Assert.False(leftovers.Length > 0, "leftover DotnetSolutionMcp: " + string.Join(", ", leftovers));

        var authored = Directory.GetFiles(Path.Combine(root, "plugins"), "mcp.json",
            SearchOption.AllDirectories);
        Assert.NotEmpty(authored);
        foreach (var path in authored)
        {
            var mcp = JsonNode.Parse(File.ReadAllText(path))!;
            Assert.NotNull(mcp["mcpServers"]);
            Assert.Empty(mcp["mcpServers"]!.AsObject());
        }

        foreach (var relative in new[] { "README.md", Path.Combine("docs", "ADD-SKILL.md") })
        {
            var text = File.ReadAllText(Path.Combine(root, relative));
            Assert.DoesNotContain("DotnetSolutionMcp", text, StringComparison.Ordinal);
            Assert.DoesNotContain("--list-tools", text, StringComparison.Ordinal);
        }
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
            TestRepository.SourceRoot(), "plugins", "dotnet", "skills", "dotnet-solution", "SKILL.md"));

        Assert.Contains("audience: loop", skill, StringComparison.Ordinal);
        Assert.Contains("not as a user entrypoint", skill, StringComparison.Ordinal);
        Assert.Contains("No write tools", skill, StringComparison.Ordinal);
        Assert.Contains("No codegen", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.Contains("dotnet sln", skill, StringComparison.Ordinal);
        Assert.Contains("dotnet list", skill, StringComparison.Ordinal);
        Assert.Contains("Local machine only", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("DotnetSolutionMcp", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("list_symbols", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", skill, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("127.0.0.1", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("Load `/dotnet-solution`", skill, StringComparison.Ordinal);
    }

    [Fact]
    public void Implementer_loads_optional_solution_skill_by_exact_name()
    {
        var implementer = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "agents", "squad-implementer.md"));
        Assert.Contains("<lang>-solution", implementer, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name", implementer, StringComparison.Ordinal);
    }

    [Fact]
    public void Authored_skills_agents_and_commands_share_one_frontmatter_shape()
    {
        var root = TestRepository.SourceRoot();
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
        Assert.Contains("name: squad-reviewer", addAgent, StringComparison.Ordinal);
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
        var docs = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "docs", "ADD-MCP.md"));
        Assert.Contains("filtered", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("one tool per endpoint", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dotnet test tools/AgentPacks.slnx", docs, StringComparison.Ordinal);
        Assert.DoesNotContain("one tool per path", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("local stdio process", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dotnet sln list", docs, StringComparison.Ordinal);
        Assert.Contains("read-only", docs, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("never a hosted URL", docs, StringComparison.Ordinal);
        Assert.Contains("streamable-http", docs, StringComparison.Ordinal);
        Assert.Contains("empty scaffold", docs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("127.0.0.1:8765", docs, StringComparison.Ordinal);
    }

    [Fact]
    public void Squad_mcp_is_empty_scaffold_and_docs_example_is_read_only_http_without_secrets()
    {
        var root = TestRepository.SourceRoot();
        var mcp = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "plugins", "squad", "mcp.json")))!;
        Assert.NotNull(mcp["mcpServers"]);
        Assert.Empty(mcp["mcpServers"]!.AsObject());
        Assert.Null(mcp["mcpServers"]!["url"]);

        using var repo = new TestRepository()
            .WithPlugin("squad", File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")))
            .WithSkill("squad", extraFrontmatter: "disable-model-invocation: true", plugin: "squad")
            .WithMcp(File.ReadAllText(Path.Combine(root, "plugins", "squad", "mcp.json")), plugin: "squad");
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);
        Assert.False(run.HasFile("plugins/squad/.mcp.json"));

        var docs = File.ReadAllText(Path.Combine(root, "docs", "ADD-MCP.md"));
        Assert.Contains("\"type\": \"streamable-http\"", docs, StringComparison.Ordinal);
        Assert.Contains("https://mcp.example.com/architecture", docs, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Authorization\"", docs, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer ", docs, StringComparison.Ordinal);

        foreach (var path in Directory.GetFiles(Path.Combine(root, "plugins"), "mcp.json",
                     SearchOption.AllDirectories))
        {
            var json = File.ReadAllText(path);
            Assert.DoesNotContain("authorization", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("api-key", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"token\"", json, StringComparison.OrdinalIgnoreCase);
        }
    }

}
