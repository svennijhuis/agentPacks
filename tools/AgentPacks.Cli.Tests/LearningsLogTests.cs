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
        - Provider: cursor
        - Model tier: inherit
        - Agents spun: squad-implementer, squad-verifier
        - Skipped: squad-security-reviewer — no trust boundary
        - Ran: small-change
        - Result: pass
        - Next tweak: keep inherit; skip security when no boundary
        """;

    private const string FailedSkip = """
        # Learnings

        ## 2026-09-04 — /build

        - Entrypoint: build
        - Provider: cursor
        - Model tier: inherit
        - Agents spun: squad-implementer, squad-verifier
        - Skipped: squad-security-reviewer — no trust boundary
        - Ran: small-change
        - Result: fail
        - Next tweak: security gate missed a token check; do not skip security
        """;

    [Fact]
    public void A_prior_fail_forces_the_skipped_agent_on_the_next_gate()
    {
        var afterPass = LearningsLog.Advise(PassedSkip, "build");
        var afterFail = LearningsLog.Advise(FailedSkip, "squad");

        Assert.Contains("squad-security-reviewer", afterPass.PreferSkip);
        Assert.DoesNotContain("squad-security-reviewer", afterPass.MustRun);
        Assert.Contains("squad-security-reviewer", afterFail.MustRun);
        Assert.DoesNotContain("squad-security-reviewer", afterFail.PreferSkip);
        Assert.NotEqual(afterPass.MustRun.Contains("squad-security-reviewer"),
            afterFail.MustRun.Contains("squad-security-reviewer"));
    }

    [Fact]
    public void Squad_and_build_share_one_learnings_entrypoint()
    {
        var fromSquadHeading = LearningsLog.Advise("""
            ## 2026-09-05 — /squad

            - Entrypoint: squad
            - Provider: claude
            - Model tier: standard
            - Agents spun: squad-planner
            - Skipped: None
            - Result: pass
            - Next tweak: None
            """, "build");

        Assert.Equal("standard", fromSquadHeading.PreferredTier);
        Assert.Equal("squad", LearningsLog.CanonicalEntrypoint("build"));
        Assert.Equal("squad", LearningsLog.CanonicalEntrypoint("squad"));
        Assert.Equal("squad-review", LearningsLog.CanonicalEntrypoint("review"));
        Assert.Equal("squad-review", LearningsLog.CanonicalEntrypoint("squad-review"));
        Assert.Equal("claude", LearningsLog.Parse("""
            ## 2026-09-05 — /squad

            - Entrypoint: squad
            - Provider: claude
            - Model tier: standard
            - Result: pass
            """)[0].Provider);
        Assert.Equal("squad-review", LearningsLog.Parse("""
            ## 2026-09-05 — /squad-review

            - Entrypoint: squad-review
            - Provider: cursor
            - Model tier: inherit
            - Result: pass
            """)[0].Entrypoint);
    }

    [Fact]
    public void A_prior_fail_demotes_one_model_tier()
    {
        var afterFrontierFail = LearningsLog.Advise("""
            ## 2026-09-05 — /build

            - Entrypoint: build
            - Provider: copilot
            - Model tier: frontier
            - Skipped: None
            - Result: fail
            """, "build");
        var afterInheritFail = LearningsLog.Advise("""
            ## 2026-09-05 — /review

            - Entrypoint: review
            - Provider: cursor
            - Model tier: inherit
            - Skipped: None
            - Result: stopped
            """, "squad-review");
        var afterStandardPass = LearningsLog.Advise("""
            ## 2026-09-05 — /build

            - Entrypoint: build
            - Provider: cursor
            - Model tier: standard
            - Skipped: None
            - Result: pass
            """, "build");

        Assert.Equal("standard", afterFrontierFail.PreferredTier);
        Assert.Equal("inherit", afterInheritFail.PreferredTier);
        Assert.Equal("standard", afterStandardPass.PreferredTier);
        Assert.Equal("fast", LearningsLog.DemoteTier("standard"));
        Assert.Equal("inherit", LearningsLog.DemoteTier("fast"));
        Assert.Equal("inherit", LearningsLog.DemoteTier("inherit"));
    }

    [Fact]
    public void Authored_contract_requires_apply_not_acknowledge()
    {
        var skill = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Fixtures", "squad", "SKILL.md"));
        var learnings = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Fixtures", "squad", "learnings.md"));

        Assert.Contains(
            "Read and apply learnings.md (append-only): prefer passed skips/tiers; avoid what failed",
            skill,
            StringComparison.Ordinal);
        Assert.Contains("Apply the latest same-entrypoint entry", skill, StringComparison.Ordinal);
        Assert.Contains("must-run", skill, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not rewrite skills", learnings, StringComparison.OrdinalIgnoreCase);
    }
}
