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
        var (exit, output) = Lint("2026-10-12-add-x.md", Draft.Replace("draft", "confirmed", StringComparison.Ordinal) + "\nDesign: skipped (small)\n");
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

    private const string Confirmed =
        "# Outcome\n\n## Request\nadd x\n\n## Status\nconfirmed\n\n## Grill log\n### Round 1\nq\n\n"
        + "## Decisions\n\n## Acceptance criteria\n\n## Test plan matrix\n\n## Open questions\nNone.\n\n## Verification\ndotnet test\n";

    [Fact]
    public void Confirmed_full_plan_needs_design_and_map()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md", Confirmed + "\n## Design\nRule fired: public API\n");
        Assert.Equal(1, exit);
        Assert.Contains("missing ## Map", output, StringComparison.Ordinal);
        Assert.Equal(0, Lint("2026-10-12-add-x.md", Confirmed + "\n## Design\nx\n\n## Map\n| a |\n").Exit);
    }

    [Fact]
    public void Small_full_plan_skips_design_only_when_stated()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md", Confirmed);
        Assert.Equal(1, exit);
        Assert.Contains("missing ## Design or 'Design: skipped (small)'", output, StringComparison.Ordinal);
        Assert.Equal(0, Lint("2026-10-12-add-x.md", Confirmed + "\nDesign: skipped (small)\n").Exit);
        Assert.Equal(0, Lint("2026-10-12-add-x.md", Confirmed + "\n## Design\n\nDesign: skipped (small)\n").Exit);
    }

    [Fact]
    public void Posix_and_powershell_twins_exist_and_check_the_same_sections()
    {
        var scripts = Path.Combine(TestRepository.SourceRoot(), "plugins", "pack-check", "skills", "pack-check", "scripts");
        var sh = File.ReadAllText(Path.Combine(scripts, "check-plan.sh"));
        var ps1 = File.ReadAllText(Path.Combine(scripts, "check-plan.ps1"));
        foreach (var section in new[]
                 {
                     "Request", "Goal", "Decision", "Verify", "Grill log", "Decisions", "Acceptance criteria",
                     "Test plan matrix", "Open questions", "Verification", "Design", "Map", "Design: skipped (small)",
                     "file name must be <yyyy-mm-dd>-<slug>.md", "contains an em dash", "missing ## Status value",
                     "### Hard decisions", "hard decision without a docs/adr/ link", "ADR link must be docs/adr/NNNN-", "## Map has no fenced mermaid block", "slice has no files line", "slice has no done check"
                 })
        {
            Assert.Contains(section, sh, StringComparison.Ordinal);
            Assert.Contains(section, ps1, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Status_followed_only_by_blank_lines_ends_with_a_finding_instead_of_hanging()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md", "# Outcome\n\n## Request\nx\n\n## Grill log\n\n## Status\n\n\n");
        Assert.Equal(1, exit);
        Assert.Contains("missing ## Status value", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Crlf_plan_passes_like_its_lf_twin()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md", Draft.Replace("\n", "\r\n", StringComparison.Ordinal));
        Assert.Equal(0, exit);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void Hard_decision_without_adr_link_is_flagged_and_with_link_passes()
    {
        var plan = Confirmed + "\n## Design\n### Hard decisions\nNew queue dependency\n\n## Map\n| a |\n";
        var (exit, output) = Lint("2026-10-12-add-x.md", plan);
        Assert.Equal(1, exit);
        Assert.Contains("hard decision without a docs/adr/ link", output, StringComparison.Ordinal);
        var linked = plan.Replace("New queue dependency", "New queue dependency: docs/adr/0002-add-x.md", StringComparison.Ordinal);
        Assert.Equal(0, Lint("2026-10-12-add-x.md", linked).Exit);
        Assert.Equal(0, Lint("2026-10-12-add-x.md", plan.Replace("New queue dependency", "None", StringComparison.Ordinal)).Exit);
    }

    [Fact]
    public void Adr_link_must_carry_the_plan_slug()
    {
        var plan = Confirmed + "\n## Design\n### Hard decisions\nQueue: docs/adr/0002-queue.md\n\n## Map\n| a |\n";
        var (exit, output) = Lint("2026-10-12-add-x.md", plan);
        Assert.Equal(1, exit);
        Assert.Contains("ADR link must be docs/adr/NNNN-add-x.md", output, StringComparison.Ordinal);
        Assert.Equal(0, Lint("2026-10-12-add-x.md", plan.Replace("0002-queue.md", "0002-add-x-queue.md", StringComparison.Ordinal)).Exit);
    }

    [Fact]
    public void Map_without_mermaid_warns_but_still_passes()
    {
        var (exit, output) = Lint("2026-10-12-add-x.md", Confirmed + "\n## Design\nx\n\n## Map\n| a |\n");
        Assert.Equal(0, exit);
        Assert.Contains("warning: ## Map has no fenced mermaid block", output, StringComparison.Ordinal);
        var withDiagram = Confirmed + "\n## Design\nx\n\n## Map\n| a |\n\n```mermaid\nflowchart LR\n  A --> B\n```\nCaption.\n";
        Assert.Equal((0, string.Empty), Lint("2026-10-12-add-x.md", withDiagram));
    }

    [Fact]
    public void Planning_contract_template_lints_clean_in_both_twins()
    {
        var contract = File.ReadAllText(Path.Combine(TestRepository.SourceRoot(), "plugins", "squad", "skills", "squad", "references", "planning-contract.md"))
            .Replace("\r", "", StringComparison.Ordinal);
        var start = contract.IndexOf("````markdown\n", StringComparison.Ordinal);
        Assert.True(start >= 0, "plan template fence missing");
        start += "````markdown\n".Length;
        var template = contract[start..contract.IndexOf("\n````", start, StringComparison.Ordinal)] + "\n";
        Assert.Equal((0, string.Empty), Lint("2026-10-13-template.md", template));
    }

    [Fact]
    public void One_slice_needs_a_single_slice_reason_in_both_twins()
    {
        Assert.True(PwshAvailable.Value, "pwsh must be on PATH so both twins run.");
        var plan = Confirmed + "\nDesign: skipped (small)\n\n## Slices\n1. Client. Files: src/Api/A.cs, src/Infra/B.cs. Done when A_works is green.\n";
        var (exit, output) = Lint("2026-10-13-add-x.md", plan);
        Assert.Equal(0, exit);
        Assert.Contains("warning: one slice and no 'Single slice because:' line", output, StringComparison.Ordinal);
        Assert.Equal((0, string.Empty), Lint("2026-10-13-add-x.md", plan + "Single slice because: one project only.\n"));
        var two = plan + "2. Adapter. Files: src/Infra/B.cs. Done when B_works is green.\n";
        Assert.Equal((0, string.Empty), Lint("2026-10-13-add-x.md", two));
    }

    [Fact]
    public void Slice_files_and_done_match_case_insensitively()
    {
        var plan = Confirmed + "\nDesign: skipped (small)\n\n## Slices\n1. Client. Files: src/A.cs. Test: A_works. Done when A_works is green.\nSingle slice because: one project only.\n";
        Assert.Equal((0, string.Empty), Lint("2026-10-12-add-x.md", plan));
    }

    [Fact]
    public void Slice_without_files_or_done_check_warns_but_still_passes()
    {
        var plan = Draft + "\n## Slices\n1. Port: criteria 1; files src/Port.cs; test PortTests; done when PortTests is green\n2. Adapter: criteria 2\n";
        var (exit, output) = Lint("2026-10-12-add-x.md", plan);
        Assert.Equal(0, exit);
        Assert.Contains("warning: slice has no files line: 2. Adapter: criteria 2", output, StringComparison.Ordinal);
        Assert.Contains("warning: slice has no done check: 2. Adapter: criteria 2", output, StringComparison.Ordinal);
        Assert.DoesNotContain("1. Port", output, StringComparison.Ordinal);
    }

    private const string CleanRetro = "# Retro: add x\n\n## Summary\nResult: pass. Last SHA abc1234.\n\n## What went wrong\nNone\n";

    [Fact]
    public void Retro_with_an_em_dash_fails_in_both_twins()
    {
        Assert.True(PwshAvailable.Value, "pwsh must be on PATH so both twins run.");
        var (exit, output) = Lint("2026-10-13-add-x.md", CleanRetro + "\nOne lesson \u2014 quoted from a vendor.\n", "check-retro");
        Assert.Equal(1, exit);
        Assert.Contains("line 9 contains an em dash", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Clean_retro_passes_in_both_twins()
    {
        Assert.True(PwshAvailable.Value, "pwsh must be on PATH so both twins run.");
        Assert.Equal((0, string.Empty), Lint("2026-10-13-add-x.md", CleanRetro, "check-retro"));
        Assert.Equal(1, Lint("retro.md", CleanRetro, "check-retro").Exit);
        Assert.Equal((0, string.Empty), Lint("2026-10-13-add-x-interrupted.md", CleanRetro, "check-retro"));
    }

    /// <summary>
    /// Runs check-plan.sh and, when pwsh is on PATH, check-plan.ps1 on the same file, and requires
    /// both twins to agree on exit code and findings. Every case in this class is a parity case.
    /// </summary>
    private (int Exit, string Output) Lint(string name, string text, string script = "check-plan")
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, text);
        var posix = Run("sh", [Script(script + ".sh"), path], name);
        if (PwshAvailable.Value)
        {
            var powerShell = Run("pwsh", ["-NoProfile", "-NonInteractive", "-File", Script(script + ".ps1"), path], name);
            Assert.Equal(posix.Exit, powerShell.Exit);
            Assert.Equal(posix.Output.Replace("\r", "", StringComparison.Ordinal), powerShell.Output.Replace("\r", "", StringComparison.Ordinal));
        }
        return posix;
    }

    private static readonly Lazy<bool> PwshAvailable = new(() =>
    {
        try
        {
            using var probe = Process.Start(new ProcessStartInfo("pwsh", "-NoProfile -Command exit") { RedirectStandardOutput = true, UseShellExecute = false })!;
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
            Assert.Fail($"{shell} check-plan did not finish within 20 s on {name}.");
        }
        return (process.ExitCode, read.Result);
    }
}
