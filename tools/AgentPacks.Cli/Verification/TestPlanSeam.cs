using System.Text.RegularExpressions;

namespace AgentPacks.Cli.Verification;

/// <summary>One test-plan matrix row, including the pre-agreed Seam.</summary>
public sealed record TestPlanMatrixRow(
    int Criterion,
    string Happy,
    string Edge,
    string Fail,
    string Kind,
    string Seam);

/// <summary>
/// Fail-closed evaluation of the planning-contract Seam column. Prompt text is not evidence:
/// a blank or unconfirmed seam is <c>not verified</c>, and tests that hit internals not named
/// in the column cannot pass.
/// </summary>
public static partial class TestPlanSeam
{
    [GeneratedRegex(
        @"^\|\s*(\d+)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|")]
    private static partial Regex SixColumnRow { get; }

    [GeneratedRegex(
        @"^\|\s*(\d+)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|")]
    private static partial Regex FiveColumnRow { get; }

    [GeneratedRegex(@"^\|\s*(\d+)\s*\|")]
    private static partial Regex NumberedRow { get; }

    /// <summary>
    /// Reads matrix rows. Prefer a six-column Seam cell; a numbered row that drops it
    /// (or a header without Seam) is fail-closed as a blank seam, not omitted.
    /// </summary>
    public static IReadOnlyList<TestPlanMatrixRow> Parse(string markdown)
    {
        var rows = new List<TestPlanMatrixRow>();

        foreach (var raw in markdown.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            var six = SixColumnRow.Match(line);
            if (six.Success)
            {
                rows.Add(Row(six, Unwrap(six.Groups[6].Value)));
                continue;
            }

            var numbered = NumberedRow.Match(line);
            if (!numbered.Success)
            {
                continue;
            }

            var five = FiveColumnRow.Match(line);
            rows.Add(five.Success
                ? Row(five, string.Empty)
                : new TestPlanMatrixRow(
                    int.Parse(numbered.Groups[1].Value),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
        }

        return rows;
    }

    /// <summary>
    /// A named, confirmed test surface — not blank, dash, <c>None</c>/<c>N/A</c>, TBD, or unconfirmed.
    /// Sentinels match ignore-case so the contract empty marker cannot pass.
    /// </summary>
    public static bool IsConfirmed(string seam)
    {
        var value = Unwrap(seam);
        if (value.Length == 0)
        {
            return false;
        }

        if (value.Contains("unconfirmed", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !value.Equals("none", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("n/a", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("tbd", StringComparison.OrdinalIgnoreCase)
            && value is not ("—" or "-" or "–");
    }

    /// <summary>Blank or unconfirmed Seam is <c>not verified</c>, never <c>pass</c>.</summary>
    public static string ResultForSeam(string seam) =>
        IsConfirmed(seam) ? VerificationEvidence.Pass : VerificationEvidence.NotVerified;

    /// <summary>
    /// Applies the Seam gates. Blank/unconfirmed rows are not verified. A test hit that is an
    /// internal and is not named in the Seam column fails the matrix.
    /// </summary>
    public static VerificationOutcome Evaluate(
        string matrixMarkdown,
        IReadOnlyList<string> testHits)
    {
        var rows = Parse(matrixMarkdown);
        if (rows.Count == 0)
        {
            return VerificationOutcome.NotPass;
        }

        foreach (var row in rows)
        {
            if (ResultForSeam(row.Seam) == VerificationEvidence.NotVerified)
            {
                return VerificationOutcome.NotPass;
            }
        }

        var named = rows
            .SelectMany(row => SplitSeams(row.Seam))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var hit in testHits)
        {
            if (IsInternal(hit) && !NamedSeamCovers(named, hit))
            {
                return VerificationOutcome.NotPass;
            }
        }

        return VerificationOutcome.Pass;
    }

    private static TestPlanMatrixRow Row(Match match, string seam) =>
        new(
            int.Parse(match.Groups[1].Value),
            match.Groups[2].Value.Trim(),
            match.Groups[3].Value.Trim(),
            match.Groups[4].Value.Trim(),
            match.Groups[5].Value.Trim(),
            seam);

    private static bool IsInternal(string hit) =>
        hit.Contains("internal", StringComparison.OrdinalIgnoreCase)
        || hit.Contains("private", StringComparison.OrdinalIgnoreCase);

    private static bool NamedSeamCovers(HashSet<string> named, string hit) =>
        named.Any(seam =>
            hit.Equals(seam, StringComparison.OrdinalIgnoreCase)
            || hit.Contains(seam, StringComparison.OrdinalIgnoreCase)
            || seam.Contains(hit, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<string> SplitSeams(string seam) =>
        Unwrap(seam)
            .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Unwrap)
            .Where(part => part.Length > 0);

    private static string Unwrap(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Length >= 2 && trimmed[0] == '`' && trimmed[^1] == '`')
        {
            return trimmed[1..^1].Trim();
        }

        return trimmed;
    }
}
