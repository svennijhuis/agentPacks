using System.Text.RegularExpressions;

namespace AgentPacks.Cli.Verification;

/// <summary>
/// Independent evaluation of a verifier report. Prompt text is not evidence: these rules decide
/// whether a planned change is a verified pass, and they are what the tests exercise.
/// </summary>
public enum VerificationOutcome
{
    /// <summary>No confirmed plan exists; verification must stop.</summary>
    StoppedNoPlan,

    /// <summary>The report cannot support a pass verdict.</summary>
    NotPass,

    /// <summary>Every gate that can fail has been evidenced as passing.</summary>
    Pass
}

/// <summary>How a failing command relates to this change.</summary>
public enum FailureOrigin
{
    Unspecified,
    ThisChange,
    PreExisting
}

/// <summary>One acceptance-criterion row from a verifier report.</summary>
public sealed record VerificationCriterion(
    int Number,
    string Result,
    string Command,
    string Evidence);

/// <summary>Structured fields the verifier must supply beyond the criterion table.</summary>
public sealed record VerificationReport(
    IReadOnlyList<VerificationCriterion> Criteria,
    bool WiderSuitePassed,
    FailureOrigin WiderSuiteFailureOrigin,
    IReadOnlyList<string> StacksVerified,
    bool BoundaryVerified,
    bool IncludesEdgeCases);

/// <summary>
/// Inputs that are not in the report itself but gate the verdict. <paramref name="QuantitativeCriteria"/>
/// names the plan criteria that state a bound; their Evidence must quote a measured value against it.
/// <paramref name="CompileCriteria"/> names the plan criteria whose stated outcome is that the code
/// compiles; only those may pass on a build-only command.
/// </summary>
public sealed record VerificationContext(
    bool HasConfirmedPlan,
    IReadOnlyList<string> ApplicableStacks,
    VerificationReport? Report,
    IReadOnlyList<int>? QuantitativeCriteria = null,
    IReadOnlyList<int>? CompileCriteria = null);

/// <summary>Parses the review-contract verifier report and applies the pass gates.</summary>
public static partial class VerificationEvidence
{
    public const string Pass = "pass";
    public const string Fail = "fail";
    public const string NotVerified = "not verified";

    /// <summary>
    /// A check exists but the environment or a missing secret stopped it. Unproven, not failed:
    /// the remedy is to unblock and rerun, not to write a new check. Never a pass.
    /// </summary>
    public const string Blocked = "blocked";

    /// <summary>
    /// A <c>needs:</c>-tagged Doctor check failed (with reason). Does not block a pass; the rest of
    /// the run continues. Never a stand-in for an undriven scenario (<see cref="NotRun"/>).
    /// </summary>
    public const string Skipped = "skipped";

    /// <summary>
    /// A touched scenario or check was not driven. Blocks a pass. Distinct from <see cref="Skipped"/>.
    /// </summary>
    public const string NotRun = "not run";

    // Known Result values first; final alternative keeps unknown values so Evaluate can reject them
    // instead of silently dropping the row.
    [GeneratedRegex(
        @"^\|\s*(\d+)\s*\|\s*(pass|fail|not verified|blocked|skipped|not run|[^|\n]+?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex CriterionRow { get; }

    // Digits glued to a metric name (`p95 ≤ 200`) are not a measurement. Before→after arrows excluded.
    [GeneratedRegex(@"(?<![A-Za-z0-9.])\d[\d.,]*\s*[A-Za-z%µ/]*\s*(<=|>=|≤|≥|<|>)\s*\d[\d.,]*")]
    private static partial Regex MeasuredAgainstBound { get; }

    // Commands that only prove the code compiles, restores, formats, or type-checks. Anything that
    // also runs the code (`test`, `run`, an HTTP call) is not compile-only even if a build precedes it.
    [GeneratedRegex(@"\b(dotnet\s+(build|restore|format)|cargo\s+(build|check|fmt)|tsc\b|(npm|pnpm|yarn|bun)\s+(run\s+)?(build|typecheck|type-check|lint)|go\s+(build|vet))",
        RegexOptions.IgnoreCase)]
    private static partial Regex CompileVerb { get; }

    // Toolchain verbs and URL calls execute code. Path tokens (Http, Tests, Bench, Start, Serve)
    // and `pnpm exec tsc` must not match. curl/wget count only as the command, not a path word.
    [GeneratedRegex(@"\b(dotnet|cargo|go)\s+(test|run|bench)\b|\b(npm|pnpm|yarn|bun)\s+test\b|(?:^|&&|;|\|)\s*(curl|wget)\b|https?://",
        RegexOptions.IgnoreCase)]
    private static partial Regex ExecutesCode { get; }

    [GeneratedRegex(@"\*\*Suite:\*\*\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex SuiteLine { get; }

    [GeneratedRegex(@"\*\*Stacks:\*\*\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex StacksLine { get; }

    [GeneratedRegex(@"\*\*Coverage:\*\*\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex CoverageLine { get; }

    [GeneratedRegex(@"\*\*Boundary:\*\*\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex BoundaryLine { get; }

    /// <summary>Reads the contracted verifier-report fields from Markdown.</summary>
    public static VerificationReport Parse(string markdown)
    {
        var criteria = CriterionRow.Matches(markdown)
            .Select(match => Criterion(
                int.Parse(match.Groups[1].Value),
                match.Groups[2].Value.Trim().Trim('`').Trim().ToLowerInvariant(),
                Unwrap(match.Groups[3].Value),
                match.Groups[4].Value.Trim()))
            .ToList();

        var suite = SuiteLine.Match(markdown).Groups[1].Value;
        var widerFailed = ContainsAny(suite, "fail", "failed", "error");
        var origin = ContainsAny(suite, "pre-existing", "preexisting", "already failing")
            ? FailureOrigin.PreExisting
            : ContainsAny(suite, "this-change", "this change", "introduced")
                ? FailureOrigin.ThisChange
                : widerFailed ? FailureOrigin.Unspecified : FailureOrigin.Unspecified;

        var stacks = ParseStacks(StacksLine.Match(markdown).Groups[1].Value);
        var coverage = CoverageLine.Match(markdown).Groups[1].Value;
        var boundary = BoundaryLine.Match(markdown).Groups[1].Value;

        return new VerificationReport(
            criteria,
            WiderSuitePassed: !widerFailed && suite.Length > 0,
            WiderSuiteFailureOrigin: origin,
            StacksVerified: stacks,
            BoundaryVerified: IsAffirmative(boundary) &&
                               !ContainsAny(boundary, "not covered", "uncovered", "skipped"),
            IncludesEdgeCases: coverage.Length > 0 &&
                               !ContainsAny(coverage, "happy-path only", "happy path only"));
    }

    /// <summary>
    /// Applies the verify-path gates. A pass here is the only pass the orchestrator may treat as
    /// verified; prompt wording cannot override it.
    /// </summary>
    public static VerificationOutcome Evaluate(VerificationContext context)
    {
        if (!context.HasConfirmedPlan)
        {
            return VerificationOutcome.StoppedNoPlan;
        }

        if (context.Report is not { } report || report.Criteria.Count == 0)
        {
            return VerificationOutcome.NotPass;
        }

        // A report with no `pass` row proves nothing, even when every row is `skipped`.
        if (!report.Criteria.Any(criterion => criterion.Result == Pass))
        {
            return VerificationOutcome.NotPass;
        }

        foreach (var criterion in report.Criteria)
        {
            // Failed needs: Doctor check: visible, non-blocking. Undriven work is NotRun, not this.
            // A skip without a `needs:` reason is not a sanctioned skip.
            if (criterion.Result == Skipped)
            {
                if (!IsNeedsSkip(criterion.Evidence))
                {
                    return VerificationOutcome.NotPass;
                }

                continue;
            }

            // Unknown Result must not be dropped at parse time; treat as malformed here.
            if (criterion.Result is not (Pass or Fail or NotVerified or Blocked or NotRun))
            {
                return VerificationOutcome.NotPass;
            }

            // A dash, empty cell, or "none" never covers a criterion. Claiming `pass` there is
            // the exact lie this gate exists to catch; `not verified` is honest and still not a pass.
            // `fail`, `not verified`, `blocked`, and `not run` all block a pass.
            if (!IsCoveringCommand(criterion.Command) || criterion.Result != Pass)
            {
                return VerificationOutcome.NotPass;
            }

            // A build or type-check proves the code compiles, nothing about behavior. Only a
            // criterion whose stated outcome is compilation may pass on such a command.
            if (IsCompileOnlyCommand(criterion.Command) &&
                !(context.CompileCriteria ?? []).Contains(criterion.Number))
            {
                return VerificationOutcome.NotPass;
            }
        }

        foreach (var number in context.QuantitativeCriteria ?? [])
        {
            // Only the verifier's own number quoted against the bound is evidence;
            // `2 passed` says nothing about a bound. A skipped needs: row does not block.
            var row = report.Criteria.FirstOrDefault(criterion => criterion.Number == number);
            if (row is null)
            {
                return VerificationOutcome.NotPass;
            }

            if (row.Result == Skipped && IsNeedsSkip(row.Evidence))
            {
                continue;
            }

            if (!IsMeasuredAgainstBound(row.Evidence))
            {
                return VerificationOutcome.NotPass;
            }
        }

        if (!report.WiderSuitePassed)
        {
            return VerificationOutcome.NotPass;
        }

        if (!CoversEveryStack(context.ApplicableStacks, report.StacksVerified))
        {
            return VerificationOutcome.NotPass;
        }

        if (IsMixed(context.ApplicableStacks) && !report.BoundaryVerified)
        {
            return VerificationOutcome.NotPass;
        }

        if (!report.IncludesEdgeCases)
        {
            return VerificationOutcome.NotPass;
        }

        return VerificationOutcome.Pass;
    }

    /// <summary>A command that can actually cover a criterion, not a dash or an empty cell.</summary>
    public static bool IsCoveringCommand(string command)
    {
        var value = Unwrap(command);
        return value.Length > 0 && value != "—" && value != "-" && value != "none";
    }

    /// <summary>
    /// The harness Outcome form <c>skipped (needs: kv, 403)</c> becomes Result <c>skipped</c> with the
    /// <c>needs:</c> reason moved to the front of Evidence, so both stated forms gate the same way.
    /// </summary>
    private static VerificationCriterion Criterion(int number, string result, string command, string evidence)
    {
        if (result.StartsWith("skipped (needs:", StringComparison.Ordinal) && result.EndsWith(')'))
        {
            var reason = result["skipped (".Length..^1].Trim();
            return new VerificationCriterion(number, Skipped, command,
                evidence.Length == 0 ? reason : $"{reason}; {evidence}");
        }

        return new VerificationCriterion(number, result, command, evidence);
    }

    /// <summary>
    /// A sanctioned skip: Evidence starts with <c>needs:</c> (optional backticks allowed).
    /// </summary>
    private static bool IsNeedsSkip(string evidence) =>
        evidence.TrimStart('`', ' ').StartsWith("needs:", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Evidence for a quantitative criterion: the verifier's measured value compared to the bound
    /// (`p95 143 ms ≤ 200 ms`). A pass count or a test name is not a measurement.
    /// </summary>
    public static bool IsMeasuredAgainstBound(string evidence) =>
        MeasuredAgainstBound.IsMatch(Unwrap(evidence));

    /// <summary>
    /// A command that only builds, restores, formats, or type-checks (`dotnet build`, `cargo check`,
    /// `tsc --noEmit`). Not evidence for a behavioral criterion. A chain that also runs the code
    /// (`dotnet build &amp;&amp; dotnet test`) is not compile-only.
    /// </summary>
    public static bool IsCompileOnlyCommand(string command)
    {
        var value = Unwrap(command);
        return CompileVerb.IsMatch(value) && !ExecutesCode.IsMatch(value);
    }

    private static bool CoversEveryStack(IReadOnlyList<string> applicable, IReadOnlyList<string> verified)
    {
        if (applicable.Count == 0)
        {
            return true;
        }

        return applicable.All(stack =>
            verified.Contains(stack, StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsMixed(IReadOnlyList<string> stacks) =>
        stacks.Contains("dotnet", StringComparer.OrdinalIgnoreCase) &&
        stacks.Contains("rust", StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyList<string> ParseStacks(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        if (ContainsAny(value, "both"))
        {
            return ["dotnet", "rust"];
        }

        return value
            .Split([',', ';', '/', '+', '|', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Trim().Trim('`').ToLowerInvariant())
            .Where(part => part is "dotnet" or "rust")
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsAffirmative(string value) =>
        ContainsAny(value, "yes", "covered", "verified", "checked");

    private static string Unwrap(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Length >= 2 && trimmed[0] == '`' && trimmed[^1] == '`')
        {
            return trimmed[1..^1].Trim();
        }

        return trimmed;
    }

    private static bool ContainsAny(string value, params string[] needles) =>
        needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
}
