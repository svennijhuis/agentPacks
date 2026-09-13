using System.Text.Json.Nodes;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Item 50+51 fail bar: every shipped plugin.json is 0.1.3 so clients see an
/// update when the version string changes. Claude/Copilot skip the install
/// when it is unchanged. Catalog entries that omit version stay omitted.
/// Replaces the item 49 0.1.2 bar.
/// </summary>
public sealed class PluginVersionContractTests
{
    private static readonly string[] PluginNames =
        ["dotnet", "git", "pack-check", "rust", "squad", "typescript"];

    [Fact]
    public void All_plugins_version_0_1_3()
    {
        var root = SourceRoot();
        var plugins = Path.Combine(root, "plugins");
        var names = Directory.GetDirectories(plugins)
            .Select(path => Path.GetFileName(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(PluginNames, names);

        foreach (var name in PluginNames)
        {
            var manifest = JsonNode.Parse(File.ReadAllText(
                Path.Combine(plugins, name, "plugin.json")))!;
            Assert.Equal("0.1.3", manifest["version"]!.GetValue<string>());
        }

        using var repo = new TestRepository()
            .WithPlugin("git", File.ReadAllText(Path.Combine(plugins, "git", "plugin.json")))
            .WithSkill("probe", plugin: "git");
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.Equal(
            "0.1.3",
            run.File("plugins/git/.cursor-plugin/plugin.json").Content["version"]!.GetValue<string>());
        Assert.Equal(
            "0.1.3",
            run.File("plugins/git/.codex-plugin/plugin.json").Content["version"]!.GetValue<string>());

        foreach (var catalog in new[]
                 {
                     ".claude-plugin/marketplace.json",
                     ".github/plugin/marketplace.json",
                     ".cursor-plugin/marketplace.json"
                 })
        {
            var entry = run.File(catalog).Content["plugins"]!.AsArray()
                .OfType<JsonObject>()
                .Single();
            Assert.Null(entry["version"]);
        }
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
