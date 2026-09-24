using System.Text.Json.Nodes;
using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Loading;
using AgentPacks.Cli.Validation;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Reviewer fail bars for the official-schema Cursor catalog (PR #31).
/// Closed #27 / #29 / #30 are superseded twins; this is the sole catalog track.
/// </summary>
public sealed class CursorCatalogContractTests
{
    private static readonly string[] PluginNames =
        ["dotnet", "git", "pack-check", "rust", "security", "squad", "typescript"];

    private static readonly string[] SquadAgents =
    [
        "squad-planner",
        "squad-implementer",
        "squad-verifier",
        "squad-reviewer",
        "squad-security-reviewer",
        "squad-simplifier",
        "squad-orchestrator"
    ];

    /// <summary>
    /// Reviewer named proof. Official-schema Cursor catalog ships: generated
    /// <c>.cursor-plugin/marketplace.json</c> lists every pack with only
    /// <c>name</c> / <c>source</c> / <c>description</c>. Sources omit <c>./</c>.
    /// Extra entry fields (author, version, keywords, homepage) are not emitted.
    /// </summary>
    [Fact]
    public void Official_schema_cursor_catalog_ships()
    {
        using var repo = ShippedPlugins();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var marketplace = run.File(".cursor-plugin/marketplace.json").Content;
        Assert.Equal(
            ["name", "owner", "metadata", "plugins"],
            marketplace.AsObject().Select(property => property.Key).ToArray());

        var entries = marketplace["plugins"]!.AsArray().OfType<JsonObject>().ToArray();
        Assert.Equal(PluginNames, entries.Select(entry => entry["name"]!.GetValue<string>()).OrderBy(name => name, StringComparer.Ordinal));

        foreach (var entry in entries)
        {
            var name = entry["name"]!.GetValue<string>();
            Assert.Equal(["name", "source", "description"], entry.Select(property => property.Key).ToArray());
            Assert.Equal($"plugins/{name}", entry["source"]!.GetValue<string>());
            Assert.False(string.IsNullOrWhiteSpace(entry["description"]!.GetValue<string>()));
            Assert.Null(entry["version"]);
            Assert.Null(entry["author"]);
            Assert.Null(entry["keywords"]);
            Assert.Null(entry["homepage"]);
            Assert.Null(entry["agents"]);
            Assert.Null(entry["commands"]);
            Assert.Null(entry["hooks"]);
        }
    }

    /// <summary>
    /// Reviewer named proof. Extra catalog fields and <c>./</c> sources fail
    /// compatibility the way cursor/plugins <c>additionalProperties: false</c> does.
    /// </summary>
    [Fact]
    public void Official_schema_cursor_catalog_rejects_extra_entry_fields()
    {
        using var repo = new TestRepository().WithValidPlugin();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var marketplace = (JsonObject)run.File(".cursor-plugin/marketplace.json").Content.DeepClone()!;
        marketplace["repository"] = "https://github.com/svennijhuis/agentPacks";
        var entry = (JsonObject)marketplace["plugins"]![0]!;
        entry["author"] = "agentPacks";
        entry["version"] = "1.0.0";
        entry["keywords"] = new JsonArray(JsonValue.Create("catalog"));
        entry["source"] = "./plugins/engineering";

        var context = new RepositoryContext { Root = repo.Root };
        new CompatibilityValidator(context).Validate(
            [GeneratedFile.FromJson(".cursor-plugin/marketplace.json", marketplace)]);

        var text = context.Diagnostics.Render();
        Assert.True(context.Diagnostics.HasErrors, text);
        Assert.Contains("not in the official catalog schema", text, StringComparison.Ordinal);
        Assert.Contains("without './'", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reviewer named proof. Remapped squad agents load: <c>plugin.json</c> points
    /// at <c>./.cursor-plugin/agents/</c> so Cursor does not scan portable root
    /// <c>agents/</c>. Generated ids are Cursor ids, not Claude aliases.
    /// </summary>
    [Fact]
    public void Remapped_squad_agents_load()
    {
        var root = TestRepository.SourceRoot();
        using var repo = new TestRepository()
            .WithPlugin(
                "squad",
                File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")))
            .WithCopiedDirectory(root, "plugins/squad/agents", "*.md");

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.Equal(
            "./.cursor-plugin/agents/",
            run.File("plugins/squad/.cursor-plugin/plugin.json").Content["agents"]!.GetValue<string>());

        var remapped = run.Generated
            .Select(file => file.RelativePath.Replace('\\', '/'))
            .Where(path => path.StartsWith("plugins/squad/.cursor-plugin/agents/", StringComparison.Ordinal))
            .Select(path => Path.GetFileNameWithoutExtension(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(SquadAgents.OrderBy(name => name, StringComparer.Ordinal), remapped);

        foreach (var agent in SquadAgents)
        {
            var text = run.File($"plugins/squad/.cursor-plugin/agents/{agent}.md").Text;
            Assert.DoesNotContain("model: \"sonnet\"", text, StringComparison.Ordinal);
            Assert.DoesNotContain("model: \"haiku\"", text, StringComparison.Ordinal);
            Assert.DoesNotContain("model: \"opus\"", text, StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(repo.Root, "plugins", "squad", "agents", $"{agent}.md")));
            Assert.False(run.HasFile($"plugins/squad/agents/{agent}.md"));
        }

        Assert.Contains(
            "model: \"grok-4.5\"",
            run.File("plugins/squad/.cursor-plugin/agents/squad-implementer.md").Text,
            StringComparison.Ordinal);
        Assert.Contains(
            "model: \"composer-2\"",
            run.File("plugins/squad/.cursor-plugin/agents/squad-planner.md").Text,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Official openai/plugins classifies coding/workflow packs as
    /// <c>Developer Tools</c> (superpowers, github, plugin-eval) and audit packs
    /// as <c>Security</c> (codex-security). <c>Productivity</c> is for Linear /
    /// Notion / calendar apps. Claude and Copilot catalogs have no Codex category
    /// field; Cursor catalog entries still cannot carry one.
    /// </summary>
    [Fact]
    public void Codex_catalog_uses_official_developer_tools_taxonomy()
    {
        using var repo = ShippedPlugins();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var expected = PluginNames.ToDictionary(
            name => name,
            name => name == "security" ? CodexCategory.Security : CodexCategory.DeveloperTools,
            StringComparer.Ordinal);

        var codexEntries = run.File(".agents/plugins/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .ToDictionary(entry => entry["name"]!.GetValue<string>(), StringComparer.Ordinal);
        var claudeEntries = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .ToDictionary(entry => entry["name"]!.GetValue<string>(), StringComparer.Ordinal);
        var copilotEntries = run.File(".github/plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .ToDictionary(entry => entry["name"]!.GetValue<string>(), StringComparer.Ordinal);
        var cursorEntries = run.File(".cursor-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .ToArray();

        foreach (var (name, category) in expected)
        {
            Assert.Equal(category, codexEntries[name]["category"]!.GetValue<string>());
            Assert.Equal(
                category,
                run.File($"plugins/{name}/.codex-plugin/plugin.json")
                    .Content["interface"]!["category"]!.GetValue<string>());
            Assert.Null(claudeEntries[name]["category"]);
            Assert.Null(copilotEntries[name]["category"]);
        }

        Assert.All(cursorEntries, entry => Assert.Null(entry["category"]));
        Assert.Equal(CodexCategory.DeveloperTools, CodexCategory.From("squad"));
        Assert.Equal(CodexCategory.Security, CodexCategory.From("security"));
    }

    /// <summary>
    /// Official Cursor packs set <c>displayName</c> on <c>.cursor-plugin/plugin.json</c>
    /// (schemas/plugin.schema.json). Claude marketplace entries use the same titles
    /// (v2.1.143+). Copilot's catalog schema has no display-name field.
    /// Cursor catalog entries stay kebab-case; the per-plugin manifest is the title
    /// Customize shows.
    /// </summary>
    [Fact]
    public void Cursor_plugin_manifests_set_title_case_display_name()
    {
        using var repo = ShippedPlugins();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["dotnet"] = ".NET",
            ["git"] = "Git",
            ["pack-check"] = "Pack Check",
            ["rust"] = "Rust",
            ["security"] = "Security",
            ["squad"] = "Squad",
            ["typescript"] = "TypeScript"
        };

        var claudeEntries = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .ToDictionary(entry => entry["name"]!.GetValue<string>(), StringComparer.Ordinal);
        var copilotEntries = run.File(".github/plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .ToDictionary(entry => entry["name"]!.GetValue<string>(), StringComparer.Ordinal);

        foreach (var (name, displayName) in expected)
        {
            Assert.Equal(
                displayName,
                run.File($"plugins/{name}/.cursor-plugin/plugin.json")
                    .Content["displayName"]!.GetValue<string>());
            Assert.Equal(
                displayName,
                run.File($"plugins/{name}/.codex-plugin/plugin.json")
                    .Content["interface"]!["displayName"]!.GetValue<string>());
            Assert.Equal(displayName, claudeEntries[name]["displayName"]!.GetValue<string>());
            Assert.Null(copilotEntries[name]["displayName"]);
        }

        Assert.Equal("Pack Check", PluginDisplayName.From("pack-check"));
        Assert.Equal("Squad", PluginDisplayName.From("squad"));
        Assert.Equal(".NET", PluginDisplayName.From("dotnet"));
        Assert.Equal("TypeScript", PluginDisplayName.From("typescript"));
        Assert.Equal("Security", PluginDisplayName.From("security"));
    }

    /// <summary>
    /// Reviewer named proof. No twin of closed #27 / #29 / #30: this is the sole
    /// Cursor catalog track. Generation emits one root catalog; no plugin-local
    /// or extra <c>marketplace.json</c>; one <c>CursorMarketplaceGenerator</c>.
    /// </summary>
    [Fact]
    public void Cursor_catalog_is_the_sole_catalog_track()
    {
        using var repo = ShippedPlugins();
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var catalogs = run.Generated
            .Select(file => file.RelativePath.Replace('\\', '/'))
            .Where(path => path.EndsWith("marketplace.json", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            [
                ".agents/plugins/marketplace.json",
                ".claude-plugin/marketplace.json",
                ".cursor-plugin/marketplace.json",
                ".github/plugin/marketplace.json"
            ],
            catalogs);
        Assert.DoesNotContain(
            run.Generated,
            file => file.RelativePath.Replace('\\', '/').Contains("/.cursor-plugin/marketplace.json", StringComparison.Ordinal));

        var root = TestRepository.SourceRoot();
        Assert.DoesNotContain(
            Directory.GetFiles(root, "marketplace.json", SearchOption.AllDirectories),
            path => !TestRepository.IsGeneratedPath(path));
        Assert.Equal(
            ["CursorMarketplaceGenerator.cs"],
            Directory.GetFiles(Path.Combine(root, "tools"), "*CursorMarketplace*", SearchOption.AllDirectories)
                .Where(path => !TestRepository.IsGeneratedPath(path))
                .Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray());

        var pipeline = File.ReadAllText(Path.Combine(root, "tools", "AgentPacks.Cli", "Commands", "Pipeline.cs"));
        Assert.Equal(1, CountToken(pipeline, "new CursorMarketplaceGenerator"));
        Assert.Contains(
            ".cursor-plugin/marketplace.json",
            File.ReadAllText(Path.Combine(root, ".github", "workflows", "publish-marketplace.yml")),
            StringComparison.Ordinal);
    }

    private static TestRepository ShippedPlugins()
    {
        var root = TestRepository.SourceRoot();
        var repo = new TestRepository();

        foreach (var name in PluginNames)
        {
            var manifestText = File.ReadAllText(Path.Combine(root, "plugins", name, "plugin.json"));
            repo.WithPlugin(name, manifestText);

            var keywords = JsonNode.Parse(manifestText)?["keywords"]?.AsArray()
                .Select(node => node!.GetValue<string>())
                .ToHashSet(StringComparer.Ordinal)
                ?? [];

            if (keywords.Contains(LanguagePackContract.Keyword))
            {
                foreach (var slot in LanguagePackContract.RequiredSlots)
                    repo.WithLoopSkill(LanguagePackContract.SkillName(name, slot), name);
            }
            else
            {
                repo.WithSkill("probe", plugin: name);
            }
        }

        return repo;
    }

    private static int CountToken(string text, string token)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0; index += token.Length)
            count++;
        return count;
    }

}
