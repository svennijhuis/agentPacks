using AgentPacks.Cli.Verification;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// A prior fail must change the next gate. Reading learnings.md without applying it is not enough.
/// </summary>
public sealed class LearningsLogTests
{
    private const string PassedSkip = """
        # Learnings

        ## 2026-09-04 — /build

        - Entrypoint: build
        - Model tier: inherit
        - Agents spun: loop-implementer, loop-verifier
        - Skipped: loop-security-reviewer — no trust boundary
        - Ran: small-change
        - Result: pass
        - Next tweak: keep inherit; skip security when no boundary
        """;

    private const string FailedSkip = """
        # Learnings

        ## 2026-09-04 — /build

        - Entrypoint: build
        - Model tier: inherit
        - Agents spun: loop-implementer, loop-verifier
        - Skipped: loop-security-reviewer — no trust boundary
        - Ran: small-change
        - Result: fail
        - Next tweak: security gate missed a token check; do not skip security
        """;

    [Fact]
    public void A_prior_fail_forces_the_skipped_agent_on_the_next_gate()
    {
        var afterPass = LearningsLog.Advise(PassedSkip, "build");
        var afterFail = LearningsLog.Advise(FailedSkip, "squad");

        Assert.Contains("loop-security-reviewer", afterPass.PreferSkip);
        Assert.DoesNotContain("loop-security-reviewer", afterPass.MustRun);
        Assert.Contains("loop-security-reviewer", afterFail.MustRun);
        Assert.DoesNotContain("loop-security-reviewer", afterFail.PreferSkip);
        Assert.NotEqual(afterPass.MustRun.Contains("loop-security-reviewer"),
            afterFail.MustRun.Contains("loop-security-reviewer"));
    }

    [Fact]
    public void Squad_and_build_share_one_learnings_entrypoint()
    {
        var fromSquadHeading = LearningsLog.Advise("""
            ## 2026-09-05 — /squad

            - Entrypoint: squad
            - Model tier: standard
            - Agents spun: loop-planner
            - Skipped: None
            - Result: pass
            - Next tweak: None
            """, "build");

        Assert.Equal("standard", fromSquadHeading.PreferredTier);
        Assert.Equal("build", LearningsLog.CanonicalEntrypoint("squad"));
    }

    [Fact]
    public void Authored_contract_requires_apply_not_acknowledge()
    {
        var skill = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Fixtures", "delivery-loop", "SKILL.md"));
        var learnings = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Fixtures", "delivery-loop", "learnings.md"));

        Assert.Contains("Apply the latest same-entrypoint entry", skill, StringComparison.Ordinal);
        Assert.Contains("must-run", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not rewrite skills", learnings, StringComparison.OrdinalIgnoreCase);
    }
}
