using System.Diagnostics;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// The pack-check plan linter accepts the round 1 draft and rejects plans that break the contract.
/// </summary>
public sealed class PlanLintTests : IDisposable
{
    private const string Draft = "# Outcome\n\n## Request\nadd x\n\n## Status\ndraft\n\n## Grill log\n### Round 1\nQ1 asked\n";
    private readonly string _directory = Directory.CreateTempSubdirectory("plan-lint-").FullName;

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public void Draft_written_after_round_one_passes()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md", Draft);
        Assert.Equal(0, exit);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Tiny_plan_passes_without_grill_log()
    {
        var (exit, _) = Lint("2026-10-12-typo.md",
            "# Fix typo\n\n## Request\nfix\n\n## Goal\ng\n\n## Decision\nd\n\n## Verify\ndotnet test\n\n## Status\nconfirmed\n");
        Assert.Equal(0, exit);
    }

    [Fact]
    public void Confirmed_plan_missing_sections_fails_with_each_section()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md", Draft.Replace("draft", "confirmed", StringComparison.Ordinal));
        Assert.Equal(1, exit);
        Assert.Contains("missing ## Acceptance criteria", output, StringComparison.Ordinal);
        Assert.Contains("missing ## Verification", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("add-x.md", "file name must be <yyyy-mm-dd>-<slug>.md")]
    [InlineData("2026-10-12-Add_X.md", "file name must be <yyyy-mm-dd>-<slug>.md")]
    public void Name_without_date_slug_fails(string name, string expected)
    {
        var (exit, output) = Lint(name, Draft);
        Assert.Equal(1, exit);
        Assert.Contains(expected, output, StringComparison.Ordinal);
    }

    [Fact]
    public void Em_dash_and_missing_grill_log_fail()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md",
            "# Outcome\n\n## Request\nadd x \u2014 now\n\n## Status\ndraft\n");
        Assert.Equal(1, exit);
        Assert.Contains("contains an em dash", output, StringComparison.Ordinal);
        Assert.Contains("missing ## Grill log", output, StringComparison.Ordinal);
    }

    private (int Exit, string Output) Lint(string name, string text)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, text);
        var start = new ProcessStartInfo("sh") { RedirectStandardOutput = true, UseShellExecute = false };
        start.ArgumentList.Add(Path.Combine(TestRepository.SourceRoot(),
            "plugins", "pack-check", "skills", "pack-check", "scripts", "check-plan.sh"));
        start.ArgumentList.Add(path);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }
}
