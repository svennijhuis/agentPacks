using System.Diagnostics;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// check-verify.sh and check-verify.ps1 agree on good and missing-heading fixtures.
/// </summary>
public sealed class VerifyLintTests : IDisposable
{
    private const string Good = """
        # Verify

        ## Launch
        cmd: none

        ## Doctor
        none

        ## Drive
        1. list: curl /pets → 200

        ## Evidence
        $(git rev-parse --git-dir)/squad/verify/demo/

        ## Cleanup
        none; keep evidence
        """;

    private readonly string _directory = Directory.CreateTempSubdirectory("verify-lint-").FullName;

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public void Good_verify_md_passes_in_both_twins()
    {
        var (exit, output) = Lint("verify.md", Good);
        Assert.Equal(0, exit);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Missing_heading_fails_with_identical_twins_output()
    {
        var missingCleanup = """
            # Verify

            ## Launch
            cmd: none

            ## Doctor
            none

            ## Drive
            1. list: curl /pets → 200

            ## Evidence
            $(git rev-parse --git-dir)/squad/verify/demo/
            """;
        var (exit, output) = Lint("verify.md", missingCleanup);
        Assert.Equal(1, exit);
        Assert.Contains("missing ## Cleanup", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Launch")]
    [InlineData("Doctor")]
    [InlineData("Drive")]
    [InlineData("Evidence")]
    [InlineData("Cleanup")]
    public void Each_required_heading_is_reported_when_missing(string section)
    {
        var text = Good.Replace($"## {section}\n", "## Other\n", StringComparison.Ordinal);
        var (exit, output) = Lint("verify.md", text);
        Assert.Equal(1, exit);
        Assert.Contains($"missing ## {section}", output, StringComparison.Ordinal);
    }

    private const string Dash = "\u2014";

    [Fact]
    public void Em_dash_in_verify_md_fails_with_identical_twins_output()
    {
        var (exit, output) = Lint("verify.md", Good.Replace("cmd: none", "cmd: none " + Dash + " later", StringComparison.Ordinal));
        Assert.Equal(1, exit);
        Assert.Contains(":4: em dash", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Em_dash_in_smoke_report_fails_and_clean_report_passes()
    {
        var bad = Lint("verify.md", Good, "smoke-report.md", "# Smoke report\n\nOutcome: pass " + Dash + " ok\n");
        Assert.Equal(1, bad.Exit);
        Assert.Contains("smoke-report.md:3: em dash", bad.Output, StringComparison.Ordinal);
        var clean = Lint("verify.md", Good, "smoke-report.md", "# Smoke report\n\nOutcome: pass\n");
        Assert.Equal(0, clean.Exit);
        Assert.Equal(string.Empty, clean.Output);
    }

    [Fact]
    public void Posix_and_powershell_twins_exist()
    {
        var scripts = Path.Combine(TestRepository.SourceRoot(), "plugins", "pack-check", "skills", "pack-check", "scripts");
        Assert.True(File.Exists(Path.Combine(scripts, "check-verify.sh")));
        Assert.True(File.Exists(Path.Combine(scripts, "check-verify.ps1")));
        var sh = File.ReadAllText(Path.Combine(scripts, "check-verify.sh"));
        var ps1 = File.ReadAllText(Path.Combine(scripts, "check-verify.ps1"));
        foreach (var section in new[] { "Launch", "Doctor", "Drive", "Evidence", "Cleanup", "missing ## " })
        {
            Assert.Contains(section, sh, StringComparison.Ordinal);
            Assert.Contains(section, ps1, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Runs check-verify.sh and, when pwsh is on PATH, check-verify.ps1 on the same file.
    /// </summary>
    private (int Exit, string Output) Lint(string name, string text, string? reportName = null, string? reportText = null)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, text);
        string[] files = [path];
        if (reportName is not null)
        {
            var report = Path.Combine(_directory, reportName);
            File.WriteAllText(report, reportText);
            files = [path, report];
        }
        var posix = Run("sh", [Script("check-verify.sh"), .. files], name);
        if (PwshAvailable.Value)
        {
            var powerShell = Run("pwsh", ["-NoProfile", "-NonInteractive", "-File", Script("check-verify.ps1"), .. files], name);
            Assert.Equal(posix.Exit, powerShell.Exit);
            Assert.Equal(
                posix.Output.Replace("\r", "", StringComparison.Ordinal),
                powerShell.Output.Replace("\r", "", StringComparison.Ordinal));
        }
        return posix;
    }

    private static readonly Lazy<bool> PwshAvailable = new(() =>
    {
        try
        {
            using var probe = Process.Start(new ProcessStartInfo("pwsh", "-NoProfile -Command exit")
            {
                RedirectStandardOutput = true, UseShellExecute = false
            })!;
            probe.WaitForExit(20000);
            return probe.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    });

    private static string Script(string file) => Path.Combine(TestRepository.SourceRoot(),
        "plugins", "pack-check", "skills", "pack-check", "scripts", file);

    private static (int Exit, string Output) Run(string shell, string[] arguments, string name)
    {
        var start = new ProcessStartInfo(shell) { RedirectStandardOutput = true, UseShellExecute = false };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var read = process.StandardOutput.ReadToEndAsync();
        if (!process.WaitForExit(TimeSpan.FromSeconds(20)))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"{shell} check-verify did not finish within 20 s on {name}.");
        }
        return (process.ExitCode, read.Result);
    }
}
