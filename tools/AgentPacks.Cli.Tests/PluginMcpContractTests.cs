using System.Text.Json.Nodes;
using AgentPacks.Cli.Io;
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
        var frontmatter = Frontmatter.TryParse(skill, out var error);
        Assert.True(frontmatter is not null, error);
        Assert.Equal("loop", frontmatter!.StringMap("metadata")?["audience"]);
        Assert.Null(frontmatter.Scalar("disable-model-invocation"));
    }

    [Fact]
    public void Authored_skills_agents_and_commands_share_one_frontmatter_shape()
    {
        var root = TestRepository.SourceRoot();
        var loopSkills = 0;
        foreach (var skill in Directory.GetFiles(Path.Combine(root, "plugins"), "SKILL.md",
                     SearchOption.AllDirectories)
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal)
                         && !path.Contains(".cursor-plugin", StringComparison.Ordinal)))
        {
            var text = File.ReadAllText(skill);
            var directoryName = Directory.GetParent(skill)!.Name;
            var parsed = Frontmatter.TryParse(text, out var error);
            Assert.True(parsed is not null, $"{directoryName} frontmatter: {error}");
            Assert.Equal(directoryName, parsed!.Scalar("name"));
            Assert.False(string.IsNullOrWhiteSpace(parsed.Scalar("description")));
            Assert.False(string.IsNullOrWhiteSpace(parsed.Scalar("license")));
            var metadata = parsed.StringMap("metadata");
            var isLoopAudience = metadata is not null
                && metadata.TryGetValue("audience", out var audience)
                && audience.Equals("loop", StringComparison.Ordinal);
            if (isLoopAudience)
            {
                loopSkills++;
                Assert.Null(parsed.Scalar("disable-model-invocation"));
            }
        }

        Assert.True(loopSkills >= 10, $"loop-audience scan must run; found {loopSkills}.");

        foreach (var agent in Directory.GetFiles(Path.Combine(root, "plugins"), "*.md",
                     SearchOption.AllDirectories).Where(path =>
                         path.Contains($"{Path.DirectorySeparatorChar}agents{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                         && !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal)
                         && !path.Contains(".cursor-plugin", StringComparison.Ordinal)))
        {
            var parsed = Frontmatter.TryParse(File.ReadAllText(agent), out var error);
            var name = Path.GetFileNameWithoutExtension(agent);
            Assert.True(parsed is not null, $"{name} frontmatter: {error}");
            Assert.Equal(name, parsed!.Scalar("name"));
            Assert.False(string.IsNullOrWhiteSpace(parsed.Scalar("model")));
            Assert.False(string.IsNullOrWhiteSpace(parsed.Scalar("readonly")));
            Assert.True(parsed.Has("tools"));
        }

        foreach (var command in Directory.GetFiles(Path.Combine(root, "plugins"), "*.md",
                     SearchOption.AllDirectories).Where(path =>
                         path.Contains($"{Path.DirectorySeparatorChar}commands{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                         && !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal)
                         && !path.Contains(".cursor-plugin", StringComparison.Ordinal)))
        {
            var parsed = Frontmatter.TryParse(File.ReadAllText(command), out var error);
            Assert.True(parsed is not null, $"{command} frontmatter: {error}");
            Assert.False(string.IsNullOrWhiteSpace(parsed!.Scalar("name")));
        }

        Assert.False(Directory.Exists(Path.Combine(root, "examples")),
            "Do not add a second examples tree; docs/ADD-*.md plus plugins/ are the examples.");
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
