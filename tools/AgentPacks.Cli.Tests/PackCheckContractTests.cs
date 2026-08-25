using System.Diagnostics;
using System.Text.Json.Nodes;
using AgentPacks.Cli.Generation;
using AgentPacks.Cli.Validation;

namespace AgentPacks.Cli.Tests;

/// <summary>The registry, session hook, approval boundary, and four-provider install contract.</summary>
public sealed class PackCheckContractTests
{
    private static readonly string FixtureRoot =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "pack-check");

    private static string Fixture(string name) => File.ReadAllText(Path.Combine(FixtureRoot, name));

    [Fact]
    public void Pack_check_does_not_opt_into_the_language_pack_contract()
    {
        var manifest = JsonNode.Parse(Fixture("plugin.json"))!;
        var keywords = manifest["keywords"]!.AsArray().Select(value => value!.GetValue<string>());

        Assert.DoesNotContain(LanguagePackContract.Keyword, keywords, StringComparer.Ordinal);
    }

    [Fact]
    public void Every_language_pack_has_exactly_one_registry_row()
    {
        var registered = RegistryPacks(Fixture("packs.md"));
        var manifests = Directory.GetFiles(Path.Combine(SourceRoot(), "plugins"), "plugin.json", SearchOption.AllDirectories);
        var languages = new List<string>();

        foreach (var path in manifests)
        {
            var manifest = JsonNode.Parse(File.ReadAllText(path))!;
            var keywords = manifest["keywords"]?.AsArray()
                .Select(value => value?.GetValue<string>())
                .Where(value => value is not null)
                .ToList() ?? [];

            if (!keywords.Contains(LanguagePackContract.Keyword, StringComparer.Ordinal))
            {
                continue;
            }

            languages.Add(manifest["name"]!.GetValue<string>());
        }

        Assert.Equal(
            languages.Order(StringComparer.Ordinal),
            registered.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Install_actions_use_the_built_marketplace_name_and_require_approval_and_reload()
    {
        var skill = Fixture("SKILL.md");
        var selector = $"<pack>@{ClaudeCompatGenerator.MarketplaceName}";

        Assert.Contains($"claude plugin install {selector} --scope user", skill, StringComparison.Ordinal);
        Assert.Contains($"codex plugin add {selector}", skill, StringComparison.Ordinal);
        Assert.Contains($"copilot plugin install {selector}", skill, StringComparison.Ordinal);
        Assert.Contains("Customize → agentpacks → &lt;pack&gt; → Install", skill, StringComparison.Ordinal);
        Assert.Contains("explicit approval", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reload or start a new session", skill, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("~/.agent" + "packs/packs", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", skill, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pack_check_generates_a_session_start_hook_for_every_provider()
    {
        using var repo = new TestRepository()
            .WithPlugin("pack-check", Fixture("plugin.json"))
            .WithSkill("pack-check", plugin: "pack-check")
            .WithFile("plugins/pack-check/hooks.source.json", Fixture("hooks.source.json"))
            .WithScript("pack-check-session", plugin: "pack-check");

        var run = repo.ValidateAndGenerate();

        Assert.False(run.HasErrors, run.Text);
        Assert.True(run.HasFile("plugins/pack-check/hooks/hooks.json"));
        Assert.True(run.HasFile("plugins/pack-check/com.anthropic.claude-code/hooks/hooks.json"));
        Assert.True(run.HasFile("plugins/pack-check/com.openai.codex/hooks/hooks.json"));
        Assert.True(run.HasFile("plugins/pack-check/com.github.copilot/hooks/hooks.json"));
    }

    [Fact]
    public void Session_hook_is_silent_without_a_registered_marker()
    {
        using var repo = new TestRepository();

        var result = RunHook(repo.Root);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Session_hook_requests_pack_check_context_for_a_bare_csproj()
    {
        using var repo = new TestRepository();
        File.WriteAllText(Path.Combine(repo.Root, "Bare.csproj"), string.Empty);

        var result = RunHook(repo.Root);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("detected stack dotnet with pack dotnet", result.Output, StringComparison.Ordinal);
        Assert.Contains("request installation approval", result.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Session_hook_requests_pack_check_context_for_a_cargo_workspace()
    {
        using var repo = new TestRepository();
        File.WriteAllText(Path.Combine(repo.Root, "Cargo.toml"), "[workspace]\nresolver = \"3\"\n");

        var result = RunHook(repo.Root);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("detected stack rust with pack rust", result.Output, StringComparison.Ordinal);
        Assert.Contains("request installation approval", result.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Session_hook_reports_every_detected_stack_once()
    {
        using var repo = new TestRepository();
        File.WriteAllText(Path.Combine(repo.Root, "App.csproj"), string.Empty);
        File.WriteAllText(Path.Combine(repo.Root, "Cargo.toml"), "[workspace]\nresolver = \"3\"\n");
        Directory.CreateDirectory(Path.Combine(repo.Root, "crates", "member"));
        File.WriteAllText(Path.Combine(repo.Root, "crates", "member", "Cargo.toml"), "[package]\nname = \"member\"\n");

        var result = RunHook(repo.Root);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, Occurrences(result.Output, "detected stack dotnet with pack dotnet"));
        Assert.Equal(1, Occurrences(result.Output, "detected stack rust with pack rust"));
        Assert.Equal(1, Occurrences(result.Output, "Before handling the first coding request"));
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Session_hook_ignores_rust_markers_in_build_and_vendor_directories()
    {
        using var repo = new TestRepository();
        foreach (var directory in new[] { "target", "vendor" })
        {
            var dependency = Path.Combine(repo.Root, directory, "dependency");
            Directory.CreateDirectory(dependency);
            File.WriteAllText(Path.Combine(dependency, "Cargo.toml"), "[package]\nname = \"dependency\"\n");
        }

        var result = RunHook(repo.Root);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Posix_and_powershell_hooks_share_the_registry_and_context_contract()
    {
        var bash = Fixture("pack-check-session.sh");
        var powerShell = Fixture("pack-check-session.ps1");

        foreach (var fragment in (string[])
                 ["skills/pack-check/references/packs.md", "pack-check detected stack", "request installation approval", "target", "vendor"])
        {
            Assert.Contains(fragment, bash, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(fragment, powerShell, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Delivery_loop_honors_install_refusal_bypass_and_small_change_gate()
    {
        var skill = Fixture("SKILL.md");
        var delivery = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "delivery-loop", "deliver.md"));

        Assert.Contains("do not ask again in that session", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("continue an ordinary", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--no-pack", delivery, StringComparison.Ordinal);
        Assert.Contains("Small work may continue", delivery, StringComparison.Ordinal);
        Assert.Contains("stop for a reload before creating `docs/plans/`", delivery, StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> RegistryPacks(string registry) =>
        registry.Split('\n')
            .Where(line => line.StartsWith("| `", StringComparison.Ordinal))
            .Select(line => line.Split('|'))
            .Where(cells => cells.Length >= 5)
            .Select(cells => cells[3].Trim().Trim('`'))
            .ToList();

    private static int Occurrences(string text, string value) =>
        text.Split(value, StringSplitOptions.None).Length - 1;

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

    private static (int ExitCode, string Output, string Error) RunHook(string workingDirectory)
    {
        var start = new ProcessStartInfo("bash")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        start.ArgumentList.Add(Path.Combine(
            SourceRoot(), "plugins", "pack-check", "scripts", "pack-check-session.sh"));

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, error);
    }
}
