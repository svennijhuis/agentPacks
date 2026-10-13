using System.Text.Json.Nodes;
using AgentPacks.Cli.Importing;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Squad inventory, frontmatter, and generator contracts. Prompt wording is reviewed,
/// not asserted. Parsers of structured markdown live in <see cref="VerificationEvidenceTests"/>
/// and <see cref="PlanLintTests"/>.
/// </summary>
public class SquadContractTests
{
    private const string Manifest = """
        {
          "$schema": "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
          "name": "squad",
          "description": "Test Squad."
        }
        """;

    private static readonly string[] AgentNames = TestRepository.SquadAgentNames;

    private static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "squad", name));

    [Fact]
    public void All_nine_agents_are_preserved_in_every_generated_agent_client()
    {
        using var repo = new TestRepository().WithPlugin("squad", Manifest);
        foreach (var agent in AgentNames)
            repo.WithFile($"plugins/squad/agents/{agent}.md", Fixture($"{agent}.md"));

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.Equal(9, AgentNames.Length);
        foreach (var agent in AgentNames)
        {
            Assert.Equal(agent, TestRepository.ParseFrontmatter(Fixture($"{agent}.md")).Scalar("name"));
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/squad/com.anthropic.claude-code/agents/{agent}.md").Text));
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/squad/com.openai.codex/agents/{agent}.toml").Text));
            Assert.False(string.IsNullOrWhiteSpace(run.File($"plugins/squad/com.github.copilot/agents/{agent}.agent.md").Text));
        }
    }

    [Fact]
    public void Squad_commands_are_exactly_squad_and_review()
    {
        var directory = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands");
        var names = Directory.GetFiles(directory, "*.md")
            .Select(path => Path.GetFileName(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["scenarios.md", "squad-review.md", "squad.md"], names);
        Assert.Equal("squad-review", TestRepository.ParseFrontmatter(File.ReadAllText(Path.Combine(directory, "squad-review.md"))).Scalar("name"));
        Assert.Equal("squad", TestRepository.ParseFrontmatter(File.ReadAllText(Path.Combine(directory, "squad.md"))).Scalar("name"));
        Assert.Equal("scenarios", TestRepository.ParseFrontmatter(File.ReadAllText(Path.Combine(directory, "scenarios.md"))).Scalar("name"));
        Assert.False(File.Exists(Path.Combine(directory, "review.md")));
        Assert.False(File.Exists(Path.Combine(directory, "http-scenarios.md")));
    }

    [Fact]
    public void Two_user_invoked_entrypoints_are_squad_and_review()
    {
        var skill = TestRepository.ParseFrontmatter(Fixture("SKILL.md"));
        Assert.Equal("true", skill.Scalar("disable-model-invocation"));
        Assert.Equal("false", skill.Scalar("user-invocable"));
        Assert.Equal("squad", skill.Scalar("name"));
        Squad_commands_are_exactly_squad_and_review();
    }

    [Fact]
    public void User_facing_surfaces_do_not_say_delivery_loop()
    {
        var root = TestRepository.SourceRoot();
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "delivery-loop")));

        var leftovers = Directory.GetDirectories(Path.Combine(root, "plugins"))
            .Select(path => Path.GetFileName(path) ?? path)
            .Where(name => name.Contains("delivery", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        Assert.Empty(leftovers);

        var commands = Directory.GetFiles(Path.Combine(root, "plugins", "squad", "commands"), "*.md")
            .Select(path => Path.GetFileName(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["scenarios.md", "squad-review.md", "squad.md"], commands);
        Assert.DoesNotContain(commands, name => name.Contains("delivery", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void No_user_facing_loop_star_agents_are_squad_star()
    {
        var agentsDir = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "agents");
        var names = Directory.GetFiles(agentsDir, "*.md")
            .Select(path => Path.GetFileNameWithoutExtension(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
        [
            "squad-architect",
            "squad-implementer",
            "squad-orchestrator",
            "squad-planner",
            "squad-retro",
            "squad-reviewer",
            "squad-security-reviewer",
            "squad-simplifier",
            "squad-verifier"
        ], names);
        foreach (var name in names)
        {
            Assert.StartsWith("squad-", name, StringComparison.Ordinal);
            Assert.Equal(name, TestRepository.ParseFrontmatter(File.ReadAllText(Path.Combine(agentsDir, $"{name}.md"))).Scalar("name"));
        }

        Assert.False(File.Exists(Path.Combine(agentsDir, "loop-planner.md")));
        Assert.DoesNotContain(AgentNames, name => name.StartsWith("loop-", StringComparison.Ordinal));
    }

    [Fact]
    public void Loop_agent_bodies_stay_tiny()
    {
        foreach (var agent in AgentNames)
        {
            var lines = TestRepository.NonEmptyBodyLines(Fixture($"{agent}.md"));
            var cap = agent == "squad-security-reviewer" ? 44 : 32;
            Assert.True(lines <= cap, $"{agent} body is {lines} lines; cap is {cap}.");
        }
    }

    [Fact]
    public void Architect_is_read_only_frontier_and_planner_cannot_run_shell()
    {
        var architect = TestRepository.ParseFrontmatter(Fixture("squad-architect.md"));
        Assert.Equal("frontier", architect.Scalar("model"));
        Assert.Equal("true", architect.Scalar("readonly"));
        var tools = Fixture("squad-architect.md");
        foreach (var tool in new[] { "write", "edit", "bash", "websearch" })
            Assert.DoesNotContain($"\n  - {tool}\n", tools, StringComparison.Ordinal);
        var frontier = AgentNames
            .Where(name => TestRepository.ParseFrontmatter(Fixture($"{name}.md")).Scalar("model") == "frontier")
            .ToArray();
        Assert.Equal(["squad-architect"], frontier);

        var planner = Fixture("squad-planner.md");
        Assert.Contains("\n  - write\n", planner, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  - bash\n", planner, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  - edit\n", planner, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("squad.md")]
    [InlineData("squad-review.md")]
    public void Each_entrypoint_ends_by_spawning_the_retro(string command)
    {
        var body = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands", command));
        var lastStep = body.Split('\n').Last(line => System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\. "));
        Assert.Contains("Spawn `squad-retro` once", lastStep, StringComparison.Ordinal);
    }

    [Fact]
    public void Retro_also_runs_on_stop_or_fail_but_never_on_a_small_change()
    {
        var root = TestRepository.SourceRoot();
        var squad = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad.md"));
        var review = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad-review.md"));
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        Assert.Contains("A full run that stops or fails at any step also ends with this `squad-retro` spawn, passing the interruptions.", squad, StringComparison.Ordinal);
        Assert.Contains("A review that stops early also ends with this `squad-retro` spawn, passing the interruptions.", review, StringComparison.Ordinal);
        Assert.Contains("A small change gets no retro.", squad, StringComparison.Ordinal);
        Assert.Contains("Small change: spawn none of these; no retro.", skill, StringComparison.Ordinal);
    }

    [Fact]
    public void No_squad_text_forbids_writing_the_draft_plan_before_confirmation()
    {
        var root = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad");
        var banned = new System.Text.RegularExpressions.Regex(
            @"(do not|don't|never) (create|write) the plan before confirm",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (var file in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
            Assert.False(banned.IsMatch(File.ReadAllText(file)), file);
        Assert.Contains("writes the draft plan .md from round 1, then finalizes it after confirmation",
            File.ReadAllText(Path.Combine(root, "commands", "squad.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Next_run_catches_up_a_retro_for_an_interrupted_draft_plan()
    {
        var root = TestRepository.SourceRoot();
        var squad = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad.md"));
        var retro = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "squad", "references", "retro.md"));
        Assert.Contains("still `Status: draft` and no `docs/retros/` file has its date-slug (with or without `-interrupted`), spawn `squad-retro` on that plan with reason `interrupted` before anything else", squad, StringComparison.Ordinal);
        Assert.Contains("## Catch-up", retro, StringComparison.Ordinal);
    }

    [Fact]
    public void Planner_has_write_in_all_four_generated_clients_and_read_only_agents_stay_read_only()
    {
        using var repo = new TestRepository().WithPlugin("squad", Manifest);
        foreach (var agent in AgentNames)
            repo.WithFile($"plugins/squad/agents/{agent}.md", Fixture($"{agent}.md"));
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.Contains("\"Write\"", run.File("plugins/squad/com.anthropic.claude-code/agents/squad-planner.md").Text, StringComparison.Ordinal);
        Assert.Contains("\"write\"", run.File("plugins/squad/com.github.copilot/agents/squad-planner.agent.md").Text, StringComparison.Ordinal);
        var cursor = run.File("plugins/squad/.cursor-plugin/agents/squad-planner.md").Text;
        Assert.Contains("\"write\"", cursor, StringComparison.Ordinal);
        Assert.DoesNotContain("readonly: true", cursor, StringComparison.Ordinal);
        Assert.Contains("sandbox_mode = \"workspace-write\"", run.File("plugins/squad/com.openai.codex/agents/squad-planner.toml").Text, StringComparison.Ordinal);
        foreach (var agent in new[] { "squad-architect", "squad-reviewer", "squad-verifier" })
            Assert.Contains("sandbox_mode = \"read-only\"", run.File($"plugins/squad/com.openai.codex/agents/{agent}.toml").Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Architect_flags_a_missing_vendor_project_and_maps_existing_infrastructure()
    {
        var contract = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(),
            "plugins", "squad", "skills", "squad", "references", "architect-contract.md"));
        var flags = contract[contract.IndexOf("## Red flags", StringComparison.Ordinal)..]
            .Split('\n').Count(line => System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\. "));
        Assert.Equal(9, flags);
        Assert.Contains("9. Missing vendor project:", contract, StringComparison.Ordinal);
        Assert.Contains("| Existing `Infrastructure.*` projects |", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void Planner_defers_to_design_and_implementer_spawn_names_the_plan_path()
    {
        var root = TestRepository.SourceRoot();
        Assert.Contains("When `## Design` exists, skip the shape compare and the architecture fitness check; write `see ## Design` for both.",
            Fixture("squad-planner.md"), StringComparison.Ordinal);
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        var squad = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad.md"));
        Assert.Contains("spawn passes the slice name and the plan path;", skill, StringComparison.Ordinal);
        Assert.Contains("passes the slice name (or `whole-plan`) and the plan path;", squad, StringComparison.Ordinal);
    }

    [Fact]
    public void Squad_loop_slash_commands_stay_exactly_four()
    {
        var root = TestRepository.SourceRoot();
        var commands = new[] { "squad", "pack-check" }
            .SelectMany(plugin => Directory.GetFiles(Path.Combine(root, "plugins", plugin, "commands"), "*.md"))
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["pack-check", "scenarios", "squad", "squad-review"], commands);
    }

    [Fact]
    public void Loop_agents_keep_role_tiers()
    {
        Assert.Equal("standard", TestRepository.ParseFrontmatter(Fixture("squad-implementer.md")).Scalar("model"));
        Assert.Equal("standard", TestRepository.ParseFrontmatter(Fixture("squad-planner.md")).Scalar("model"));
        Assert.Equal("true", TestRepository.ParseFrontmatter(Fixture("squad-simplifier.md")).Scalar("readonly"));
        foreach (var agent in AgentNames.Where(name => name is not ("squad-implementer" or "squad-planner" or "squad-architect")))
        {
            Assert.Equal("standard", TestRepository.ParseFrontmatter(Fixture($"{agent}.md")).Scalar("model"));
        }

        foreach (var agent in AgentNames.Where(name => name != "squad-architect"))
        {
            Assert.NotEqual("frontier", TestRepository.ParseFrontmatter(Fixture($"{agent}.md")).Scalar("model"));
        }
    }

    [Fact]
    public void Empty_mcp_scaffolds_test_is_not_named_dotnet_only()
    {
        Plugin_mcp_files_are_empty_scaffolds();
    }

    [Fact]
    public void Retro_agent_is_standard_without_shell_and_squad_ships_two_skills()
    {
        var root = TestRepository.SourceRoot();
        var skills = Directory.GetDirectories(Path.Combine(root, "plugins", "squad", "skills"))
            .Select(path => Path.GetFileName(path)).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(["scenarios-md", "squad"], skills);
        var retro = TestRepository.ParseFrontmatter(Fixture("squad-retro.md"));
        Assert.Equal("squad-retro", retro.Scalar("name"));
        Assert.Equal("standard", retro.Scalar("model"));
        Assert.Contains("\n  - write\n", Fixture("squad-retro.md"), StringComparison.Ordinal);
        Assert.Contains(
            "Writes exactly one file, `docs/retros/<date-slug>.md` (`<date-slug>-interrupted.md` for a catch-up), and never edits skills, standards, or code.",
            retro.Body, StringComparison.Ordinal);
        foreach (var tool in new[] { "edit", "bash" })
            Assert.DoesNotContain($"\n  - {tool}\n", Fixture("squad-retro.md"), StringComparison.Ordinal);
        Squad_commands_are_exactly_squad_and_review();
    }

    [Fact]
    public void Suggestions_contract_is_human_apply_only_and_excluded_from_review()
    {
        var root = TestRepository.SourceRoot();
        var suggestionsPath = Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "suggestions.md");
        var skill = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        var review = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "review-contract.md"));
        var retro = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "retro.md"));
        var squad = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad.md"));
        var squadReview = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "commands", "squad-review.md"));
        var worktree = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "worktree.md"));
        var suggestions = File.ReadAllText(suggestionsPath);

        // Inventory: reference exists, skill href, no suggestions slash, pathspec excludes.
        Assert.True(File.Exists(suggestionsPath));
        Assert.Contains("references/suggestions.md", skill, StringComparison.Ordinal);
        Assert.Contains("[suggestions](suggestions.md)", retro, StringComparison.Ordinal);
        Assert.Contains(":(exclude)docs/suggestions.md", review, StringComparison.Ordinal);
        Assert.Contains(":(exclude)docs/security-audit", review, StringComparison.Ordinal);
        Assert.False(
            File.Exists(Path.Combine(root, "plugins", "squad", "commands", "suggestions.md")),
            "suggestions is a contract, not a slash command");

        // Shared write-gate noun set across slash · worktree · SKILL · contract · retro · review (no drift).
        const string Gate = "pack/skill/agent/command/contract";
        Assert.Contains(Gate, squad, StringComparison.Ordinal);
        Assert.Contains(Gate, squadReview, StringComparison.Ordinal);
        Assert.Contains(Gate, worktree, StringComparison.Ordinal);
        Assert.Contains(Gate, skill, StringComparison.Ordinal);
        Assert.Contains(Gate, suggestions, StringComparison.Ordinal);
        Assert.Contains(Gate, retro, StringComparison.Ordinal);
        Assert.Contains(Gate, review, StringComparison.Ordinal);
        Assert.Contains("`pack` / `skill` / `agent` / `command` / `contract`", suggestions, StringComparison.Ordinal);
    }

    [Fact]
    public void Squad_skill_is_not_user_invocable_command_is_the_only_slash()
    {
        Two_user_invoked_entrypoints_are_squad_and_review();
        Assert.False(File.Exists(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "commands", "squad-skill.md")));
    }

    [Fact]
    public void Squad_and_squad_review_command_blurbs_are_short_user_friendly()
    {
        Assert.Equal(
            "Plan → build → verify → review (gated). Uncommitted hand-off.",
            TestRepository.ParseFrontmatter(Fixture("squad.md")).Scalar("description"));
        Assert.Equal(
            "Report-only review of a PR / uncommitted / vs main.",
            TestRepository.ParseFrontmatter(Fixture("squad-review.md")).Scalar("description"));
        Assert.Equal(
            "From changed code, write docs/smoke/<slug>.md. No product-code edits.",
            TestRepository.ParseFrontmatter(File.ReadAllText(
                Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands", "scenarios.md")))
                .Scalar("description"));
        Squad_commands_are_exactly_squad_and_review();
    }

    [Fact]
    public void Claude_package_ships_one_squad_and_one_squad_review_command()
    {
        using var repo = new TestRepository()
            .WithPlugin(
                "squad",
                File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "plugin.json")))
            .WithCopiedDirectory(TestRepository.SourceRoot(), "plugins/squad/commands", "*.md");

        repo.WithSkill(
            "squad",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var entry = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .Single(plugin => plugin["name"]!.GetValue<string>() == "squad");

        Assert.True(entry["strict"]!.GetValue<bool>());
        Assert.Null(entry["version"]);
        Assert.Equal("Squad", entry["displayName"]!.GetValue<string>());
        Assert.Equal("./com.anthropic.claude-code/commands/", entry["commands"]![0]!.GetValue<string>());

        var pluginDirectory = repo.PluginDirectory("squad");
        var names = DiscoverableClaudeCommandNames(pluginDirectory, entry);
        Assert.Equal(["scenarios", "squad", "squad-review"], names.OrderBy(name => name, StringComparer.Ordinal));

        var rootNames = CommandNames(Path.Combine(pluginDirectory, "commands"));
        var claudeNames = CommandNames(
            Path.Combine(pluginDirectory, "com.anthropic.claude-code", "commands"));
        Assert.Equal(rootNames, claudeNames);
        Assert.Equal(claudeNames.Count, names.Count);

        var twins = (JsonObject)entry.DeepClone();
        twins["strict"] = false;
        var loose = DiscoverableClaudeCommandNames(pluginDirectory, twins);
        Assert.True(
            loose.Count > names.Count,
            "root commands/ and com.anthropic.claude-code/commands/ must still both exist; strict is what hides the Cursor twin.");
        Assert.Equal(2, loose.Count(name => name == "squad"));
    }

    [Fact]
    public void Copilot_factory_command_name_differs_from_plugin_name()
    {
        var root = TestRepository.SourceRoot();
        using var repo = new TestRepository()
            .WithPlugin(
                "squad",
                File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")))
            .WithCopiedDirectory(root, "plugins/squad/commands", "*.md");

        repo.WithSkill(
            "squad",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");

        repo.WithPlugin(
            "pack-check",
            File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "plugin.json")));
        repo.WithFile(
            "plugins/pack-check/commands/pack-check.md",
            File.ReadAllText(Path.Combine(root, "plugins", "pack-check", "commands", "pack-check.md")));
        repo.WithSkill("pack-check", plugin: "pack-check");

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var pluginName = JsonNode.Parse(
            File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")))!
            ["name"]!.GetValue<string>();
        Assert.Equal("squad", pluginName);

        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/run.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/squad-review.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/squad.md"));

        var factory = run.File("plugins/squad/com.github.copilot/commands/run.md").Text;
        Assert.Contains("name: \"run\"", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", factory, StringComparison.Ordinal);
        Assert.NotEqual("run", pluginName);

        var copilotNames = CommandNames(
            Path.Combine(repo.PluginDirectory("squad"), "com.github.copilot", "commands"));
        Assert.Equal(
            new HashSet<string>(["run", "scenarios", "squad-review"], StringComparer.Ordinal),
            copilotNames);

        var claudeEntry = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .Single(plugin => plugin["name"]!.GetValue<string>() == "squad");
        var claudeNames = DiscoverableClaudeCommandNames(repo.PluginDirectory("squad"), claudeEntry);
        Assert.Equal(["scenarios", "squad", "squad-review"], claudeNames.OrderBy(name => name, StringComparer.Ordinal));
        Assert.DoesNotContain("run", claudeNames);
        Assert.Equal(1, claudeNames.Count(name => name == "squad"));
    }

    [Fact]
    public void Caveman_external_pins_and_squad_invokes_by_exact_name()
    {
        const string sha = "5184b3d11ac6a1acb7d44b9bfaa31698157cff97";
        var root = TestRepository.SourceRoot();
        var manifest = JsonNode.Parse(File.ReadAllText(
            Path.Combine(root, "plugins", "squad", "external-skills.json")))!;
        var sources = manifest["sources"]!.AsArray();
        Assert.Equal(2, sources.Count);

        var byName = sources.OfType<JsonObject>().ToDictionary(
            entry => entry["name"]!.GetValue<string>(),
            StringComparer.Ordinal);
        Assert.Equal(["caveman", "caveman-compress"],
            byName.Keys.OrderBy(name => name, StringComparer.Ordinal));

        foreach (var (name, path) in new[]
                 {
                     ("caveman", "skills/caveman"),
                     ("caveman-compress", "skills/caveman-compress")
                 })
        {
            var entry = byName[name];
            Assert.Equal("https://github.com/JuliusBrussee/caveman", entry["repository"]!.GetValue<string>());
            Assert.Equal(path, entry["path"]!.GetValue<string>());
            Assert.Equal(sha, entry["commit"]!.GetValue<string>());
            Assert.Equal("MIT", entry["license"]!.GetValue<string>());
        }

        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "caveman")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "caveman-compress")));
    }

    [Fact]
    public void Caveman_pins_are_not_user_invocable()
    {
        var root = TestRepository.SourceRoot();
        var catalog = File.ReadAllText(Path.Combine(root, "plugins", "squad", "external-skills.json"));
        var names = JsonNode.Parse(catalog)!["sources"]!.AsArray()
            .OfType<JsonObject>()
            .Select(entry => entry["name"]!.GetValue<string>())
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["caveman", "caveman-compress"], names);
        Assert.DoesNotContain("grill-me", catalog, StringComparison.Ordinal);

        foreach (var name in names)
        {
            var fetched = Path.Combine(Path.GetTempPath(), "agentpacks-caveman-fetched", Guid.NewGuid().ToString("N"));
            var target = Path.Combine(Path.GetTempPath(), "agentpacks-caveman-target", Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(fetched);
                var extra = name == "caveman-compress" ? "user-invocable: true\n" : string.Empty;
                File.WriteAllText(
                    Path.Combine(fetched, "SKILL.md"),
                    $"---\nname: {name}\ndescription: Compressed communication.\n{extra}---\n\nStand-in pin.\n");

                ExternalSourceMaterializer.InstallFetchedSkill(
                    fetched,
                    target,
                    new ExternalSourceEntry
                    {
                        Name = name,
                        Repository = "https://github.com/JuliusBrussee/caveman",
                        Path = $"skills/{name}",
                        Commit = "5184b3d11ac6a1acb7d44b9bfaa31698157cff97",
                        License = "MIT",
                        PluginDirectory = Path.Combine(root, "plugins", "squad")
                    });

                var skill = TestRepository.ParseFrontmatter(File.ReadAllText(Path.Combine(target, "SKILL.md")));
                Assert.Equal("false", skill.Scalar("user-invocable"));
            }
            finally
            {
                if (Directory.Exists(fetched)) Directory.Delete(fetched, recursive: true);
                if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
            }
        }
    }

    [Fact]
    public void Pull_request_ci_stays_one_job_no_matrix()
    {
        var root = TestRepository.SourceRoot();
        var workflows = Directory.GetFiles(Path.Combine(root, ".github", "workflows"), "*.yml");
        Assert.Equal(
            ["drift.yml", "promote-marketplace.yml", "publish-marketplace.yml", "validate.yml"],
            workflows.Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));

        foreach (var path in workflows)
        {
            var name = Path.GetFileName(path) ?? path;
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("strategy:", text, StringComparison.Ordinal);
            Assert.DoesNotContain("matrix:", text, StringComparison.Ordinal);
            if (!name.Equals("validate.yml", StringComparison.Ordinal))
                Assert.DoesNotContain("pull_request:", text, StringComparison.Ordinal);
        }

        var validate = File.ReadAllText(Path.Combine(root, ".github", "workflows", "validate.yml"));
        Assert.Contains("pull_request:", validate, StringComparison.Ordinal);
        Assert.Equal(1, TestRepository.CountOccurrences(validate, "runs-on:"));
        Assert.Contains("dotnet test", validate, StringComparison.Ordinal);
        Assert.Contains("validate-all --out", validate, StringComparison.Ordinal);
    }

    /// <summary>
    /// A push to main publishes <c>marketplace-beta</c>. Stable <c>marketplace</c> moves only
    /// when a maintainer copies that exact generated commit; the promote job does not regenerate.
    /// Pin the SHA testers installed. Force-push publish history is not ancestor-checked.
    /// </summary>
    [Fact]
    public void Main_publishes_beta_marketplace_and_promote_copies_it()
    {
        var root = TestRepository.SourceRoot();
        var publish = File.ReadAllText(Path.Combine(root, ".github", "workflows", "publish-marketplace.yml"));
        var promote = File.ReadAllText(Path.Combine(root, ".github", "workflows", "promote-marketplace.yml"));
        var drift = File.ReadAllText(Path.Combine(root, ".github", "workflows", "drift.yml"));
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));

        Assert.Contains("MARKETPLACE_BRANCH: marketplace-beta", publish, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "MARKETPLACE_BRANCH: marketplace\n",
            publish.Replace("\r\n", "\n"),
            StringComparison.Ordinal);
        Assert.Contains("branches:\n      - main", publish.Replace("\r\n", "\n"), StringComparison.Ordinal);

        Assert.Contains("workflow_dispatch:", promote, StringComparison.Ordinal);
        Assert.Contains("BETA_BRANCH: marketplace-beta", promote, StringComparison.Ordinal);
        Assert.Contains(
            "MARKETPLACE_BRANCH: marketplace\n",
            promote.Replace("\r\n", "\n"),
            StringComparison.Ordinal);
        Assert.DoesNotContain("MARKETPLACE_BRANCH: marketplace-beta", promote, StringComparison.Ordinal);
        Assert.Contains("chore: generate plugin marketplace from", publish, StringComparison.Ordinal);
        Assert.Contains("chore: generate plugin marketplace from", promote, StringComparison.Ordinal);
        Assert.Contains("git fetch --no-tags origin \"$REQUESTED\"", promote, StringComparison.Ordinal);
        Assert.DoesNotContain("merge-base --is-ancestor", promote, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet ", promote, StringComparison.Ordinal);

        Assert.Contains("marketplace marketplace-beta", drift, StringComparison.Ordinal);
        Assert.Contains("#marketplace-beta", readme, StringComparison.Ordinal);
        Assert.Contains("Promote beta marketplace to stable", readme, StringComparison.Ordinal);
        Assert.Contains("git#marketplace", readme, StringComparison.Ordinal);
        Assert.Contains("pin the SHA testers installed", readme, StringComparison.Ordinal);
        Assert.Contains("not an ancestor of current beta HEAD", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Test_plan_matrix_seam_column_blank_or_unnamed_internal_is_not_verified()
    {
        new VerificationEvidenceTests().Blank_or_unconfirmed_seam_and_unnamed_internal_hits_are_not_verified();
    }

    [Fact]
    public void Owasp_2025_categories_live_in_the_review_contract()
    {
        var contract = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(),
            "plugins", "squad", "skills", "squad", "references", "review-contract.md"));
        var agent = Fixture("squad-security-reviewer.md");

        Assert.Contains("## Security gate", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("A01_2025-Broken_Access_Control", agent, StringComparison.Ordinal);
        Assert.Contains("Security gate", agent, StringComparison.Ordinal);

        string[] ids =
        [
            "A01_2025-Broken_Access_Control",
            "A02_2025-Security_Misconfiguration",
            "A03_2025-Software_Supply_Chain_Failures",
            "A04_2025-Cryptographic_Failures",
            "A05_2025-Injection",
            "A06_2025-Insecure_Design",
            "A07_2025-Authentication_Failures",
            "A08_2025-Software_or_Data_Integrity_Failures",
            "A09_2025-Security_Logging_and_Alerting_Failures",
            "A10_2025-Mishandling_of_Exceptional_Conditions"
        ];
        foreach (var id in ids)
        {
            Assert.Contains(id, contract, StringComparison.Ordinal);
            Assert.Contains($"https://owasp.org/Top10/2025/{id}/", contract, StringComparison.Ordinal);
        }

        var squad = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "commands", "squad.md"));
        var review = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "commands", "squad-review.md"));
        foreach (var command in new[] { squad, review })
        {
            Assert.Contains("auth, untrusted input, files, shell, crypto, dependencies, credentials, or exceptional conditions", command, StringComparison.Ordinal);
            Assert.Contains("Do not launch a full-repo audit from this review.", command, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Review_agents_have_anti_reentry_on_every_provider_tree()
    {
        const string line = "Do not re-invoke review or the orchestrator.";
        var agents = new[] { "squad-reviewer", "squad-simplifier", "squad-security-reviewer" };

        foreach (var agent in agents)
            Assert.Contains(line, Fixture($"{agent}.md"), StringComparison.Ordinal);

        Assert.DoesNotContain(line, Fixture("squad-orchestrator.md"), StringComparison.Ordinal);
        Assert.Contains(
            "Do not launch, retry, or hand work to another agent",
            Fixture("squad-orchestrator.md"),
            StringComparison.Ordinal);

        using var repo = new TestRepository().WithPlugin("squad", Manifest);
        foreach (var agent in AgentNames)
            repo.WithFile($"plugins/squad/agents/{agent}.md", Fixture($"{agent}.md"));

        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        foreach (var agent in agents)
        {
            foreach (var generated in new[]
            {
                $"plugins/squad/com.anthropic.claude-code/agents/{agent}.md",
                $"plugins/squad/com.openai.codex/agents/{agent}.toml",
                $"plugins/squad/com.github.copilot/agents/{agent}.agent.md",
                $"plugins/squad/.cursor-plugin/agents/{agent}.md"
            })
            {
                Assert.Contains(line, run.File(generated).Text, StringComparison.Ordinal);
            }
        }

        Loop_agent_bodies_stay_tiny();
    }

    [Fact]
    public void Still_five_user_commands()
    {
        new HttpScenariosContractTests().User_commands_stay_squad_squad_review_pack_check_plus_http_scenarios();
        Squad_commands_are_exactly_squad_and_review();
        Assert.False(Directory.Exists(Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "commands", "squad-review")));
    }

    [Fact]
    public void Squad_skill_progressive_disclosure_moves_four_bodies_to_references()
    {
        var skill = Fixture("SKILL.md");
        var body = TestRepository.ParseFrontmatter(skill).Body;
        foreach (var href in new[]
                 {
                     "references/malformed-reask.md",
                     "references/residual-fixup.md",
                     "references/worktree.md",
                     "references/advisor-lite.md"
                 })
        {
            Assert.Contains(href, body, StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(
                TestRepository.SourceRoot(), "plugins", "squad", "skills", "squad", href.Replace('/', Path.DirectorySeparatorChar))),
                href);
        }

        Squad_skill_body_is_not_grown();
    }

    [Fact]
    public void Squad_skill_body_is_not_grown()
    {
        var lines = TestRepository.NonEmptyBodyLines(Fixture("SKILL.md"));
        Assert.True(lines <= 87, $"squad SKILL.md body is {lines} nonempty lines; cap is 87.");
    }

    [Fact]
    public void Lang_test_patterns_local_vs_deployed_smoke_examples()
    {
        var root = TestRepository.SourceRoot();
        foreach (var pack in new[] { "dotnet", "typescript", "rust" })
        {
            var skillDir = Path.Combine(root, "plugins", pack, "skills", $"{pack}-test-patterns");
            Assert.True(File.Exists(Path.Combine(skillDir, "references", "examples", "deployed-smoke.md")));
            var skill = TestRepository.ParseFrontmatter(File.ReadAllText(Path.Combine(skillDir, "SKILL.md")));
            Assert.Contains("references/examples/deployed-smoke.md", skill.Body, StringComparison.Ordinal);

            var examples = Path.Combine(skillDir, "references", "examples");
            Assert.True(
                File.Exists(Path.Combine(examples, "http-integration.md"))
                || File.Exists(Path.Combine(examples, "integration.md")),
                $"{pack} is missing a local integration example");

            var catalog = JsonNode.Parse(File.ReadAllText(
                Path.Combine(root, "plugins", pack, "standards.source.json")))!;
            var mapped = catalog["consumers"]![$"{pack}-test-patterns"]!.AsArray()
                .Select(value => value!.GetValue<string>())
                .ToArray();
            Assert.Equal(["testing", "testing-principles"], mapped);
        }
    }

    [Fact]
    public void Smoke_matrix_not_user_slash_no_new_plugin()
    {
        var root = TestRepository.SourceRoot();
        Assert.Equal(TestRepository.ShippedPluginNames, TestRepository.ShippedPluginDirectories(root));

        Squad_commands_are_exactly_squad_and_review();
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "smoke.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "squad-smoke.md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "smoke")));
        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "rules", "review-checklist.mdc")));
        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "references", "smoke-matrix.md")));
    }

    [Fact]
    public void User_facing_surfaces_say_squad_review_not_bare_review()
    {
        var commandDir = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "commands");
        Assert.False(File.Exists(Path.Combine(commandDir, "review.md")));
        Assert.False(File.Exists(Path.Combine(commandDir, "code-review.md")));
        Assert.Equal(
            "squad-review",
            TestRepository.ParseFrontmatter(File.ReadAllText(Path.Combine(commandDir, "squad-review.md"))).Scalar("name"));
        Squad_commands_are_exactly_squad_and_review();
    }

    private void Plugin_mcp_files_are_empty_scaffolds()
    {
        var root = TestRepository.SourceRoot();
        var authored = Directory.GetFiles(root, "mcp.json", SearchOption.AllDirectories)
            .Where(path => !TestRepository.IsGeneratedPath(path))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["plugins/dotnet/mcp.json", "plugins/squad/mcp.json"], authored);

        foreach (var relative in authored)
        {
            var mcp = JsonNode.Parse(File.ReadAllText(Path.Combine(root, relative)))!;
            Assert.Empty(mcp["mcpServers"]!.AsObject());
        }
    }

    private static List<string> DiscoverableClaudeCommandNames(string pluginDirectory, JsonObject entry)
    {
        var directories = new List<string>();

        switch (entry["commands"])
        {
            case JsonArray declared:
                directories.AddRange(declared
                    .Select(node => node?.GetValue<string>())
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => Path.Combine(pluginDirectory, path!.TrimStart('.', '/', '\\'))));
                break;
            case JsonValue single:
                directories.Add(Path.Combine(pluginDirectory, single.GetValue<string>().TrimStart('.', '/', '\\')));
                break;
        }

        if (entry["strict"]?.GetValue<bool>() is not true)
        {
            directories.Add(Path.Combine(pluginDirectory, "commands"));
        }

        return directories
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.GetFiles(directory, "*.md"))
            .Select(path => Path.GetFileNameWithoutExtension(path)!)
            .ToList();
    }

    private static string Squad(params string[] parts) =>
        File.ReadAllText(Path.Combine([TestRepository.SourceRoot(), "plugins", "squad", .. parts]));

    [Fact]
    public void Repo_conventions_outrank_skill_standards_unless_an_improvement_is_logged()
    {
        const string line = "A standard overrides a repo convention only as an explicit improvement, logged under `## Decisions`";
        Assert.Contains(line, Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
        Assert.Contains(line, Squad("agents", "squad-implementer.md"), StringComparison.Ordinal);
        var reviewer = Squad("agents", "squad-reviewer.md");
        Assert.Contains("A break from a repo convention without an improvement logged under `## Decisions` → `medium`", reviewer, StringComparison.Ordinal);
        Assert.Contains("a break from our standard where the repo has no convention → `medium`", reviewer, StringComparison.Ordinal);
        Assert.DoesNotContain("convention → `low`", reviewer, StringComparison.Ordinal);
    }

    [Fact]
    public void Design_gap_reruns_the_architect_at_most_once()
    {
        Assert.Contains("`Design gap: <question>`", Squad("agents", "squad-planner.md"), StringComparison.Ordinal);
        var command = Squad("commands", "squad.md");
        Assert.Contains("rerun `squad-architect` once with that question", command, StringComparison.Ordinal);
        Assert.Contains("at most 1 rerun", command, StringComparison.Ordinal);
    }

    [Fact]
    public void Plan_gap_stops_the_slice_and_routes_to_one_planner_round()
    {
        Assert.Contains("return `Plan gap: <question>` instead of guessing", Squad("agents", "squad-implementer.md"), StringComparison.Ordinal);
        var command = Squad("commands", "squad.md");
        Assert.Contains("then `squad-planner` in `next-round` mode on that gap only", command, StringComparison.Ordinal);
        Assert.Contains("`## Grill log` and `## Decisions`", command, StringComparison.Ordinal);
        Assert.Contains("a fresh `squad-implementer` redoes that slice", command, StringComparison.Ordinal);
    }

    [Fact]
    public void Second_plan_gap_on_a_slice_is_replan_and_the_retro_records_it()
    {
        Assert.Contains("a second becomes `Replan:` for the whole plan and the retro records it", Squad("commands", "squad.md"), StringComparison.Ordinal);
        Assert.Contains("A second one on the same slice becomes `Replan:` for the whole plan", Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
        Assert.Contains("second `Plan gap:` on one slice", Squad("skills", "squad", "references", "review-contract.md"), StringComparison.Ordinal);
        Assert.Contains("each `Plan gap:`", Squad("skills", "squad", "references", "retro.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Locked_v1_fence_is_identical_in_skill_and_both_readmes()
    {
        static string Fence(string text)
        {
            var start = text.IndexOf("## Locked v1 flow", StringComparison.Ordinal);
            var open = text.IndexOf("```text", start, StringComparison.Ordinal);
            var close = text.IndexOf("\n```", open + 7, StringComparison.Ordinal);
            return text[open..close].Replace("\r", "", StringComparison.Ordinal);
        }

        var skill = Fence(Squad("skills", "squad", "SKILL.md"));
        Assert.Equal(skill, Fence(Squad("README.md")));
        Assert.Equal(skill, Fence(File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "README.md"))));
    }

    [Fact]
    public void After_a_plan_gap_the_gitignored_patch_holds_the_diff_and_the_tree_is_kept()
    {
        var contract = Squad("skills", "squad", "references", "planning-contract.md");
        var start = contract.IndexOf("```sh\np=\"$(git rev-parse --git-dir)/squad/", StringComparison.Ordinal);
        Assert.True(start >= 0, "save command missing from planning-contract");
        var save = contract[(start + 6)..contract.IndexOf("\n```", start + 6, StringComparison.Ordinal)]
            .Replace("<yyyy-mm-dd>-<slug>", "2026-10-12-x", StringComparison.Ordinal).Replace("<n>", "1", StringComparison.Ordinal);
        foreach (var banned in new[] { "reset --hard", "git clean", "git checkout", "git restore" })
        {
            Assert.DoesNotContain(banned, save, StringComparison.Ordinal);
        }

        var dir = Directory.CreateTempSubdirectory("plan-gap-").FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(dir, "docs", "plans"));
            File.WriteAllText(Path.Combine(dir, "a.txt"), "base\n");
            Sh(dir, "git init -q && git add -A && git -c user.name=t -c user.email=t@t commit -qm base");
            File.WriteAllText(Path.Combine(dir, "a.txt"), "slice 1 done, uncommitted\n");
            File.WriteAllText(Path.Combine(dir, "new.txt"), "partial slice 2\n");

            Sh(dir, save);

            var patch = Path.Combine(dir, ".git", "squad", "2026-10-12-x.gap-1.patch");
            Assert.True(new FileInfo(patch).Length > 0);
            var text = File.ReadAllText(patch);
            Assert.Contains("+slice 1 done", text, StringComparison.Ordinal);
            Assert.Contains("new.txt", text, StringComparison.Ordinal);
            Assert.False(File.Exists(Path.Combine(dir, ".gitignore")), "save must not create or edit .gitignore");
            var status = Sh(dir, "git status --porcelain").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).OrderBy(l => l, StringComparer.Ordinal).ToArray();
            Assert.Equal(new[] { "?? new.txt", "M a.txt" }, status);
            Assert.Equal("", Sh(dir, "git diff --cached --name-only"));
            Assert.Contains("?? new.txt", Sh(dir, "git status --porcelain"), StringComparison.Ordinal);
            Assert.Equal("slice 1 done, uncommitted\n", File.ReadAllText(Path.Combine(dir, "a.txt")));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Replan_after_a_plan_gap_gets_the_main_agent_diff_stat_and_patch()
    {
        var contract = Squad("skills", "squad", "references", "planning-contract.md");
        var at = contract.IndexOf("Replan after a Plan gap", StringComparison.Ordinal);
        Assert.True(at >= 0, "replan-after-plan-gap line missing");
        var line = contract[at..contract.IndexOf('\n', at)];
        Assert.Contains("`git diff --stat`", line, StringComparison.Ordinal);
        Assert.Contains("`$(git rev-parse --git-dir)/squad/<yyyy-mm-dd>-<slug>.gap-<n>.patch`", line, StringComparison.Ordinal);
        Assert.Contains("the main agent runs `git diff --stat`", line, StringComparison.Ordinal);
        Assert.Contains("keeps its already-changed files", line, StringComparison.Ordinal);
        Assert.Contains("run `git diff --stat`, then `squad-planner` in `next-round` mode on that gap only", Squad("commands", "squad.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void No_em_dash_in_any_plugin_text_file()
    {
        var root = Path.Combine(TestRepository.SourceRoot(), "plugins");
        var binary = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".ico", ".zip", ".dll", ".pdf", ".woff", ".woff2" };
        var hits = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !binary.Contains(Path.GetExtension(path)))
            .SelectMany(path => File.ReadAllLines(path).Select((text, index) => (path, text, index)))
            .Where(hit => hit.text.Contains('\u2014'))
            .Select(hit => $"{Path.GetRelativePath(root, hit.path)}:{hit.index + 1}")
            .ToList();
        Assert.True(hits.Count == 0, "U+2014 found:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void Main_agent_fixes_check_plan_errors_before_confirming()
    {
        Assert.Contains("fixes every error-level finding in the plan text, and reruns until it exits 0",
            Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
        Assert.Contains("Before confirming, run check-plan on the draft and fix every error-level finding (em dash included; paraphrase quoted vendor text)",
            Squad("commands", "squad.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Retro_writer_never_emits_an_em_dash()
    {
        Assert.Contains("Never emit U+2014 (em dash) in the retro", Squad("agents", "squad-retro.md"), StringComparison.Ordinal);
        Assert.Contains("The retro never contains U+2014 (em dash)", Squad("skills", "squad", "references", "retro.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Malformed_reask_marker_is_one_token_everywhere()
    {
        var root = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad");
        var variants = Directory.GetFiles(root, "*.md", SearchOption.AllDirectories)
            .SelectMany(path => System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(path), @"accepted\W{1,3}malformed after re-ask")
                .Select(match => match.Value))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(["accepted, malformed after re-ask"], variants);
    }

    [Theory]
    [InlineData("squad.md")]
    [InlineData("squad-review.md")]
    public void Main_agent_lints_the_retro_and_fixes_findings_before_done(string command)
    {
        Assert.Contains("After `squad-retro` writes, run the `pack-check` skill's `scripts/check-retro.sh <retro>` (or `check-retro.ps1` without `sh`) and fix every finding in the retro before reporting Done",
            Squad("commands", command), StringComparison.Ordinal);
    }

    [Fact]
    public void Check_plan_reruns_after_every_plan_write_and_a_rejected_call_is_not_a_pass()
    {
        Assert.Contains("Every time the plan is written (round 1 draft, each round, write-plan, `## Status`, `## Fix list`, `## Handoff notes`",
            Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
        Assert.Contains("A rejected or failed linter call counts as not run, never as a manual pass:", Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
        Assert.Contains("A rejected or failed linter call counts as not run, never as a manual pass.", Squad("commands", "squad.md"), StringComparison.Ordinal);
        Assert.Contains("a rejected linter call is not run, never a manual pass.", Squad("skills", "squad", "SKILL.md"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("squad-orchestrator.md")]
    [InlineData("squad-implementer.md")]
    [InlineData("squad-planner.md")]
    public void Plan_writers_never_write_an_em_dash(string agent)
    {
        Assert.Contains("Never write U+2014 (em dash) into the plan (`## Fix list`, `## Handoff notes`, `## Status`)", Squad("agents", agent), StringComparison.Ordinal);
        Assert.Contains("No writer puts U+2014 (em dash) into these sections", Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void No_shell_is_an_environment_block_not_a_fix_round()
    {
        var contract = Squad("skills", "squad", "references", "review-contract.md");
        Assert.Contains("the main agent stops before routing and reports `Environment block: no shell`", contract, StringComparison.Ordinal);
        Assert.Contains("Environment block (at verify and fix rounds; a rejected linter call at a plan edit stops the same way", contract, StringComparison.Ordinal);
        var command = Squad("commands", "squad.md");
        var block = command.IndexOf("No shell (calls rejected) at verify: stop and report `Environment block: no shell` before routing", StringComparison.Ordinal);
        Assert.True(block >= 0, "no-shell stop missing from /squad");
        Assert.True(block < command.IndexOf("Route `fix`, `pass`, or `replan`", StringComparison.Ordinal), "no-shell stop must come before routing a fix");
        Assert.Contains("Shell calls rejected → `blocked: environment (no shell)`", Squad("agents", "squad-verifier.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Catch_up_retro_uses_the_interrupted_suffix()
    {
        Assert.Contains("`docs/retros/<yyyy-mm-dd>-<slug>-interrupted.md`", Squad("skills", "squad", "references", "retro.md"), StringComparison.Ordinal);
        Assert.Contains("retro with the same date-slug (`<date-slug>.md` or `<date-slug>-interrupted.md`)", Squad("skills", "squad", "references", "retro.md"), StringComparison.Ordinal);
        Assert.Contains("it writes `docs/retros/<yyyy-mm-dd>-<slug>-interrupted.md`", Squad("commands", "squad.md"), StringComparison.Ordinal);
        Assert.Contains("`<date-slug>-interrupted.md` for a catch-up", Squad("agents", "squad-retro.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Round_one_draft_is_linted_right_away()
    {
        Assert.Contains("Right after round 1 writes the draft, the main agent runs check-plan on it and fixes its findings", Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
        Assert.Contains("After round 1 writes the draft plan, run check-plan on it right away and fix its findings", Squad("commands", "squad.md"), StringComparison.Ordinal);
        Assert.Contains("draft plan file from round 1 (check-plan it at once, fix findings)", Squad("skills", "squad", "SKILL.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Catch_up_retro_is_linted_with_check_retro()
    {
        Assert.Contains("Then run check-retro on that file (not just `test -f`) and fix its findings", Squad("commands", "squad.md"), StringComparison.Ordinal);
        Assert.Contains("the main agent runs check-retro on it (not just `test -f`) and fixes its findings", Squad("skills", "squad", "references", "retro.md"), StringComparison.Ordinal);
        Assert.Contains("the main agent runs check-retro on it (not just `test -f`) and fixes its findings before round 1 starts", Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Every_plan_edit_is_a_hard_check_plan_step_including_resume()
    {
        var contract = Squad("skills", "squad", "references", "planning-contract.md");
        Assert.Contains("Plan edits are a hard step.", contract, StringComparison.Ordinal);
        Assert.Contains("and on a resumed or stopped run", contract, StringComparison.Ordinal);
        Assert.Contains("reruns until it exits 0 before doing anything else", contract, StringComparison.Ordinal);
        Assert.Contains("## Resume\n\nA resumed or stopped run edits the plan too", contract, StringComparison.Ordinal);
        var command = Squad("commands", "squad.md");
        Assert.Contains("Resume: a resumed or stopped run that edits the plan (`## Status`) runs check-plan and does not continue until it exits 0.", command, StringComparison.Ordinal);
        Assert.Contains("Every plan edit is a hard step: write, run check-plan, fix, and do not continue until it exits 0", command, StringComparison.Ordinal);
        Assert.Contains("Every plan edit (resume and stop too): write, run check-plan, continue only at exit 0", Squad("skills", "squad", "SKILL.md"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("before each implementer slice")]
    [InlineData("before the fix-list implementer")]
    [InlineData("before each reviewer round")]
    [InlineData("before `squad-retro` at hand-off")]
    public void Last_plan_edit_is_linted_before_every_spawn_and_retro(string step)
    {
        const string rule = "Before any agent starts or the retro is written, the last plan edit must be followed by check-plan exit 0: ";
        foreach (var text in new[] { Squad("commands", "squad.md"), Squad("skills", "squad", "references", "planning-contract.md") })
        {
            var at = text.IndexOf(rule, StringComparison.Ordinal);
            Assert.True(at >= 0, "spawn lint rule missing");
            var line = text.Substring(at, text.IndexOf('.', at + rule.Length) - at);
            Assert.Contains(step, line, StringComparison.Ordinal);
        }
        Assert.Contains("the last edit exits 0 before any agent starts or the retro is written", Squad("skills", "squad", "SKILL.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Linter_path_is_named_and_old_duplicate_rule_is_gone()
    {
        var command = Squad("commands", "squad.md");
        Assert.Contains("`skills/pack-check/scripts/check-plan.sh <plan>` (or `check-plan.ps1`); do not search the filesystem", command, StringComparison.Ordinal);
        Assert.DoesNotContain("After every plan write (fix list, hand-off notes, status), rerun check-plan", command, StringComparison.Ordinal);
        var contract = Squad("skills", "squad", "references", "planning-contract.md");
        Assert.Contains("`skills/pack-check/scripts/check-plan.sh` and `skills/pack-check/scripts/check-plan.ps1`", contract, StringComparison.Ordinal);
        foreach (var f in new[] { "check-plan.sh", "check-plan.ps1" })
            Assert.True(File.Exists(Path.Combine(TestRepository.SourceRoot(), "plugins", "pack-check", "skills", "pack-check", "scripts", f)), f);
    }

    [Fact]
    public void Plan_gap_step_allows_the_save_reset_and_bans_only_destructive_git()
    {
        var command = Squad("commands", "squad.md");
        Assert.Contains("which ends with `git reset -q`; no `reset --hard`, `clean`, `checkout`, or `restore`", command, StringComparison.Ordinal);
        Assert.DoesNotContain("no reset or clean", command, StringComparison.Ordinal);
    }

    [Fact]
    public void Slice_loop_lints_status_before_each_implementer_and_back_to_back_edits_each_lint()
    {
        var command = Squad("commands", "squad.md");
        Assert.Contains("Inside the loop, slice 1 included: write `## Status` (implementing slice i/N), run check-plan, wait for exit 0, then spawn the implementer.", command, StringComparison.Ordinal);
        Assert.Contains("Two plan edits in a row each get their own check-plan, or combine them into one write and lint that.", command, StringComparison.Ordinal);
    }

    [Fact]
    public void Skill_and_linter_paths_are_named_and_filesystem_search_is_banned()
    {
        var command = Squad("commands", "squad.md");
        Assert.Contains("read `skills/squad/SKILL.md` relative to the squad plugin root (its `--plugin-dir`", command, StringComparison.Ordinal);
        Assert.DoesNotContain("the parent of this `commands/` folder", command, StringComparison.Ordinal);
        Assert.Contains("`../pack-check/skills/pack-check/scripts/` from that root", command, StringComparison.Ordinal);
        Assert.Contains("Never run `find` or any filesystem search for skills or linters.", command, StringComparison.Ordinal);
        var root = Path.Combine(TestRepository.SourceRoot(), "plugins", "squad");
        Assert.True(File.Exists(Path.Combine(root, "skills", "squad", "SKILL.md")));
        Assert.True(File.Exists(Path.Combine(root, "..", "pack-check", "skills", "pack-check", "scripts", "check-plan.sh")));
        Assert.True(File.Exists(Path.Combine(root, "..", "pack-check", "skills", "pack-check", "scripts", "check-plan.ps1")));
    }

    [Fact]
    public void Gap_patch_is_dated_outside_the_worktree_and_never_touches_gitignore()
    {
        foreach (var text in new[] { Squad("commands", "squad.md"), Squad("skills", "squad", "references", "planning-contract.md") })
        {
            Assert.Contains("`$(git rev-parse --git-dir)/squad/<yyyy-mm-dd>-<slug>.gap-<n>.patch`", text, StringComparison.Ordinal);
            Assert.DoesNotContain("docs/plans/<slug>.gap-", text, StringComparison.Ordinal);
            Assert.DoesNotContain("gitignored by", text, StringComparison.Ordinal);
            Assert.DoesNotContain("`.git/squad", text, StringComparison.Ordinal);
        }
        Assert.Contains("never edit the product `.gitignore` for it", Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Gap_save_works_in_a_linked_worktree()
    {
        var contract = Squad("skills", "squad", "references", "planning-contract.md");
        var start = contract.IndexOf("```sh\np=\"$(git rev-parse --git-dir)/squad/", StringComparison.Ordinal);
        var save = contract[(start + 6)..contract.IndexOf("\n```", start + 6, StringComparison.Ordinal)]
            .Replace("<yyyy-mm-dd>-<slug>", "2026-10-12-x", StringComparison.Ordinal).Replace("<n>", "1", StringComparison.Ordinal);
        var dir = Directory.CreateTempSubdirectory("plan-gap-wt-").FullName;
        try
        {
            var main = Path.Combine(dir, "main");
            Directory.CreateDirectory(main);
            File.WriteAllText(Path.Combine(main, "a.txt"), "base\n");
            Sh(main, "git init -q && git add -A && git -c user.name=t -c user.email=t@t commit -qm base && git worktree add -q ../wt");
            var wt = Path.Combine(dir, "wt");
            Assert.True(File.Exists(Path.Combine(wt, ".git")), "linked worktree .git should be a file");
            File.WriteAllText(Path.Combine(wt, "a.txt"), "slice 1 done\n");
            File.WriteAllText(Path.Combine(wt, "new.txt"), "partial\n");
            Sh(wt, save);
            var patch = Path.Combine(Sh(wt, "git rev-parse --git-dir").Trim(), "squad", "2026-10-12-x.gap-1.patch");
            if (!Path.IsPathRooted(patch)) patch = Path.Combine(wt, patch);
            Assert.Contains("new.txt", File.ReadAllText(patch), StringComparison.Ordinal);
            var status = Sh(wt, "git status --porcelain").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).OrderBy(l => l, StringComparer.Ordinal).ToArray();
            Assert.Equal(new[] { "?? new.txt", "M a.txt" }, status);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Command_body_starts_with_exact_paths_and_readme_mirrors_them()
    {
        var command = Squad("commands", "squad.md").Replace("\r\n", "\n", StringComparison.Ordinal);
        var body = command[(command.IndexOf("\n---\n", 4, StringComparison.Ordinal) + 5)..].TrimStart('\n');
        var first = body[..body.IndexOf('\n')];
        Assert.StartsWith("Root: use the stamped `Squad plugin root:` line above this one when present, else the squad `--plugin-dir`; never search for it (no find/glob). Paths, relative to that root:", first, StringComparison.Ordinal);
        var readme = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "README.md"));
        var plugins = Path.Combine(TestRepository.SourceRoot(), "plugins");
        foreach (var rel in new[] { "skills/squad/SKILL.md", "skills/squad/references/planning-contract.md", "../pack-check/skills/pack-check/scripts/check-plan.sh", "../pack-check/skills/pack-check/scripts/check-plan.ps1" })
        {
            var name = rel.EndsWith(".ps1", StringComparison.Ordinal) ? "`check-plan.ps1`" : "`" + rel + "`";
            Assert.Contains(name, first, StringComparison.Ordinal);
            Assert.True(File.Exists(Path.GetFullPath(Path.Combine(plugins, "squad", rel))), rel);
            var fromPlugins = Path.GetRelativePath(plugins, Path.GetFullPath(Path.Combine(plugins, "squad", rel))).Replace('\\', '/');
            Assert.Contains(fromPlugins.EndsWith(".ps1", StringComparison.Ordinal) ? "`check-plan.ps1`" : "`plugins/" + fromPlugins + "`", readme, StringComparison.Ordinal);
        }
        Assert.Contains("no find/glob search", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Plan_gap_answer_needs_the_user_unless_this_session_accepts_recommendations()
    {
        Assert.Contains("Who answers a Plan gap: the user, unless this session explicitly said to accept recommendations. An old grill log or a prior plan is not authorization.",
            Squad("skills", "squad", "references", "planning-contract.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void Gap_save_step_runs_the_contract_command_verbatim_with_test_s()
    {
        Assert.Contains("run the planning contract's save command word for word, no substitutes, so `test -s \"$p\"` fails an empty patch and the run stops", Squad("commands", "squad.md"), StringComparison.Ordinal);
        var contract = Squad("skills", "squad", "references", "planning-contract.md");
        var start = contract.IndexOf("```sh\np=\"$(git rev-parse --git-dir)/squad/", StringComparison.Ordinal);
        var save = contract[start..contract.IndexOf("\n```", start + 6, StringComparison.Ordinal)];
        Assert.Contains("> \"$p\" && test -s \"$p\" && git reset -q", save, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("commands", "squad.md")]
    [InlineData("agents", "squad-implementer.md")]
    public void Exploration_uses_git_ls_files_and_rg_never_find(string folder, string file)
    {
        Assert.Contains("Explore the repo with `git ls-files` and `rg`, never `find`.", Squad(folder, file), StringComparison.Ordinal);
    }

    [Fact]
    public void Planner_explores_with_its_own_tools_not_shell()
    {
        var planner = Squad("agents", "squad-planner.md");
        Assert.Contains("Explore the repo with your `grep` and `glob` tools inside the workspace (you have no shell), never a box-wide search.", planner, StringComparison.Ordinal);
        Assert.DoesNotContain("git ls-files", planner, StringComparison.Ordinal);
    }

    [Fact]
    public void Multi_project_change_needs_two_slices_or_a_reason()
    {
        Assert.Contains("More than one project touched: at least 2 slices, or a `Single slice because: <reason>` line", Squad("agents", "squad-planner.md"), StringComparison.Ordinal);
        var contract = Squad("skills", "squad", "references", "planning-contract.md");
        Assert.Contains("A change that touches more than one project needs at least 2 slices, or a `Single slice because: <reason>` line", contract, StringComparison.Ordinal);
        Assert.Contains("Single slice because: <reason;", contract, StringComparison.Ordinal);
    }

    private static string Sh(string dir, string command)
    {
        var info = new System.Diagnostics.ProcessStartInfo("sh", ["-c", command])
        {
            WorkingDirectory = dir, RedirectStandardOutput = true, RedirectStandardError = true,
        };
        using var p = System.Diagnostics.Process.Start(info)!;
        var output = p.StandardOutput.ReadToEnd();
        var error = p.StandardError.ReadToEnd();
        p.WaitForExit();
        Assert.True(p.ExitCode == 0, $"{command}: {error}");
        return output;
    }

    private static HashSet<string> CommandNames(string directory) =>
        Directory.Exists(directory)
            ? Directory.GetFiles(directory, "*.md")
                .Select(path => Path.GetFileNameWithoutExtension(path)!)
                .ToHashSet(StringComparer.Ordinal)
            : [];
}
