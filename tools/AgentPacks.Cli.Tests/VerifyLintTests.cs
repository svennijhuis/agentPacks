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

    private const string GoodWithInlineToken = """
        # Verify

        ## Launch
        cmd: none

        ## Doctor
        none

        ## Drive
        1. list: curl -sS -H "Authorization: Bearer $(az account get-access-token --scope https://vault.azure.net/.default --query accessToken -o tsv)" http://localhost/pets

        ## Evidence
        $(git rev-parse --git-dir)/squad/verify/demo/

        ## Cleanup
        none; keep evidence

        ## Local access
        provider: azure
        scope: https://vault.azure.net/.default
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
        Assert.Contains("secret-looking value", sh, StringComparison.Ordinal);
        Assert.Contains("secret-looking value", ps1, StringComparison.Ordinal);
        Assert.Contains("get-access-token outside", sh, StringComparison.Ordinal);
        Assert.Contains("get-access-token outside", ps1, StringComparison.Ordinal);
    }

    [Fact]
    public void Optional_local_access_section_without_secrets_passes()
    {
        var (exit, output) = Lint("verify.md", GoodWithInlineToken);
        Assert.Equal(0, exit);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Fake_jwt_in_local_access_fails_without_printing_secret()
    {
        const string fake = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.fakesig";
        var text = GoodWithInlineToken + "\ntoken: " + fake + "\n";
        var (exit, output) = Lint("verify.md", text);
        Assert.Equal(1, exit);
        Assert.Contains("secret-looking value", output, StringComparison.Ordinal);
        Assert.DoesNotContain(fake, output, StringComparison.Ordinal);
    }

    [Fact]
    public void Fake_jwt_in_drive_section_fails_in_both_twins()
    {
        const string fake = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.fakesig";
        var text = Good.Replace(
            "1. list: curl /pets → 200",
            "1. list: curl /pets Authorization: Bearer " + fake,
            StringComparison.Ordinal);
        var (exit, output) = Lint("verify.md", text);
        Assert.Equal(1, exit);
        Assert.Contains("secret-looking value", output, StringComparison.Ordinal);
        Assert.DoesNotContain(fake, output, StringComparison.Ordinal);
    }

    [Fact]
    public void Fake_token_in_smoke_report_fails()
    {
        const string fake = "AKIAIOSFODNN7EXAMPLE0";
        var (exit, output) = Lint(
            "verify.md",
            Good,
            "smoke-report.md",
            "# Smoke\n\nOutcome: skipped key=" + fake + "\n");
        Assert.Equal(1, exit);
        Assert.Contains("smoke-report.md:", output, StringComparison.Ordinal);
        Assert.Contains("secret-looking value", output, StringComparison.Ordinal);
        Assert.DoesNotContain(fake, output, StringComparison.Ordinal);
    }

    [Fact]
    public void Fake_token_in_evidence_file_fails()
    {
        const string fake = "ya29.a0AfH6SMCfaketokengoogleoauth";
        var (exit, output) = Lint(
            "verify.md",
            Good,
            "smoke-report.md",
            "# Smoke\nOutcome: pass\n",
            ("transcript.txt", "startup: " + fake + "\n"));
        Assert.Equal(1, exit);
        Assert.Contains("transcript.txt:", output, StringComparison.Ordinal);
        Assert.Contains("secret-looking value", output, StringComparison.Ordinal);
        Assert.DoesNotContain(fake, output, StringComparison.Ordinal);
    }

    [Fact]
    public void Get_access_token_outside_command_substitution_fails()
    {
        var text = Good.Replace(
            "1. list: curl /pets → 200",
            "1. list: az account get-access-token --scope x",
            StringComparison.Ordinal);
        var (exit, output) = Lint("verify.md", text);
        Assert.Equal(1, exit);
        Assert.Contains("get-access-token outside $(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Get_access_token_outside_substitution_in_smoke_report_fails()
    {
        var (exit, output) = Lint(
            "verify.md",
            Good,
            "smoke-report.md",
            "# Smoke\n\nManual: az account get-access-token --scope x\n");
        Assert.Equal(1, exit);
        Assert.Contains("smoke-report.md:", output, StringComparison.Ordinal);
        Assert.Contains("get-access-token outside $(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Get_access_token_outside_substitution_in_evidence_file_fails()
    {
        var (exit, output) = Lint(
            "verify.md",
            Good,
            "smoke-report.md",
            "# Smoke\nOutcome: pass\n",
            ("transcript.txt", "cmd: az account get-access-token --scope x\n"));
        Assert.Equal(1, exit);
        Assert.Contains("transcript.txt:", output, StringComparison.Ordinal);
        Assert.Contains("get-access-token outside $(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_get_access_token_in_command_substitution_passes()
    {
        var (exit, output) = Lint("verify.md", GoodWithInlineToken);
        Assert.Equal(0, exit);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Good_verify_md_plus_skill_smoke_report_passes_in_both_twins()
    {
        var report = File.ReadAllText(Path.Combine(
            TestRepository.SourceRoot(),
            "plugins", "squad", "skills", "verify-harness", "references", "smoke-report.md"));
        var (exit, output) = Lint("verify.md", Good, "smoke-report.md", report);
        Assert.Equal(0, exit);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Evidence_path_and_git_sha_pass_long_base64_fails()
    {
        const string sha = "abcdef0123456789abcdef0123456789abcdef01";
        var withPathAndSha = Good.Replace(
            "$(git rev-parse --git-dir)/squad/verify/demo/",
            ".git/squad/verify/pets-api/20261013-2300/scenario-name-long-enough.txt\ncommit: " + sha,
            StringComparison.Ordinal);
        var clean = Lint("verify.md", withPathAndSha);
        Assert.Equal(0, clean.Exit);
        Assert.Equal(string.Empty, clean.Output);

        const string fake = "dGhpc2lzYWZha2Vsb25nYmFzZTY0dG9rZW5mb3JsaW50dGVzdA==";
        var dirty = Lint("verify.md", withPathAndSha + "\ntoken: " + fake + "\n");
        Assert.Equal(1, dirty.Exit);
        Assert.Contains("secret-looking value", dirty.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(fake, dirty.Output, StringComparison.Ordinal);
    }

    /// <summary>
    /// Runs check-verify.sh and, when pwsh is on PATH, check-verify.ps1 on the same file.
    /// </summary>
    private (int Exit, string Output) Lint(
        string name,
        string text,
        string? reportName = null,
        string? reportText = null,
        params (string Name, string Text)[] evidence)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, text);
        var files = new List<string> { path };
        if (reportName is not null)
        {
            var report = Path.Combine(_directory, reportName);
            File.WriteAllText(report, reportText);
            files.Add(report);
        }
        foreach (var (evidenceName, evidenceText) in evidence)
        {
            var evidencePath = Path.Combine(_directory, evidenceName);
            File.WriteAllText(evidencePath, evidenceText);
            files.Add(evidencePath);
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
