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

/// <summary>Inputs that are not in the report itself but gate the verdict.</summary>
public sealed record VerificationContext(
    bool HasConfirmedPlan,
    IReadOnlyList<string> ApplicableStacks,
    VerificationReport? Report);

/// <summary>Parses the review-contract verifier report and applies the pass gates.</summary>
public static partial class VerificationEvidence
{
    public const string Pass = "pass";
    public const string Fail = "fail";
    public const string NotVerified = "not verified";

    [GeneratedRegex(@"^\|\s*(\d+)\s*\|\s*(pass|fail|not verified)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex CriterionRow { get; }

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
            .Select(match => new VerificationCriterion(
                int.Parse(match.Groups[1].Value),
                match.Groups[2].Value.Trim().ToLowerInvariant(),
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

        foreach (var criterion in report.Criteria)
        {
            // A dash, empty cell, or "none" never covers a criterion. Claiming `pass` there is
            // the exact lie this gate exists to catch; `not verified` is honest and still not a pass.
            if (!IsCoveringCommand(criterion.Command) || criterion.Result != Pass)
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
