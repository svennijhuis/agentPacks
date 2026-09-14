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
            ## squad-verifier — round 1

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
        var emptyCommand = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass |  | assumed from reading the code |

            **Suite:** pass
            **Stacks:** dotnet
            **Coverage:** empty input
            """);

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

        Assert.False(VerificationEvidence.IsCoveringCommand(emptyCommand.Criteria[0].Command));
        Assert.False(VerificationEvidence.IsCoveringCommand(claimedPass.Criteria[0].Command));
        Assert.Equal(VerificationEvidence.NotVerified, honest.Criteria[0].Result);
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], emptyCommand)));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], claimedPass)));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], honest)));
    }

    /// <summary>
    /// Item 65: a check that exists but could not run (environment, missing secret) is
    /// <c>blocked</c> — distinct from <c>not verified</c> (no check) and <c>fail</c>, and never a pass.
    /// </summary>
    [Fact]
    public void Blocked_row_is_parsed_distinct_from_not_verified_and_is_not_a_pass()
    {
        var blocked = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet test App.slnx` | `2 passed` |
            | 2 | blocked | `dotnet test App.slnx --filter Vault` | `Key Vault 403 on local; the check did not run` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and duplicate id
            """);

        Assert.Equal(2, blocked.Criteria.Count);
        Assert.Equal(VerificationEvidence.Blocked, blocked.Criteria[1].Result);
        Assert.NotEqual(VerificationEvidence.NotVerified, blocked.Criteria[1].Result);
        Assert.NotEqual(VerificationEvidence.Fail, blocked.Criteria[1].Result);
        Assert.True(VerificationEvidence.IsCoveringCommand(blocked.Criteria[1].Command));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], blocked)));
    }

    /// <summary>
    /// Item 68 (from orchestrate's <c>type-check-only</c> tier): a build, restore, format, or
    /// type-check proves the code compiles, nothing about behavior. A pass row on such a command is
    /// not a pass unless the criterion itself is about compiling. A chain that also runs the code
    /// (<c>dotnet build &amp;&amp; dotnet test</c>) is not compile-only.
    /// </summary>
    [Fact]
    public void Compile_only_command_is_not_evidence_for_a_behavioral_criterion()
    {
        var buildOnly = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet build App.slnx --no-restore` | `Build succeeded. 0 Warning(s)` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and duplicate id
            """);

        var buildThenTest = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet build App.slnx && dotnet test App.slnx --no-build` | `10 passed, 0 failed` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and duplicate id
            """);

        var httpProjectBuild = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet build App.Http.csproj` | `Build succeeded. 0 Warning(s)` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and duplicate id
            """);

        Assert.True(VerificationEvidence.IsCompileOnlyCommand("dotnet build App.slnx"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("`cargo check --all-targets`"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("tsc --noEmit -p tsconfig.json"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("npm run build"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("dotnet format --verify-no-changes"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("dotnet build App.Http.csproj"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("dotnet build App.Tests.csproj --no-restore"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("dotnet build Bench.csproj"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("cargo build -p http"));
        Assert.True(VerificationEvidence.IsCompileOnlyCommand("npm run build -- --filter=@scope/http-client"));
        Assert.False(VerificationEvidence.IsCompileOnlyCommand("dotnet test App.slnx"));
        Assert.False(VerificationEvidence.IsCompileOnlyCommand("cargo run --bin cli -- --help"));
        Assert.False(VerificationEvidence.IsCompileOnlyCommand("dotnet build App.slnx && dotnet test App.slnx --no-build"));
        Assert.False(VerificationEvidence.IsCompileOnlyCommand("curl -s localhost:5000/health"));
        Assert.False(VerificationEvidence.IsCompileOnlyCommand("curl https://example.com/health"));

        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], buildOnly)));
        Assert.Equal(VerificationOutcome.Pass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], buildOnly, CompileCriteria: [1])));
        Assert.Equal(VerificationOutcome.Pass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], buildThenTest)));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], httpProjectBuild)));
        Assert.Equal(VerificationOutcome.Pass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], httpProjectBuild, CompileCriteria: [1])));
    }

    /// <summary>
    /// Item 66: a quantitative criterion passes only when the verifier quotes its own measured
    /// value against the bound. A pass count is not a measurement; the implementer's claim is not evidence.
    /// </summary>
    [Fact]
    public void Quantitative_criterion_needs_a_measured_value_against_the_bound()
    {
        var passCountOnly = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet run --project Bench -- p95` | `2 passed` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and cold cache
            """);

        var measured = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet run --project Bench -- p95` | `p95 143 ms ≤ 200 ms` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and cold cache
            """);

        var restatedBound = VerificationEvidence.Parse("""
            | Criterion | Result | Command | Evidence |
            |---|---|---|---|
            | 1 | pass | `dotnet run --project Bench -- p95` | `p95 ≤ 200 ms` |

            **Suite:** pass
            **Stacks:** dotnet
            **Boundary:** not covered
            **Coverage:** empty input and cold cache
            """);

        Assert.False(VerificationEvidence.IsMeasuredAgainstBound("2 passed"));
        Assert.False(VerificationEvidence.IsMeasuredAgainstBound("BenchTests.P95_under_budget green"));
        Assert.False(VerificationEvidence.IsMeasuredAgainstBound("412 → 354"));
        Assert.False(VerificationEvidence.IsMeasuredAgainstBound("p95 ≤ 200 ms"));
        Assert.True(VerificationEvidence.IsMeasuredAgainstBound("p95 143 ms ≤ 200 ms"));
        Assert.True(VerificationEvidence.IsMeasuredAgainstBound("`bundle 2.39 MB < 2.4 MB`"));
        Assert.True(VerificationEvidence.IsMeasuredAgainstBound("throughput 1,250 rps >= 1000"));

        Assert.Equal(VerificationOutcome.Pass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], passCountOnly)));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], passCountOnly, QuantitativeCriteria: [1])));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], measured, QuantitativeCriteria: [1, 2])));
        Assert.Equal(VerificationOutcome.Pass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], measured, QuantitativeCriteria: [1])));
        Assert.Equal(VerificationOutcome.NotPass,
            VerificationEvidence.Evaluate(new VerificationContext(true, ["dotnet"], restatedBound, QuantitativeCriteria: [1])));
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

    /// <summary>
    /// Item 53: a matrix with a blank or unconfirmed Seam is not verified.
    /// Tests that hit internals not named in the Seam column fail.
    /// </summary>
    [Fact]
    public void Blank_or_unconfirmed_seam_and_unnamed_internal_hits_are_not_verified()
    {
        const string named = """
            | Criterion | Happy | Edge | Fail | Kind | Seam |
            |---|---|---|---|---|---|
            | 1 | create | empty | 400 | unit | `POST /orders` |
            """;

        const string blank = """
            | Criterion | Happy | Edge | Fail | Kind | Seam |
            |---|---|---|---|---|---|
            | 1 | create | empty | 400 | unit |  |
            """;

        const string unconfirmed = """
            | Criterion | Happy | Edge | Fail | Kind | Seam |
            |---|---|---|---|---|---|
            | 1 | create | empty | 400 | unit | unconfirmed |
            """;

        const string missingColumn = """
            | Criterion | Happy | Edge | Fail | Kind |
            |---|---|---|---|---|
            | 1 | create | empty | 400 | unit |
            """;

        const string mixedShortRow = """
            | Criterion | Happy | Edge | Fail | Kind | Seam |
            |---|---|---|---|---|---|
            | 1 | create | empty | 400 | unit | `POST /orders` |
            | 2 | list | empty | 404 | unit |
            """;

        Assert.Equal(VerificationEvidence.NotVerified, TestPlanSeam.ResultForSeam(""));
        Assert.Equal(VerificationEvidence.NotVerified, TestPlanSeam.ResultForSeam("unconfirmed"));
        Assert.Equal(VerificationEvidence.NotVerified, TestPlanSeam.ResultForSeam("None"));
        Assert.Equal(VerificationEvidence.NotVerified, TestPlanSeam.ResultForSeam("none"));
        Assert.Equal(VerificationEvidence.NotVerified, TestPlanSeam.ResultForSeam("—"));
        Assert.Equal(VerificationEvidence.NotVerified, TestPlanSeam.ResultForSeam("N/A"));
        Assert.Equal(VerificationEvidence.Pass, TestPlanSeam.ResultForSeam("POST /orders"));

        Assert.Equal(VerificationOutcome.NotPass, TestPlanSeam.Evaluate(blank, ["POST /orders"]));
        Assert.Equal(VerificationOutcome.NotPass, TestPlanSeam.Evaluate(unconfirmed, ["POST /orders"]));
        Assert.Equal(VerificationOutcome.NotPass, TestPlanSeam.Evaluate(missingColumn, ["POST /orders"]));
        Assert.Equal(VerificationOutcome.NotPass, TestPlanSeam.Evaluate(mixedShortRow, ["POST /orders"]));
        Assert.Equal(
            VerificationOutcome.NotPass,
            TestPlanSeam.Evaluate(named, ["Orders.internal.Validate"]));
        Assert.Equal(VerificationOutcome.Pass, TestPlanSeam.Evaluate(named, ["POST /orders"]));
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
