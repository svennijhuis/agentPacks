using System.Text.Json.Nodes;

namespace AgentPacks.Cli.Tests;

/// <summary>Po 44 fail bars for the sibling <c>/http-scenarios</c> slash.</summary>
public sealed class HttpScenariosContractTests
{
    /// <summary>
    /// Reviewer named proof (item 44). <c>/http-scenarios</c> writes only
    /// <c>docs/smoke/&lt;slug&gt;.md</c> in the current app workspace. Not a
    /// <c>/squad-review</c> phase. Does not write smoke for push.
    /// </summary>
    [Fact]
    public void Http_scenarios_command_writes_docs_smoke_md_only()
    {
        var root = SourceRoot();
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));
        var review = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad-review.md"));
        var squad = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad.md"));
        var combined = string.Join('\n', command, skill);

        Assert.Equal("scenarios", FrontmatterName(command));
        Assert.DoesNotContain("name: http-scenarios", command, StringComparison.Ordinal);
        Assert.Equal("scenarios-md", FrontmatterName(skill));
        Assert.DoesNotContain("name: http-scenarios", skill, StringComparison.Ordinal);
        Assert.Contains("docs/smoke/<slug>.md", command, StringComparison.Ordinal);
        Assert.Contains("docs/smoke/<slug>.md", skill, StringComparison.Ordinal);
        Assert.Contains("Write only `docs/smoke/<slug>.md`", combined, StringComparison.Ordinal);
        Assert.Contains("repo under test", combined, StringComparison.Ordinal);
        Assert.Contains("current app workspace", combined, StringComparison.Ordinal);
        Assert.Contains("does not write smoke for push", combined, StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("docs/reviews/", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("docs/plans/", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("docs/decisions.md", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("docs/learnings.md", combined, StringComparison.Ordinal);

        Assert.DoesNotContain("http-scenarios", review, StringComparison.Ordinal);
        Assert.DoesNotContain("docs/smoke/", review, StringComparison.Ordinal);
        Assert.DoesNotContain("docs/smoke/", squad, StringComparison.Ordinal);
        Assert.DoesNotContain("Write only `docs/smoke/<slug>.md`", squad, StringComparison.Ordinal);

        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "commands", "squad-review")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "squad-smoke.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "smoke.md")));
    }

    /// <summary>
    /// Reviewer named proof (item 44). Research table shape:
    /// # · Case · Kind · Request · Status · Expected · Why.
    /// Kinds include happy/edge/fail/auth/biz/nothing-breaks.
    /// </summary>
    [Fact]
    public void Http_scenarios_table_has_case_kind_request_status_expected_why()
    {
        var skill = File.ReadAllText(ScenariosSkillPath(SourceRoot()));

        Assert.Contains("# · Case · Kind · Request · Status · Expected · Why", skill, StringComparison.Ordinal);
        Assert.Contains("| # | Case | Kind | Request | Status | Expected | Why |", skill, StringComparison.Ordinal);
        foreach (var kind in new[] { "happy", "edge", "fail", "auth", "biz", "nothing-breaks" })
            Assert.Contains($"`{kind}`", skill, StringComparison.Ordinal);

        Assert.Contains("path/operation", skill, StringComparison.Ordinal);
        Assert.Contains("OpenAPI", skill, StringComparison.Ordinal);
        Assert.Contains("Swagger", skill, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reviewer named proof (item 44). No product-source edits. No hardcoded
    /// tokens — env placeholders only. Seeded from <c>smoke-matrix.md</c>.
    /// </summary>
    [Fact]
    public void Http_scenarios_no_code_edits_no_secrets_seeded_from_smoke_matrix()
    {
        var root = SourceRoot();
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));
        var combined = string.Join('\n', command, skill);
        var matrix = File.ReadAllText(Path.Combine(root, "plugins", "squad", "references", "smoke-matrix.md"));

        Assert.Contains("Do not edit product source", combined, StringComparison.Ordinal);
        Assert.Contains("Do not commit, merge, or push", combined, StringComparison.Ordinal);
        Assert.Contains("../../references/smoke-matrix.md", skill, StringComparison.Ordinal);
        Assert.Contains("Seed kinds from that shape", skill, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "references", "smoke-matrix.md")));
        foreach (var token in new[] { "Happy", "Edge", "Fail", "Auth", "Timeout", "5xx" })
            Assert.Contains(token, matrix, StringComparison.Ordinal);

        Assert.Contains("BASE_URL", skill, StringComparison.Ordinal);
        Assert.Contains("TOKEN_VALID", skill, StringComparison.Ordinal);
        Assert.Contains("Never a real token", skill, StringComparison.Ordinal);
        Assert.Contains("Placeholders only", skill, StringComparison.Ordinal);

        Assert.DoesNotContain("sk-", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("password=", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("api_key", combined, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("optional mention", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not write one", skill, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reviewer named proof (item 44). User slashes stay
    /// <c>/squad</c> + <c>/squad-review</c> + <c>/pack-check</c> +
    /// <c>/scenarios</c>. Slash split: review is code/diff; scenarios
    /// is scenarios md for a real tester on a deployed env. Packaged for
    /// Claude/Cursor/Copilot without colliding names. No new marketplace plugin.
    /// </summary>
    [Fact]
    public void User_commands_stay_squad_squad_review_pack_check_plus_http_scenarios()
    {
        var root = SourceRoot();
        var commands = Directory.GetFiles(Path.Combine(root, "plugins"), "*.md", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}commands{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}com.", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetFileNameWithoutExtension(path) ?? path)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["pack-check", "scenarios", "squad", "squad-review"], commands);
        Assert.DoesNotContain("squad-smoke", commands, StringComparer.Ordinal);
        Assert.DoesNotContain("smoke", commands, StringComparer.Ordinal);
        Assert.DoesNotContain("review", commands, StringComparer.Ordinal);
        Assert.Equal(
            ["dotnet", "git", "pack-check", "rust", "squad", "typescript"],
            Directory.GetDirectories(Path.Combine(root, "plugins"))
                .Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));

        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));
        var review = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad-review.md"));
        var combined = string.Join('\n', command, skill);

        Assert.Contains("`/squad-review` is code/diff", combined, StringComparison.Ordinal);
        Assert.Contains("scenarios md for a real tester on a deployed env", combined, StringComparison.Ordinal);
        Assert.Contains("`/squad` may read", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("http-scenarios", review, StringComparison.Ordinal);
        Assert.Contains("name: squad-review", review, StringComparison.Ordinal);
        Assert.Contains("Save report as markdown?", review, StringComparison.Ordinal);

        Assert.Contains("disable-model-invocation: true", skill, StringComparison.Ordinal);
        Assert.Contains("user-invocable: false", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("audience: loop", skill, StringComparison.Ordinal);

        using var repo = new TestRepository().WithPlugin(
            "squad",
            File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")));
        foreach (var path in Directory.GetFiles(Path.Combine(root, "plugins", "squad", "commands"), "*.md"))
        {
            repo.WithFile(
                $"plugins/squad/commands/{Path.GetFileName(path)}",
                File.ReadAllText(path));
        }

        repo.WithSkill(
            "squad",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");
        repo.WithSkill(
            "scenarios-md",
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

        Assert.True(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/run.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.True(File.Exists(Path.Combine(repo.PluginDirectory("squad"), "commands", "scenarios.md")));
        Assert.False(File.Exists(Path.Combine(repo.PluginDirectory("squad"), "commands", "http-scenarios.md")));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/squad.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/run.md"));
        Assert.True(run.HasFile("plugins/pack-check/com.github.copilot/commands/pack-check.md"));

        var claude = run.File("plugins/squad/com.anthropic.claude-code/commands/scenarios.md").Text;
        Assert.DoesNotContain("name: \"squad\"", claude, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"run\"", claude, StringComparison.Ordinal);

        var copilot = run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text;
        Assert.Contains("name: \"scenarios\"", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"http-scenarios\"", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"run\"", copilot, StringComparison.Ordinal);

        var factory = run.File("plugins/squad/com.github.copilot/commands/run.md").Text;
        Assert.Contains("name: \"run\"", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"http-scenarios\"", factory, StringComparison.Ordinal);

        var claudeEntry = run.File(".claude-plugin/marketplace.json").Content["plugins"]!.AsArray()
            .OfType<JsonObject>()
            .Single(plugin => plugin["name"]!.GetValue<string>() == "squad");
        Assert.True(claudeEntry["strict"]!.GetValue<bool>());

        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Item 44 lock. Env-agnostic: <c>BASE_URL</c> / deployed env. Azure, TST,
    /// and AWS are not required targets.
    /// </summary>
    [Fact]
    public void Http_scenarios_env_agnostic_no_required_cloud()
    {
        var root = SourceRoot();
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var combined = string.Join('\n', command, skill);

        Assert.Contains("BASE_URL", combined, StringComparison.Ordinal);
        Assert.Contains("deployed env", combined, StringComparison.Ordinal);
        Assert.Contains("Do not require Azure, TST, or AWS", command, StringComparison.Ordinal);
        Assert.Contains("Do not require Azure, TST, or AWS", skill, StringComparison.Ordinal);
        Assert.Contains("Do not require Azure, TST, or AWS", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("on TST", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Azure App Service", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("AWS API Gateway", combined, StringComparison.Ordinal);
    }

    /// <summary>
    /// Item 44 lock. Auth rows default skip: one header line. Add auth/policy
    /// rows only when the ask or OpenAPI change is about auth or new policies.
    /// </summary>
    [Fact]
    public void Http_scenarios_auth_rows_default_skip()
    {
        var skill = File.ReadAllText(ScenariosSkillPath(SourceRoot()));

        Assert.Contains("Auth: Bearer TOKEN_VALID (tester supplies)", skill, StringComparison.Ordinal);
        Assert.Contains("Add `auth` / policy rows ONLY when the user ask", skill, StringComparison.Ordinal);
        Assert.Contains("OpenAPI change is about auth or new policies", skill, StringComparison.Ordinal);
        Assert.Contains("Do not spam 401/403 rows by default", skill, StringComparison.Ordinal);
        foreach (var kind in new[] { "happy", "edge", "fail", "biz", "nothing-breaks" })
            Assert.Contains($"`{kind}`", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("TOKEN_INVALID", skill, StringComparison.Ordinal);
    }

    /// <summary>
    /// Item 45 lock. Seed changed code first. OpenAPI/Swagger fills gaps only
    /// and is not required.
    /// </summary>
    [Fact]
    public void Http_scenarios_seeds_from_code_first_openapi_optional()
    {
        var root = SourceRoot();
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));
        var combined = string.Join('\n', command, skill);

        Assert.Contains("changed code first", combined, StringComparison.Ordinal);
        Assert.Contains("controllers", combined, StringComparison.Ordinal);
        Assert.Contains("routes", combined, StringComparison.Ordinal);
        Assert.Contains("handlers", combined, StringComparison.Ordinal);
        Assert.Contains("Azure Functions", combined, StringComparison.Ordinal);
        Assert.Contains("AWS Lambda", combined, StringComparison.Ordinal);
        Assert.Contains("fills gaps only", combined, StringComparison.Ordinal);
        Assert.Contains("Not required", skill, StringComparison.Ordinal);
        Assert.Contains("not required", command, StringComparison.Ordinal);
        Assert.Contains("Do not ask for a spec when code is enough", skill, StringComparison.Ordinal);
        Assert.Contains("Write only `docs/smoke/<slug>.md`", combined, StringComparison.Ordinal);
        Assert.Contains("for test design", combined, StringComparison.Ordinal);
    }

    /// <summary>
    /// Item 45 lock. Timer/cron triggers get <c>edge</c>/<c>fail</c> rows.
    /// Do not invent fake HTTP when the trigger is not HTTP.
    /// </summary>
    [Fact]
    public void Http_scenarios_covers_timer_cron_triggers()
    {
        var skill = File.ReadAllText(ScenariosSkillPath(SourceRoot()));
        var command = File.ReadAllText(Path.Combine(
            SourceRoot(), "plugins", "squad", "commands", "scenarios.md"));
        var combined = string.Join('\n', command, skill);

        Assert.Contains("timer/cron", combined, StringComparison.Ordinal);
        Assert.Contains("kind `edge` / `fail`", skill, StringComparison.Ordinal);
        Assert.Contains("did not run", skill, StringComparison.Ordinal);
        Assert.Contains("ran twice", skill, StringComparison.Ordinal);
        Assert.Contains("poison message", skill, StringComparison.Ordinal);
        Assert.Contains("partial batch", skill, StringComparison.Ordinal);
        Assert.Contains("Not fake HTTP when the trigger is not HTTP", skill, StringComparison.Ordinal);
        Assert.Contains("timer rows are not GET", skill, StringComparison.Ordinal);
    }

    /// <summary>
    /// Prior fail bar kept as deferral: http-scenarios points at the Squad
    /// local-secrets rule instead of repeating the essay.
    /// </summary>
    [Fact]
    public void Http_scenarios_local_unauth_kv_ask_continue()
    {
        Http_scenarios_defers_local_secrets_to_squad_rule();
    }

    /// <summary>
    /// Po lock. <c>/http-scenarios</c> defers local Key Vault / secret-store
    /// unauth to the Squad local-secrets rule. One-liner pointer, no essay.
    /// </summary>
    [Fact]
    public void Http_scenarios_defers_local_secrets_to_squad_rule()
    {
        var root = SourceRoot();
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));
        var squad = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "squad", "SKILL.md"));
        var combined = string.Join('\n', command, skill);

        Assert.Contains("Squad local-secrets rule", combined, StringComparison.Ordinal);
        Assert.Contains("../squad/SKILL.md", skill, StringComparison.Ordinal);
        Assert.Contains("skill `squad`", command, StringComparison.Ordinal);
        Assert.DoesNotContain("mention that in the chat/output", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Do not invent secrets", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Do not silently skip", combined, StringComparison.Ordinal);
        Assert.Contains("Local-secrets rule", squad, StringComparison.Ordinal);
        Assert.Contains("ask: continue?", squad, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reviewer lock (item 49). Command frontmatter stays <c>name: scenarios</c>.
    /// Skill is <c>scenarios-md</c> (item 51). No leftover <c>http-scenarios</c>
    /// slash or file. Skill stays <c>user-invocable: false</c> so the picker is
    /// the command.
    /// </summary>
    [Fact]
    public void User_slash_is_scenarios_not_http_scenarios()
    {
        var root = SourceRoot();
        var commandPath = Path.Combine(root, "plugins", "squad", "commands", "scenarios.md");
        var skillPath = ScenariosSkillPath(root);
        var command = File.ReadAllText(commandPath);
        var skill = File.ReadAllText(skillPath);

        Assert.True(File.Exists(commandPath));
        Assert.True(File.Exists(skillPath));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "scenarios")));

        Assert.Equal("scenarios", FrontmatterName(command));
        Assert.Equal("scenarios-md", FrontmatterName(skill));
        Assert.DoesNotContain("name: http-scenarios", command, StringComparison.Ordinal);
        Assert.DoesNotContain("name: http-scenarios", skill, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name `scenarios-md`", command, StringComparison.Ordinal);
        Assert.Contains("Type /scenarios", skill, StringComparison.Ordinal);
        Assert.Contains("user-invocable: false", skill, StringComparison.Ordinal);

        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        Assert.Contains("| `/scenarios` | Office tester | `docs/smoke/*.md` only |", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("| `/http-scenarios`", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("`/squad:http-scenarios`", readme, StringComparison.Ordinal);

        Http_scenarios_command_writes_docs_smoke_md_only();
        Http_scenarios_no_code_edits_no_secrets_seeded_from_smoke_matrix();
    }

    /// <summary>
    /// Po 47 fail bar, updated for item 49. Copilot ships sibling
    /// <c>scenarios</c> as <c>/squad:scenarios</c> at
    /// <c>com.github.copilot/commands/scenarios.md</c>.
    /// </summary>
    [Fact]
    public void Copilot_ships_http_scenarios_command()
    {
        Copilot_ships_scenarios_command();
    }

    /// <summary>
    /// Reviewer lock (item 49). Copilot picker is <c>/squad:scenarios</c>.
    /// No leftover Copilot <c>http-scenarios.md</c>.
    /// </summary>
    [Fact]
    public void Copilot_ships_scenarios_command()
    {
        Copilot_http_scenarios_renamed_to_scenarios();
    }

    /// <summary>
    /// Item 49 fail bar. Copilot command name is <c>scenarios</c>.
    /// No leftover Copilot <c>http-scenarios.md</c>. Picker is
    /// <c>/squad:scenarios</c>.
    /// </summary>
    [Fact]
    public void Copilot_http_scenarios_renamed_to_scenarios()
    {
        var root = SourceRoot();
        using var repo = SquadCommandRepo(root);
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/squad.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/run.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/squad-review.md"));

        var copilot = run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text;
        Assert.Contains("name: \"scenarios\"", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"http-scenarios\"", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"run\"", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", copilot, StringComparison.Ordinal);

        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        Assert.Contains("Copilot uses namespaced `/squad:…` for run, squad-review, scenarios", readme, StringComparison.Ordinal);
        Assert.Contains("`/squad:run`", readme, StringComparison.Ordinal);
        Assert.Contains("`/squad:squad-review`", readme, StringComparison.Ordinal);
        Assert.Contains("`/squad:scenarios`", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("`/squad:http-scenarios`", readme, StringComparison.Ordinal);

        Assert.False(File.Exists(Path.Combine(
            root, "plugins", "squad", "com.github.copilot", "commands", "http-scenarios.md")));

        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Item 49 fail bar. Every client tree ships <c>scenarios</c>, not leftover
    /// <c>http-scenarios</c>: root/Cursor, Claude, Copilot, and Codex.
    /// Skill id is <c>scenarios-md</c> (Skill-tool only; item 51).
    /// </summary>
    [Fact]
    public void All_trees_command_is_scenarios()
    {
        var root = SourceRoot();
        using var repo = SquadCommandRepo(root);
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md")));
        Assert.Equal(
            "scenarios",
            FrontmatterName(File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"))));

        Assert.True(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/http-scenarios.md"));
        Assert.Contains(
            "exact name `scenarios-md`",
            run.File("plugins/squad/com.anthropic.claude-code/commands/scenarios.md").Text,
            StringComparison.Ordinal);

        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.Equal(
            "scenarios",
            FrontmatterName(run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text));
        Assert.Contains(
            "exact name `scenarios-md`",
            run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text,
            StringComparison.Ordinal);

        Assert.True(File.Exists(Path.Combine(repo.PluginDirectory("squad"), "commands", "scenarios.md")));
        Assert.False(File.Exists(Path.Combine(repo.PluginDirectory("squad"), "commands", "http-scenarios.md")));

        Assert.False(run.HasFile("plugins/squad/com.openai.codex/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.openai.codex/commands/scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/.cursor-plugin/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/.cursor-plugin/commands/scenarios.md"));

        var leftover = run.Generated
            .Where(file => file.RelativePath.Replace('\\', '/').Contains("/commands/", StringComparison.Ordinal)
                && file.RelativePath.Contains("http-scenarios", StringComparison.Ordinal))
            .Select(file => file.RelativePath)
            .ToArray();
        Assert.Empty(leftover);

        var skill = File.ReadAllText(ScenariosSkillPath(root));
        Assert.Equal("scenarios-md", FrontmatterName(skill));
        Assert.DoesNotContain("name: http-scenarios", skill, StringComparison.Ordinal);
        Assert.Contains("user-invocable: false", skill, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "scenarios")));

        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Item 51 fail bar. Copilot hides a command whose name equals a skill
    /// name in the same plugin. Factory avoided this (<c>run</c> ≠ <c>squad</c>).
    /// Command stays <c>scenarios</c> (<c>/squad:scenarios</c>); skill is
    /// <c>scenarios-md</c>.
    /// </summary>
    [Fact]
    public void Copilot_scenarios_command_name_differs_from_skill()
    {
        var root = SourceRoot();
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));

        Assert.Equal("scenarios", FrontmatterName(command));
        Assert.Equal("scenarios-md", FrontmatterName(skill));
        Assert.NotEqual(FrontmatterName(command), FrontmatterName(skill));
        Assert.True(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "scenarios-md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "scenarios")));
        Assert.Contains("user-invocable: false", skill, StringComparison.Ordinal);
        Assert.Contains("Skill tool by exact name `scenarios-md`", command, StringComparison.Ordinal);

        using var repo = SquadCommandRepo(root);
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);

        var copilot = run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text;
        Assert.Equal("scenarios", FrontmatterName(copilot));
        Assert.Contains("exact name `scenarios-md`", copilot, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"scenarios-md\"", copilot, StringComparison.Ordinal);

        var factory = run.File("plugins/squad/com.github.copilot/commands/run.md").Text;
        Assert.Contains("name: \"run\"", factory, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"squad\"", factory, StringComparison.Ordinal);

        var claude = run.File("plugins/squad/com.anthropic.claude-code/commands/scenarios.md").Text;
        Assert.Contains("exact name `scenarios-md`", claude, StringComparison.Ordinal);

        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    /// <summary>
    /// Item 51 fail bar. User slash stays <c>/scenarios</c>; Copilot picker
    /// stays <c>/squad:scenarios</c>. No leftover <c>http-scenarios</c>.
    /// </summary>
    [Fact]
    public void Slash_stays_scenarios_or_squad_scenarios()
    {
        var root = SourceRoot();
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md"));
        var skill = File.ReadAllText(ScenariosSkillPath(root));
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var pluginReadme = File.ReadAllText(Path.Combine(root, "plugins", "squad", "README.md"));

        Assert.Equal("scenarios", FrontmatterName(command));
        Assert.True(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "scenarios.md")));
        Assert.False(File.Exists(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md")));
        Assert.False(Directory.Exists(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios")));

        Assert.Contains("Type /scenarios", skill, StringComparison.Ordinal);
        Assert.Contains("| `/scenarios` | Office tester | `docs/smoke/*.md` only |", readme, StringComparison.Ordinal);
        Assert.Contains("`/squad:scenarios`", readme, StringComparison.Ordinal);
        Assert.Contains("Copilot picker: `/squad:scenarios`", pluginReadme, StringComparison.Ordinal);

        Assert.DoesNotContain("`/http-scenarios`", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("`/squad:http-scenarios`", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("`/http-scenarios`", pluginReadme, StringComparison.Ordinal);
        Assert.DoesNotContain("`/squad:http-scenarios`", pluginReadme, StringComparison.Ordinal);
        Assert.DoesNotContain("name: http-scenarios", command, StringComparison.Ordinal);
        Assert.DoesNotContain("name: http-scenarios", skill, StringComparison.Ordinal);

        using var repo = SquadCommandRepo(root);
        var run = repo.ValidateAndGenerate();
        Assert.False(run.HasErrors, run.Text);
        var copilot = run.File("plugins/squad/com.github.copilot/commands/scenarios.md").Text;
        Assert.Equal("scenarios", FrontmatterName(copilot));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));

        new SquadContractTests().Pull_request_ci_stays_one_job_no_matrix();
    }

    private static TestRepository SquadCommandRepo(string root)
    {
        var repo = new TestRepository().WithPlugin(
            "squad",
            File.ReadAllText(Path.Combine(root, "plugins", "squad", "plugin.json")));

        foreach (var path in Directory.GetFiles(Path.Combine(root, "plugins", "squad", "commands"), "*.md"))
        {
            repo.WithFile(
                $"plugins/squad/commands/{Path.GetFileName(path)}",
                File.ReadAllText(path));
        }

        repo.WithSkill(
            "squad",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");
        repo.WithSkill(
            "scenarios-md",
            extraFrontmatter: "disable-model-invocation: true\nuser-invocable: false",
            plugin: "squad");

        return repo;
    }

    private static string ScenariosSkillPath(string root) =>
        Path.Combine(root, "plugins", "squad", "skills", "scenarios-md", "SKILL.md");

    private static string FrontmatterName(string markdown)
    {
        var inFrontmatter = false;
        foreach (var line in markdown.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed == "---")
            {
                if (inFrontmatter)
                    break;
                inFrontmatter = true;
                continue;
            }

            if (inFrontmatter && trimmed.StartsWith("name:", StringComparison.Ordinal))
                return trimmed["name:".Length..].Trim().Trim('"');
        }

        throw new InvalidOperationException("missing frontmatter name");
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
