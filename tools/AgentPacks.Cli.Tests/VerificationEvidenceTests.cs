using AgentPacks.Cli.Verification;

namespace AgentPacks.Cli.Tests;

/// <summary>
/// Verify-path gates as executable rules. These are not markdown string matches: a report that
/// looks green in prose still fails here when a gate is missed.
/// </summary>
public sealed class VerificationEvidenceTests
{
    [Fact]
    public void No_plan_stops_and_is_not_a_pass()
    {
        var outcome = VerificationEvidence.Evaluate(new VerificationContext(
            HasConfirmedPlan: false,
            ApplicableStacks: ["dotnet"],
            Report: PassingDotnet()));

        Assert.Equal(VerificationOutcome.StoppedNoPlan, outcome);
    }

    [Fact]
    public void Plan_command_pass_with_wider_suite_fail_is_not_a_verified_pass()
    {
        var report = VerificationEvidence.Parse("""
            ## loop-verifier — round 1

            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet test App.slnx` | `1 passed` |

            **Suite:** fail this-change — 3 failed in OrdersTests
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and max quantity
            """);

        var outcome = VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], report));

        Assert.False(report.WiderSuitePassed);
        Assert.Equal(FailureOrigin.ThisChange, report.WiderSuiteFailureOrigin);
        Assert.Equal(VerificationOutcome.NotPass, outcome);
    }

    [Fact]
    public void Criterion_command_that_never_covers_is_not_verified_and_not_a_pass()
    {
        var claimedPass = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | — | assumed from reading the code |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input
            """);

        var honest = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | not verified | — | No automated or safe manual check covers this criterion. |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input
            """);

        Assert.False(VerificationEvidence.IsCoveringCommand(claimedPass.Criteria[0].Command));
        Assert.Equal(VerificationEvidence.NotVerified, honest.Criteria[0].Result);
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], claimedPass)));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], honest)));
    }

    [Fact]
    public void Pre_existing_and_this_change_failures_are_distinguished()
    {
        var preexisting = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet test App.slnx` | `1 passed` |

            **Suite:** fail pre-existing — BillingTests red on main
            **Stacks:** dotnet
            **Coverage:** empty input
            """);

        var introduced = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | fail | `dotnet test App.slnx` | `expected 401, got 200` |

            **Suite:** fail this-change
            **Stacks:** dotnet
            **Coverage:** empty input
            """);

        Assert.Equal(FailureOrigin.PreExisting, preexisting.WiderSuiteFailureOrigin);
        Assert.Equal(FailureOrigin.ThisChange, introduced.WiderSuiteFailureOrigin);
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], preexisting)));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], introduced)));
    }

    [Fact]
    public void Mixed_dotnet_and_rust_requires_both_suites_and_the_boundary()
    {
        var rustOnly = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `cargo test --workspace` | `ok` |

            **Suite:** pass
            **Stacks:** rust
            **Boundary:** not covered
            **Coverage:** empty input and lock poison
            """);

        var bothNoBoundary = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet test App.slnx` | `ok` |
            | 2 | pass | `cargo test --workspace` | `ok` |

            **Suite:** pass
            **Stacks:** dotnet rust
            **Boundary:** not covered
            **Coverage:** empty input and lock poison
            """);

        var bothWithBoundary = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet test App.slnx` | `ok` |
            | 2 | pass | `cargo test --workspace` | `ok` |

            **Suite:** pass
            **Stacks:** dotnet rust
            **Boundary:** covered — FFI round-trip test
            **Coverage:** empty input and lock poison
            """);

        var stacks = new[] { "dotnet", "rust" };

        Assert.Equal(["rust"], rustOnly.StacksVerified);
        Assert.False(rustOnly.BoundaryVerified);
        Assert.False(bothNoBoundary.BoundaryVerified);
        Assert.True(bothWithBoundary.BoundaryVerified);
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, stacks, rustOnly)));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, stacks, bothNoBoundary)));
        Assert.Equal(VerificationOutcome.Pass,
            VerificationEvidence.Evaluate(new VerificationContext(true, stacks, bothWithBoundary)));
    }

    [Fact]
    public void Happy_path_only_agent_written_tests_are_rejected()
    {
        var happyOnly = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet test App.slnx` | `1 passed` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** happy-path only
            """);

        var withEdges = PassingDotnet();

        Assert.False(happyOnly.IncludesEdgeCases);
        Assert.True(withEdges.IncludesEdgeCases);
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], happyOnly)));
        Assert.Equal(VerificationOutcome.Pass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], withEdges)));
    }

    private static VerificationReport PassingDotnet() =>
        VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet test App.slnx` | `2 passed` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and duplicate id
            """);
}
