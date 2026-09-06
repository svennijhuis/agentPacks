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
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md"));
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios", "SKILL.md"));
        var review = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad-review.md"));
        var squad = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "squad.md"));
        var combined = string.Join('\n', command, skill);

        Assert.Contains("name: http-scenarios", command, StringComparison.Ordinal);
        Assert.Contains("name: http-scenarios", skill, StringComparison.Ordinal);
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
        var skill = File.ReadAllText(Path.Combine(
            SourceRoot(), "plugins", "squad", "skills", "http-scenarios", "SKILL.md"));

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
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md"));
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios", "SKILL.md"));
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
    /// <c>/http-scenarios</c>. Slash split: review is code/diff; http-scenarios
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

        Assert.Equal(["http-scenarios", "pack-check", "squad", "squad-review"], commands);
        Assert.DoesNotContain("squad-smoke", commands, StringComparer.Ordinal);
        Assert.DoesNotContain("smoke", commands, StringComparer.Ordinal);
        Assert.DoesNotContain("review", commands, StringComparer.Ordinal);
        Assert.Equal(
            ["dotnet", "git", "pack-check", "rust", "squad", "typescript"],
            Directory.GetDirectories(Path.Combine(root, "plugins"))
                .Select(path => Path.GetFileName(path) ?? path)
                .OrderBy(name => name, StringComparer.Ordinal));

        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md"));
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios", "SKILL.md"));
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
            "http-scenarios",
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

        Assert.True(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/http-scenarios.md"));
        Assert.False(run.HasFile("plugins/squad/com.anthropic.claude-code/commands/run.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/http-scenarios.md"));
        Assert.True(File.Exists(Path.Combine(repo.PluginDirectory("squad"), "commands", "http-scenarios.md")));
        Assert.False(run.HasFile("plugins/squad/com.github.copilot/commands/squad.md"));
        Assert.True(run.HasFile("plugins/squad/com.github.copilot/commands/run.md"));
        Assert.True(run.HasFile("plugins/pack-check/com.github.copilot/commands/pack-check.md"));

        var claude = run.File("plugins/squad/com.anthropic.claude-code/commands/http-scenarios.md").Text;
        Assert.DoesNotContain("name: \"squad\"", claude, StringComparison.Ordinal);
        Assert.DoesNotContain("name: \"run\"", claude, StringComparison.Ordinal);

        var copilot = run.File("plugins/squad/com.github.copilot/commands/http-scenarios.md").Text;
        Assert.Contains("name: \"http-scenarios\"", copilot, StringComparison.Ordinal);
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
        var command = File.ReadAllText(Path.Combine(root, "plugins", "squad", "commands", "http-scenarios.md"));
        var skill = File.ReadAllText(Path.Combine(root, "plugins", "squad", "skills", "http-scenarios", "SKILL.md"));
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
        var skill = File.ReadAllText(Path.Combine(
            SourceRoot(), "plugins", "squad", "skills", "http-scenarios", "SKILL.md"));

        Assert.Contains("Auth: Bearer TOKEN_VALID (tester supplies)", skill, StringComparison.Ordinal);
        Assert.Contains("Add `auth` / policy rows ONLY when the user ask", skill, StringComparison.Ordinal);
        Assert.Contains("OpenAPI change is about auth or new policies", skill, StringComparison.Ordinal);
        Assert.Contains("Do not spam 401/403 rows by default", skill, StringComparison.Ordinal);
        foreach (var kind in new[] { "happy", "edge", "fail", "biz", "nothing-breaks" })
            Assert.Contains($"`{kind}`", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("TOKEN_INVALID", skill, StringComparison.Ordinal);
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
