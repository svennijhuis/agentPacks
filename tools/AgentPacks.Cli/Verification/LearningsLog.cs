using System.Text.RegularExpressions;

namespace AgentPacks.Cli.Verification;

/// <summary>One append-only learnings entry from <c>docs/learnings.md</c>.</summary>
public sealed record LearningsEntry(
    string Date,
    string Entrypoint,
    string ModelTier,
    IReadOnlyList<string> AgentsSpun,
    IReadOnlyList<string> SkippedAgents,
    string Result,
    string NextTweak);

/// <summary>
/// What the next run should do differently. Prior skips that passed are preferred again;
/// prior skips that failed become required. Skills are never rewritten.
/// </summary>
public sealed record LearningsAdvice(
    string? PreferredTier,
    IReadOnlySet<string> PreferSkip,
    IReadOnlySet<string> MustRun,
    string AppliedFrom);

/// <summary>
/// Parses the human-readable learnings log and turns the latest same-entrypoint entry into
/// a gate change. This is the v1 self-improve half: apply notes, do not rewrite skills.
/// </summary>
public static partial class LearningsLog
{
    [GeneratedRegex(@"^##\s+(\d{4}-\d{2}-\d{2})\s+—\s+/?(build|review|squad)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex Heading { get; }

    [GeneratedRegex(@"^-\s+Entrypoint:\s+(\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex EntrypointLine { get; }

    [GeneratedRegex(@"^-\s+Model tier:\s+(\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex TierLine { get; }

    [GeneratedRegex(@"^-\s+Agents spun:\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex SpunLine { get; }

    [GeneratedRegex(@"^-\s+Skipped:\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex SkippedLine { get; }

    [GeneratedRegex(@"^-\s+Result:\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ResultLine { get; }

    [GeneratedRegex(@"^-\s+Next tweak:\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex TweakLine { get; }

    public static IReadOnlyList<LearningsEntry> Parse(string markdown)
    {
        var matches = Heading.Matches(markdown);
        var entries = new List<LearningsEntry>();

        for (var i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : markdown.Length;
            var block = markdown[start..end];
            var headingEntrypoint = CanonicalEntrypoint(matches[i].Groups[2].Value);
            var entrypoint = CanonicalEntrypoint(
                First(EntrypointLine, block) ?? headingEntrypoint);

            entries.Add(new LearningsEntry(
                matches[i].Groups[1].Value,
                entrypoint,
                First(TierLine, block) ?? "inherit",
                Names(First(SpunLine, block)),
                Names(First(SkippedLine, block)),
                (First(ResultLine, block) ?? string.Empty).Trim().ToLowerInvariant(),
                First(TweakLine, block) ?? "None"));
        }

        return entries;
    }

    /// <summary>
    /// Latest entry for this entrypoint drives the next gate. <c>squad</c> and <c>build</c>
    /// are the same entrypoint. A failed skip becomes a must-run; a passed skip is preferred.
    /// </summary>
    public static LearningsAdvice Advise(string markdown, string entrypoint)
    {
        var wanted = CanonicalEntrypoint(entrypoint);
        var latest = Parse(markdown).LastOrDefault(entry =>
            CanonicalEntrypoint(entry.Entrypoint) == wanted);

        if (latest is null)
        {
            return new LearningsAdvice(null, new HashSet<string>(), new HashSet<string>(), "none");
        }

        var failed = latest.Result is "fail" or "failed" or "stopped";
        var preferSkip = new HashSet<string>(StringComparer.Ordinal);
        var mustRun = new HashSet<string>(StringComparer.Ordinal);

        foreach (var agent in latest.SkippedAgents)
        {
            if (failed)
            {
                mustRun.Add(agent);
            }
            else
            {
                preferSkip.Add(agent);
            }
        }

        return new LearningsAdvice(
            latest.ModelTier,
            preferSkip,
            mustRun,
            $"{latest.Date} /{latest.Entrypoint} {latest.Result}");
    }

    public static string CanonicalEntrypoint(string value) =>
        value.Trim().Trim('/').ToLowerInvariant() switch
        {
            "squad" => "build",
            var other => other
        };

    private static string? First(Regex regex, string text)
    {
        var match = regex.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static IReadOnlyList<string> Names(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Equals("None", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return value
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part =>
            {
                var dash = part.IndexOf(" — ", StringComparison.Ordinal);
                return (dash >= 0 ? part[..dash] : part).Trim();
            })
            .Where(part => part.StartsWith("loop-", StringComparison.OrdinalIgnoreCase))
            .Select(part => part.ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
