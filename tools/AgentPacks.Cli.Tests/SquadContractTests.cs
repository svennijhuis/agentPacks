using System.Text.Json.Nodes;
using AgentPacks.Cli.Importing;
using AgentPacks.Cli.Io;
using AgentPacks.Cli.Loading;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Squad inventory, frontmatter, and generator contracts. Prompt wording is reviewed,
/// not asserted. Parsers of structured markdown live in <see cref="VerificationEvidenceTests"/>
/// and <see cref="LearningsLogTests"/>.
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

    private static readonly string[] AgentNames =
    [
        "squad-planner",
        "squad-implementer",
        "squad-verifier",
        "squad-reviewer",
        "squad-security-reviewer",
        "squad-simplifier",
        "squad-orchestrator"
    ];

    private static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "squad", name));

    [Fact]
    public void All_seven_agents_are_preserved_in_every_generated_agent_client()
    {
        using var repo = new TestRepository().WithPlugin("squad", Manifest);
        foreach (var agent in AgentNames)
            repo.WithFile($"plugins/squad/agents/{agent}.md", Fixture($"{agent}.md"));

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.Equal(7, AgentNames.Length);
        foreach (var agent in AgentNames)
        {
            Assert.Equal(agent, ParseFrontmatter(Fixture($"{agent}.md")).Scalar("name"));
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
        Assert.Equal("squad-review", ParseFrontmatter(File.ReadAllText(Path.Combine(directory, "squad-review.md"))).Scalar("name"));
        Assert.Equal("squad", ParseFrontmatter(File.ReadAllText(Path.Combine(directory, "squad.md"))).Scalar("name"));
        Assert.Equal("scenarios", ParseFrontmatter(File.ReadAllText(Path.Combine(directory, "scenarios.md"))).Scalar("name"));
        Assert.False(File.Exists(Path.Combine(directory, "review.md")));
        Assert.False(File.Exists(Path.Combine(directory, "http-scenarios.md")));
    }

    [Fact]
    public void Two_user_invoked_entrypoints_are_squad_and_review()
    {
        var skill = ParseFrontmatter(Fixture("SKILL.md"));
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
            "squad-implementer",
            "squad-orchestrator",
            "squad-planner",
            "squad-reviewer",
            "squad-security-reviewer",
            "squad-simplifier",
            "squad-verifier"
        ], names);
        foreach (var name in names)
        {
            Assert.StartsWith("squad-", name, StringComparison.Ordinal);
            Assert.Equal(name, ParseFrontmatter(File.ReadAllText(Path.Combine(agentsDir, $"{name}.md"))).Scalar("name"));
        }

        Assert.False(File.Exists(Path.Combine(agentsDir, "loop-planner.md")));
        Assert.DoesNotContain(AgentNames, name => name.StartsWith("loop-", StringComparison.Ordinal));
    }

    [Fact]
    public void Loop_agent_bodies_stay_tiny()
    {
        foreach (var agent in AgentNames)
        {
            var lines = NonEmptyBodyLines(Fixture($"{agent}.md"));
            var cap = agent == "squad-security-reviewer" ? 44 : 32;
            Assert.True(lines <= cap, $"{agent} body is {lines} lines; cap is {cap}.");
        }
    }

    [Fact]
    public void Loop_agents_use_per_role_tiers_implementer_standard_others_fast()
    {
        Assert.Equal("standard", ParseFrontmatter(Fixture("squad-implementer.md")).Scalar("model"));
        Assert.Equal("true", ParseFrontmatter(Fixture("squad-simplifier.md")).Scalar("readonly"));
        foreach (var agent in AgentNames.Where(name => name != "squad-implementer"))
            Assert.Equal("fast", ParseFrontmatter(Fixture($"{agent}.md")).Scalar("model"));
    }

    [Fact]
    public void Empty_mcp_scaffolds_test_is_not_named_dotnet_only()
    {
        Plugin_mcp_files_are_empty_scaffolds();
    }

    [Fact]
    public void Learnings_digest_is_not_user_invocable()
    {
        var root = TestRepository.SourceRoot();
        var skillPath = Path.Combine(root, "plugins", "squad", "skills", "learnings-digest", "SKILL.md");
        Assert.True(File.Exists(skillPath), "learnings-digest skill must stay; do not remove it.");
        var skill = ParseFrontmatter(File.ReadAllText(skillPath));
        Assert.Equal("learnings-digest", skill.Scalar("name"));
        Assert.Equal("false", skill.Scalar("user-invocable"));
        Assert.Equal("true", skill.Scalar("disable-model-invocation"));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "learnings-digest.md")));
        Squad_commands_are_exactly_squad_and_review();
    }

    [Fact]
    public void Learnings_digest_is_user_invoked_matt_tiny_no_rewrite()
    {
        var skillPath = Path.Combine(
            TestRepository.SourceRoot(), "plugins", "squad", "skills", "learnings-digest", "SKILL.md");
        Assert.True(File.Exists(skillPath));
        var lines = NonEmptyBodyLines(File.ReadAllText(skillPath));
        Assert.True(lines <= 16, $"learnings-digest body is {lines} lines; Matt-tiny cap is 16.");
        Learnings_digest_is_not_user_invocable();
    }

    [Fact]
    public void Suggestions_contract_is_human_apply_only_and_excluded_from_review()
    {
        var root = TestRepository.SourceRoot();
        var suggestions = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "suggestions.md"));
        var skill = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        var review = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "review-contract.md"));
        var learnings = File.ReadAllText(Path.Combine(
            root, "plugins", "squad", "skills", "squad", "references", "learnings.md"));

        Assert.Contains("docs/suggestions.md", suggestions, StringComparison.Ordinal);
        Assert.Contains("- Apply: human", suggestions, StringComparison.Ordinal);
        Assert.Contains("Do not read it to gate", suggestions, StringComparison.Ordinal);
        Assert.Contains("Never open a skill and apply the suggestion", suggestions, StringComparison.Ordinal);
        Assert.Contains("references/suggestions.md", skill, StringComparison.Ordinal);
        Assert.Contains("human-apply only; never auto-rewrite", skill, StringComparison.Ordinal);
        Assert.Contains(":(exclude)docs/suggestions.md", review, StringComparison.Ordinal);
        Assert.Contains("[suggestions](suggestions.md)", learnings, StringComparison.Ordinal);
        Assert.False(
            File.Exists(Path.Combine(root, "plugins", "squad", "commands", "suggestions.md")),
            "suggestions is a contract, not a slash command");
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
            ParseFrontmatter(Fixture("squad.md")).Scalar("description"));
        Assert.Equal(
            "Report-only review of a PR / uncommitted / vs main.",
            ParseFrontmatter(Fixture("squad-review.md")).Scalar("description"));
        Assert.Equal(
            "From changed code, write docs/smoke/<slug>.md. No product-code edits.",
            ParseFrontmatter(File.ReadAllText(
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

                var skill = ParseFrontmatter(File.ReadAllText(Path.Combine(target, "SKILL.md")));
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
        Assert.Equal(1, CountToken(validate, "runs-on:"));
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
        var body = ParseFrontmatter(skill).Body;
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
        var lines = NonEmptyBodyLines(Fixture("SKILL.md"));
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
            var skill = ParseFrontmatter(File.ReadAllText(Path.Combine(skillDir, "SKILL.md")));
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
            Assert.Equal(["testing"], mapped);
        }
    }

    [Fact]
    public void Smoke_matrix_not_user_slash_no_new_plugin()
    {
        var root = TestRepository.SourceRoot();
        Assert.Equal(
            ["dotnet", "git", "pack-check", "rust", "security", "squad", "typescript"],
            Directory.GetDirectories(Path.Combine(root, "plugins"))
                .Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));

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
            ParseFrontmatter(File.ReadAllText(Path.Combine(commandDir, "squad-review.md"))).Scalar("name"));
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

    private static Frontmatter ParseFrontmatter(string text)
    {
        var parsed = Frontmatter.TryParse(text, out var error);
        Assert.True(parsed is not null, error);
        return parsed!;
    }

    private static int NonEmptyBodyLines(string text) =>
        ParseFrontmatter(text).Body.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));

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

    private static HashSet<string> CommandNames(string directory) =>
        Directory.Exists(directory)
            ? Directory.GetFiles(directory, "*.md")
                .Select(path => Path.GetFileNameWithoutExtension(path)!)
                .ToHashSet(StringComparer.Ordinal)
            : [];

    private static int CountToken(string text, string token)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0; index += token.Length)
            count++;
        return count;
    }
}
