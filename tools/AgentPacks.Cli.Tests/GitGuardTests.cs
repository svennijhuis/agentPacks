using System.Diagnostics;
using System.Text.Json;

namespace AgentPacks.Cli.Tests;

/// <summary>The destructive-command policy and the two payload shapes used by four providers.</summary>
public sealed class GitGuardTests
{
    private static readonly string Script =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "git", "git-guard.sh");

    public static TheoryData<string, string> Providers() => new()
    {
        { "claude", "nested" },
        { "cursor", "flat" },
        { "codex", "nested" },
        { "copilot", "nested" }
    };

    [Theory]
    [MemberData(nameof(Providers))]
    public void Every_provider_payload_blocks_destructive_git(string provider, string shape)
    {
        var result = Run("cd /tmp && git -C /repo reset --hard", shape);

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("GIT001", result.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("cd /tmp", result.Error, StringComparison.Ordinal);
        Assert.DoesNotContain(provider, result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("git clean -fd", "GIT002")]
    [InlineData("git push origin main --force-with-lease", "GIT003")]
    [InlineData("git branch --delete old --force", "GIT004")]
    [InlineData("git checkout -- src/Changed.cs", "GIT005")]
    [InlineData("git restore src/Changed.cs", "GIT006")]
    [InlineData("git restore --staged --worktree src/Changed.cs", "GIT006")]
    public void Each_destructive_family_has_a_stable_rule_id(string command, string rule)
    {
        var result = Run(command, "nested");

        Assert.Equal(2, result.ExitCode);
        Assert.Contains(rule, result.Error, StringComparison.Ordinal);
    }

    /// <summary>
    /// The spellings a word-for-word flag comparison misses. Git bundles short options, so -uf is
    /// the force -f; and a grouped or substituted invocation runs the command just as surely as a
    /// bare one while leaving no whitespace in front of 'git' for the detection regex to find.
    /// </summary>
    [Theory]
    [InlineData("git push -uf origin main", "GIT003")]
    [InlineData("git checkout -f main", "GIT005")]
    [InlineData("git checkout --force main", "GIT005")]
    [InlineData("git clean -xdf", "GIT002")]
    [InlineData("(git reset --hard)", "GIT001")]
    [InlineData("echo $(git reset --hard)", "GIT001")]
    [InlineData("{ git push --force origin main; }", "GIT003")]
    [InlineData("git branch -df old", "GIT004")]
    [InlineData("git branch -d -f old", "GIT004")]
    public void Bundled_flags_and_grouped_invocations_do_not_bypass_the_guard(string command, string rule)
    {
        var result = Run(command, "nested");

        Assert.Equal(2, result.ExitCode);
        Assert.Contains(rule, result.Error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("git status")]
    [InlineData("git clean -nd")]
    [InlineData("git push origin main")]
    [InlineData("git branch --delete merged")]
    [InlineData("git restore --staged src/Changed.cs")]
    [InlineData("git push -u origin main")]
    [InlineData("git push --follow-tags origin main")]
    [InlineData("git checkout main")]
    [InlineData("git checkout -b feature/new-thing")]
    [InlineData("git branch -d merged")]
    [InlineData("git branch -a")]
    [InlineData("dotnet test")]
    public void Non_destructive_commands_are_allowed(string command)
    {
        var result = Run(command, "flat");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void A_block_never_echoes_the_command_or_secrets()
    {
        const string secret = "credential-that-must-not-leak";
        var result = Run($"git push https://{secret}@example.invalid/repo --force", "nested");

        Assert.Equal(2, result.ExitCode);
        Assert.DoesNotContain(secret, result.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("example.invalid", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Unknown_payload_shape_is_allowed_instead_of_scanning_unrelated_fields()
    {
        var result = RunPayload("""{"description":"example: git reset --hard"}""");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>
    /// The documented escape hatch. Without this test, AGENTPACKS_GIT_GUARD=off is only prose.
    /// </summary>
    [Fact]
    public void Guard_off_allows_a_command_that_would_otherwise_be_blocked()
    {
        var result = Run("git reset --hard", "nested", guard: "off");

        Assert.Equal(0, result.ExitCode);
        Assert.DoesNotContain("GIT00", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Posix_and_powershell_guards_declare_the_same_rules()
    {
        var bash = File.ReadAllText(Script);
        var powerShell = File.ReadAllText(PowerShellScript);

        foreach (var rule in Enumerable.Range(1, 6).Select(i => $"GIT{i:000}"))
        {
            Assert.Contains(rule, bash, StringComparison.Ordinal);
            Assert.Contains(rule, powerShell, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The two halves are one hook, so the PowerShell guard is held to the same verdicts rather
    /// than to the presence of six rule ids: a string check cannot see an inverted condition, a
    /// spelling the flag comparison misses, or a parse error. Skipped where pwsh is unavailable
    /// — the point is that CI, which has it, runs this.
    /// </summary>
    [Theory]
    [InlineData("cd /tmp && git -C /repo reset --hard", 2)]
    [InlineData("git clean -fd", 2)]
    [InlineData("git push -uf origin main", 2)]
    [InlineData("git checkout -f main", 2)]
    [InlineData("(git reset --hard)", 2)]
    [InlineData("git branch --delete old --force", 2)]
    [InlineData("git branch -df old", 2)]
    [InlineData("git branch -d merged", 0)]
    [InlineData("git restore src/Changed.cs", 2)]
    [InlineData("git status", 0)]
    [InlineData("git push -u origin main", 0)]
    [InlineData("git checkout -b feature/new-thing", 0)]
    [InlineData("git restore --staged src/Changed.cs", 0)]
    public void The_powershell_guard_reaches_the_same_verdict(string command, int expected)
    {
        if (PowerShell is null)
        {
            // xUnit v2 has no Assert.Skip; this is the runner's dynamic-skip contract.
            throw Xunit.Sdk.SkipException.ForSkip("pwsh is not available");
        }

        var payload = JsonSerializer.Serialize(new { tool_input = new { command } });
        var result = RunPayload(payload, PowerShell, PowerShellArguments());

        Assert.Equal(expected, result.ExitCode);
    }

    private static readonly string PowerShellScript =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "git", "git-guard.ps1");

    /// <summary>Resolved once: the parity theory is skipped rather than failed where pwsh is absent.</summary>
    private static readonly string? PowerShell = ResolvePowerShell();

    private static string? ResolvePowerShell()
    {
        foreach (var candidate in (string[])["pwsh", "powershell"])
        {
            try
            {
                using var probe = Process.Start(new ProcessStartInfo(candidate, "-NoProfile -Command exit 0")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                });

                probe!.WaitForExit();

                if (probe.ExitCode == 0)
                {
                    return candidate;
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Not on PATH. Try the next name.
            }
        }

        return null;
    }

    private static string[] PowerShellArguments() =>
        ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", PowerShellScript, "-Matcher", "git"];

    private static (int ExitCode, string Error) Run(string command, string shape, string guard = "on")
    {
        var payload = shape == "flat"
            ? JsonSerializer.Serialize(new { command })
            : JsonSerializer.Serialize(new { tool_input = new { command } });
        return RunPayload(payload, guard);
    }

    private static (int ExitCode, string Error) RunPayload(string payload, string guard = "on") =>
        RunPayload(payload, "bash", [Script, "-Matcher", "git"], guard);

    private static (int ExitCode, string Error) RunPayload(
        string payload,
        string executable,
        IReadOnlyList<string> arguments,
        string guard = "on")
    {
        var start = new ProcessStartInfo(executable)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["AGENTPACKS_GIT_GUARD"] = guard;

        using var process = Process.Start(start)!;
        process.StandardInput.Write(payload);
        process.StandardInput.Close();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, error);
    }
}
